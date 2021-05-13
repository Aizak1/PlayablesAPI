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
        public int[] Sequence;
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
                ShowException("AnimationClips list is empty");
            }

            if (Sequence == null || Sequence.Length == 0) {
                foreach (var item in AnimationClips) {
                    animationList.AddLast(AnimationClipPlayable.Create(graph, item));
                }
            } else {
                foreach (var item in Sequence) {
                    int animationPos = item - 1;
                    if (animationPos >= AnimationClips.Count) {
                        ShowException("Invalid sequence");
                    }
                    var animationClip = AnimationClips[animationPos];
                    animationList.AddLast(AnimationClipPlayable.Create(graph, animationClip));
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

                if (Time.time > currentAnimationEndTime) {
                    ChangeAnimation(currentAnimationNode, nextAnimationNode);
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

        private void ChangeAnimation(LinkedListNode<AnimationClipPlayable> currentElenemt,
            LinkedListNode<AnimationClipPlayable> nextElenemt) {
            mixer.SetInputWeight(currentAnimationNode.Value, 0);

            currentAnimationNode = nextElenemt;
            mixer.SetInputWeight(currentAnimationNode.Value, 1);
            currentAnimationNode.Value.SetTime(0);

            SetNewTransitionTime();
        }

        private void SetNewTransitionTime() {
            nextTransitionTime = Time.time +
                currentAnimationNode.Value.GetAnimationClip().length *
                StartTransitionMultiplier;

            currentAnimationEndTime = Time.time +
                currentAnimationNode.Value.GetAnimationClip().length;
        }

        private void ShowException(string exceptionText) {
            enabled = false;
            Debug.LogError(exceptionText);
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}