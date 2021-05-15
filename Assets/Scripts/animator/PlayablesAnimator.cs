using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesAnimator : MonoBehaviour {

        [SerializeField]
        private Resource resource;

        public GraphData graphData;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;

        private PlayableNodeList animationList;
        private PlayableNode currentNode;

        private float startTransitionTime;

        private int source;

        public bool isLooping;

        private void Start() {
            graph = PlayableGraph.Create();
            mixer = AnimationMixerPlayable.Create(graph);
            AnimationPlayableOutput animOutput =
                AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());

            source = animOutput.GetSourceOutputPort();
            animOutput.SetSourcePlayable(mixer);

            animationList = CreateAnimationList(graphData, resource);

            foreach (var item in animationList) {
                mixer.AddInput(item.PlayableClip, source);
            }

            if (isLooping) {
                animationList.Tail.Next = animationList.Head;
            }

            currentNode = animationList.Head;
            mixer.SetInputWeight(currentNode.PlayableClip, 1);
            currentNode.PlayableClip.SetTime(0);

            var duration = currentNode.TransitionDuration;
            var length = currentNode.PlayableClip.GetAnimationClip().length;
            startTransitionTime = CalculateTransitionStartTime(length, duration);

            graph.Play();
        }

        private PlayableNodeList CreateAnimationList(GraphData graphData, Resource resource) {
            PlayableNodeList animationList = new PlayableNodeList();
            foreach (var item in graphData.inputNodes) {
                var clip = resource.animationPairs[item.animation];
                var playableClip = AnimationClipPlayable.Create(graph, clip);
                playableClip.SetTime(clip.length);
                animationList.Add(playableClip, item.transitionDuration);
            }
            return animationList;
        }

        private void Update() {
            float time = (float)currentNode.PlayableClip.GetTime();
            if (time >= startTransitionTime) {
                var nextNode = CalculateNextAnimationNode(currentNode);
                if (nextNode == null) {
                    return;
                }

                if (time > startTransitionTime + currentNode.TransitionDuration) {
                    currentNode = MoveOnNextNode(currentNode, nextNode);

                    var duration = currentNode.TransitionDuration;
                    var length = currentNode.PlayableClip.GetAnimationClip().length;
                    startTransitionTime = CalculateTransitionStartTime(length, duration);
                    currentNode.PlayableClip.SetTime(0);
                    return;
                }

                float weight = (time - startTransitionTime) / currentNode.TransitionDuration;

                SpreadWeight(weight, currentNode, nextNode);
            }
        }

        private PlayableNode CalculateNextAnimationNode(PlayableNode currentNode) {
            if (currentNode.Next != null) {
                return currentNode.Next;
            }
            return null;
        }

        private void SpreadWeight(float weight, PlayableNode currentNode, PlayableNode nextNode) {
            mixer.SetInputWeight(currentNode.PlayableClip, 1 - weight);
            mixer.SetInputWeight(nextNode.PlayableClip, weight);
        }

        private PlayableNode MoveOnNextNode(PlayableNode currentNode, PlayableNode nextNode) {
            mixer.SetInputWeight(currentNode.PlayableClip, 0);
            mixer.SetInputWeight(nextNode.PlayableClip, 1);

            return nextNode;
        }

        private float CalculateTransitionStartTime(float length, float transitionDuration) {
            return length - transitionDuration;
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}