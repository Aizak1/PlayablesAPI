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

        private LinkedListNode<AnimationClipPlayable> currentAnimationNode;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;

        private LinkedList<AnimationClipPlayable> animationList;

        private float nextTransitionTime;
        private float currentAnimationEndTime;

        private const float WEIGHT_ACCURANCY = 0.01f;

        private void Start() {
            animationList = new LinkedList<AnimationClipPlayable>();
            graph = PlayableGraph.Create();
            graph.Play();
            mixer = AnimationMixerPlayable.Create(graph);

            string[] positionsInSequence = Sequence.Split('-');
            foreach (var item in positionsInSequence) {
                var animationClip = AnimationClips[int.Parse(item) - 1];
                var playableClip = AnimationClipPlayable.Create(graph, animationClip);
                animationList.AddLast(playableClip);
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
            if (Time.time >= nextTransitionTime && Time.time <= currentAnimationEndTime
                && currentAnimationNode.Next != null) {
                float weight = (Time.time - nextTransitionTime)
                        / (currentAnimationEndTime - nextTransitionTime);

                SpreadWeight(weight, currentAnimationNode);
            }
        }

        private void SpreadWeight(float weight, LinkedListNode<AnimationClipPlayable> element) {
            mixer.SetInputWeight(element.Value, 1 - weight);
            mixer.SetInputWeight(element.Next.Value, weight);
            if (1 - weight <= WEIGHT_ACCURANCY) {
                ChangeAnimation();
            }
        }
        private void ChangeAnimation() {
            currentAnimationNode = currentAnimationNode.Next;
            mixer.SetInputWeight(currentAnimationNode.Value, 1);
            SetNewTransitionTime();
        }

        private void SetNewTransitionTime() {
            nextTransitionTime = Time.time +
                (float)currentAnimationNode.Value.GetAnimationClip().length *
                StartTransitionMultiplier;

            currentAnimationEndTime = Time.time +
                (float)currentAnimationNode.Value.GetAnimationClip().length;
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}