using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {

    public struct AnimationController {
        public string name;
        public WeightController? weightController;
    }

    public struct WeightController {
        public CircleController? circleController;
        public RandomController? randomController;

        public List<string> animationNames;
    }

    public struct CircleController {
        public bool isClose;
    }

    public struct RandomController {
        public List<int> randomWeights;
    }

    public struct WeightControllerExecutor : IController {
        public IAdditionalController additionalControllerExecuter;
        public List<ClipNode> animationNodes;

        private int currentAnimationIndex;
        private int nextAnimationIndex;

        public void ProcessLogic(Playable owner, FrameData info) {
            if (nextAnimationIndex == currentAnimationIndex && animationNodes.Count != 1) {

                var nodes = animationNodes;
                var currentIndex = currentAnimationIndex;
                nextAnimationIndex = additionalControllerExecuter.GetNextNode(nodes, currentIndex);


            } else {
                var current = animationNodes[currentAnimationIndex];

                if (!IsTimeToMakeTransition(current)) {
                    return;
                }

                var next = animationNodes[nextAnimationIndex];

                MakeTransition(current, next);

                if (!IsEndOfTransition(current)) {
                    return;
                }

                currentAnimationIndex = nextAnimationIndex;
                current = animationNodes[currentAnimationIndex];
                current.PlayableClip.SetTime(0);
            }
        }

        private bool IsEndOfTransition(ClipNode current) {
            float length = current.AnimationLength;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            if (currentClipTime >= length) {
                return true;
            }

            return false;
        }

        private void MakeTransition(ClipNode current, ClipNode next) {
            float length = current.AnimationLength;
            float duration = current.TransitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            float transitionTime = currentClipTime - startTransitionTime;
            float weight = transitionTime / duration;

            SpreadWeight(weight, current, next);
        }

        private void SpreadWeight(float weight, ClipNode current, ClipNode next) {

            if (current.Parent.inputParent.IsNull()) {
                return;
            }

            current.Parent.inputParent.SetInputWeight(current.PlayableClip, 1 - weight);
            next.Parent.inputParent.SetInputWeight(next.PlayableClip, weight);
        }

        public bool IsTimeToMakeTransition(ClipNode current) {
            float length = current.AnimationLength;
            float duration = current.TransitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            if (currentClipTime >= startTransitionTime) {
                return true;
            }

            return false;
        }
    }

    public struct CircleControllerExecutor : IAdditionalController {
        public bool isClose;

        public int GetNextNode(List<ClipNode> nodes, int currentIndex) {

            int nextIndex = currentIndex + 1;

            if (nextIndex < nodes.Count) {
                return nextIndex;
            }

            if (isClose) {
                return 0;

            } else {
                return currentIndex;
            }
        }
    }

    public struct RandomControllerExecutor : IAdditionalController {
        public List<int> randomWeights;

        public int GetNextNode(List<ClipNode> nodes, int currentIndex) {

            int nextIndex = 0;
            int sum = 0;
            for (int i = 0; i < randomWeights.Count; i++) {
                if (i == currentIndex) {
                    continue;
                }
                sum += randomWeights[i];
            }

            int randomNumber = Random.Range(0, sum + 1);
            int randomWeight = 0;

            for (int i = 0; i < randomWeights.Count; i++) {
                if (i == currentIndex) {
                    continue;
                }
                randomWeight += randomWeights[i];
                if (randomNumber < randomWeight) {
                    nextIndex = i;
                    break;
                }
            }

            return nextIndex;
        }
    }
}
