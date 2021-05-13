using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.IO;

namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesComponent : MonoBehaviour {
        [SerializeField]
        public List<AnimationClip> AnimationClips;
        [SerializeField]
        public string Sequence;
        [SerializeField]
        public float StartTransitionMultiplier;
        [SerializeField]
        public bool IsLooping;

        private LinkedListNode<AnimationClipPlayable> currentAnimationNode;
        private LinkedListNode<AnimationClipPlayable> nextAnimationNode;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;

        private LinkedList<AnimationClipPlayable> animationList;

        private float nextTransitionTime = 0;
        private float currentAnimationEndTime = 0;

        private void Start() {
            animationList = new LinkedList<AnimationClipPlayable>();
            graph = PlayableGraph.Create();
            graph.Play();
            mixer = AnimationMixerPlayable.Create(graph);

            if (AnimationClips.Count == 0) {
                enabled = false;
                throw new UnityException("AnimationClips list is empty");
            }

            if (Sequence == null || Sequence.Length == 0) {
                foreach (var item in AnimationClips) {
                    var playableClip = AnimationClipPlayable.Create(graph, item);
                    playableClip.Pause();
                    animationList.AddLast(playableClip);
                }
            } else {
                string[] positionsInSequence = Sequence.Split('-');
                foreach (var item in positionsInSequence) {
                    int animationPos = int.Parse(item) - 1;
                    if (animationPos >= AnimationClips.Count) {
                        enabled = false;
                        throw new UnityException("Invalid sequence");
                    }
                    var animationClip = AnimationClips[animationPos];
                    var playableClip = AnimationClipPlayable.Create(graph, animationClip);
                    playableClip.Pause();
                    animationList.AddLast(playableClip);
                }
            }

            if (StartTransitionMultiplier == 0) {
                StartTransitionMultiplier = 1;
            }

            AnimationPlayableOutput animOutput =
                AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());
            var source = animOutput.GetSourceOutputPort();

            foreach (var item in animationList) {
                mixer.AddInput(item, source);
            }

            animOutput.SetSourcePlayable(mixer);

            currentAnimationNode = animationList.First;
            mixer.SetInputWeight(currentAnimationNode.Value, 1);
            currentAnimationNode.Value.Play();
            SetNewTransitionTime();
        }

        private void Update() {
            if (Time.time >= nextTransitionTime) {
                if (currentAnimationNode.Next == null) {
                    if (IsLooping) {
                        nextAnimationNode = animationList.First;
                    } else {
                        return;
                    }
                } else {
                    nextAnimationNode = currentAnimationNode.Next;
                }
                if (StartTransitionMultiplier == 1f) {
                    mixer.SetInputWeight(currentAnimationNode.Value, 0);
                    ChangeAnimation(nextAnimationNode);
                    return;
                }

                if (Time.time > currentAnimationEndTime) {
                    ChangeAnimation(nextAnimationNode);
                    return;
                }

                float weight = (Time.time - nextTransitionTime)
                        / (currentAnimationEndTime - nextTransitionTime);

                SpreadWeight(weight, currentAnimationNode, nextAnimationNode);
            }
        }

        private void SpreadWeight(float weight,
            LinkedListNode<AnimationClipPlayable> currentElement,
            LinkedListNode<AnimationClipPlayable> nextElenemt
            ){
            mixer.SetInputWeight(currentElement.Value, 1 - weight);
            mixer.SetInputWeight(nextElenemt.Value, weight);
        }

        private void ChangeAnimation(LinkedListNode<AnimationClipPlayable> nextElenemt) {
            currentAnimationNode.Value.SetTime(0);
            currentAnimationNode.Value.Pause();
            currentAnimationNode = nextElenemt;
            mixer.SetInputWeight(currentAnimationNode.Value, 1);
            currentAnimationNode.Value.Play();
            SetNewTransitionTime();
        }

        private void SetNewTransitionTime() {
            nextTransitionTime = Time.time +
                currentAnimationNode.Value.GetAnimationClip().length *
                StartTransitionMultiplier;

            currentAnimationEndTime = Time.time +
                currentAnimationNode.Value.GetAnimationClip().length;
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}