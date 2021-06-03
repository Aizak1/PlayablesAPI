using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {

    public struct AnimationController {
        public string name;
        public WeightController? WeightController;
    }

    public struct WeightController {
        public CircleController? CircleController;
        public RandomController? RandomController;

        public List<string> animationNames;
    }

    public struct CircleController {
        public bool isClose;
    }

    public struct RandomController {
        public List<int> randomWeights;
    }

    public struct WeightControllerExecutor : IController {
        public IAdditionalController executor;
        public List<ClipNodeInfo> clipNodesInfo;
        private bool isEnabled;

        private int currentAnimationIndex;
        private int nextAnimationIndex;

        public void ProcessLogic(Playable owner, FrameData info) {

            if (!isEnabled) {
                return;
            }

            if (nextAnimationIndex == currentAnimationIndex && clipNodesInfo.Count != 1) {

                var nodes = clipNodesInfo;
                var currentIndex = currentAnimationIndex;
                nextAnimationIndex = executor.GetNextNode(nodes, currentIndex);


            } else {
                var current = clipNodesInfo[currentAnimationIndex];

                if (!IsTimeToMakeTransition(current)) {
                    return;
                }

                var next = clipNodesInfo[nextAnimationIndex];

                MakeTransition(current, next);

                if (!IsEndOfTransition(current)) {
                    return;
                }

                currentAnimationIndex = nextAnimationIndex;
                current = clipNodesInfo[currentAnimationIndex];
                current.playableClip.SetTime(0);
            }
        }

        private bool IsEndOfTransition(ClipNodeInfo current) {
            float length = current.animationLength;
            float currentClipTime = (float)current.playableClip.GetTime();

            if (currentClipTime >= length) {
                return true;
            }

            return false;
        }

        private void MakeTransition(ClipNodeInfo current, ClipNodeInfo next) {
            float length = current.animationLength;
            float duration = current.transitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.playableClip.GetTime();

            float transitionTime = currentClipTime - startTransitionTime;
            float weight = transitionTime / duration;

            SpreadWeight(weight, current, next);
        }

        private void SpreadWeight(float weight, ClipNodeInfo current, ClipNodeInfo next) {

            if (!current.parent.input.HasValue) {
                return;
            }

            current.parent.input.Value.SetInputWeight(current.playableClip, 1 - weight);
            next.parent.input.Value.SetInputWeight(next.playableClip, weight);
        }

        public bool IsTimeToMakeTransition(ClipNodeInfo current) {
            float length = current.animationLength;
            float duration = current.transitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.playableClip.GetTime();

            if (currentClipTime >= startTransitionTime) {
                return true;
            }

            return false;
        }

        public void Enable() {
            isEnabled = true;
            foreach (var item in clipNodesInfo) {
                item.parent.input.Value.SetInputWeight(item.playableClip, 0);
                item.playableClip.SetTime(item.animationLength);
            }
            nextAnimationIndex = currentAnimationIndex = 0;
            clipNodesInfo[0].playableClip.SetTime(0);
            clipNodesInfo[0].parent.input.Value.SetInputWeight(clipNodesInfo[0].playableClip, 1);
        }

        public void Disable() {
            isEnabled = false;
            foreach (var item in clipNodesInfo) {
                item.parent.input.Value.SetInputWeight(item.playableClip, 0);
                item.playableClip.SetTime(item.animationLength);
            }
        }
    }



    public struct CircleControllerExecutor : IAdditionalController {
        public bool isClose;

        public int GetNextNode(List<ClipNodeInfo> nodes, int currentIndex) {

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

        public int GetNextNode(List<ClipNodeInfo> nodes, int currentIndex) {

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
