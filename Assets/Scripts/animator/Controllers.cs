using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {

    public struct AnimationController {
        public string name;
        public WeightController? WeightController;
        public CircleController? CircleController;
        public RandomController? RandomController;
    }

    public struct WeightController {

    }

    public struct CircleController {
        public string weightControllerName;
        public bool isClose;
        public List<AnimationInfo> animationInfos;

    }

    public struct RandomController {
        public string weightControllerName;
        public List<int> randomWeights;
        public List<AnimationInfo> animationInfos;
    }

    public class WeightControllerExecutor : IController {
        private Dictionary<string, Playable> animations;

        private AnimationInfo currentInfo;
        private string nextAnimationName;

        private bool isActive;
        public bool isFree;

        public void Process(Playable owner, FrameData info) {

            if (!isActive) {
                return;
            }

            float length = currentInfo.animationLength;
            float duration = currentInfo.transitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)animations[currentInfo.name].GetTime();

            float transitionTime = currentClipTime - startTransitionTime;
            float weight = transitionTime / duration;

            var currentAnim = animations[currentInfo.name];
            var nextAnim = animations[nextAnimationName];

            currentAnim.GetOutput(0).SetInputWeight(currentAnim, 1 - weight);
            nextAnim.GetOutput(0).SetInputWeight(nextAnim, weight);
        }

        public void Setup(Dictionary<string, Playable> animations, string firstAnimation) {
            this.animations = animations;
            animations[firstAnimation].SetTime(0);
        }

        public void ChangeWeight(AnimationInfo currentInfo, string nextAnimationName) {
            this.nextAnimationName = nextAnimationName;
            this.currentInfo = currentInfo;
            isActive = true;
        }

        public void Stop() {
            isActive = false;
            animations[nextAnimationName].SetTime(0);
        }
    }

    public class CircleControllerExecutor : IController {
        private bool isClose;

        private List<AnimationInfo> animationInfos;

        private WeightControllerExecutor weightController;

        private int currentAnimationIndex;
        private int nextAnimationIndex;

        private bool isInTransition;

        private float time;

        public void Setup(
            WeightControllerExecutor weightController,
            List<AnimationInfo> animationInfos,
            bool isClose
            ) {

            this.isClose = isClose;
            this.animationInfos = animationInfos;
            this.weightController = weightController;
            currentAnimationIndex = nextAnimationIndex = 0;
            time = 0;
        }

        public void Process(Playable owner, FrameData info) {
            time += info.deltaTime;

            if (currentAnimationIndex == nextAnimationIndex) {
                nextAnimationIndex = GetNextNode(animationInfos, currentAnimationIndex);
            } else {
                var currentInfo = animationInfos[currentAnimationIndex];

                if (!isInTransition) {
                    if (IsTimeToTransition(time, currentInfo)) {
                        var nextAnimationName = animationInfos[nextAnimationIndex].name;
                        weightController.ChangeWeight(currentInfo, nextAnimationName);
                        isInTransition = true;
                    }
                }

                if (IsEndOfTransition(time, currentInfo)) {
                    currentAnimationIndex = nextAnimationIndex;
                    isInTransition = false;
                    time = 0;
                    weightController.Stop();
                }
            }
        }

        private int GetNextNode(List<AnimationInfo> nodes, int currentIndex) {

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

        private bool IsTimeToTransition(float time, AnimationInfo info) {

            if (time >= info.animationLength - info.transitionDuration) {
                return true;
            }

            return false;
        }

        public bool IsEndOfTransition(float time, AnimationInfo info) {

            if (time >= info.animationLength) {
                return true;
            }

            return false;
        }
    }

    public class RandomControllerExecutor : IController {
        private List<int> randomWeights;

        private List<AnimationInfo> animationInfos;

        private WeightControllerExecutor weightController;

        private int currentAnimationIndex;
        private int nextAnimationIndex;

        private bool isInTransition;

        private float time;

        public void Setup(
            WeightControllerExecutor weightController,
            List<AnimationInfo> animationInfos,
            List<int> randomWeights
            ) {

            this.randomWeights = randomWeights;
            this.animationInfos = animationInfos;
            this.weightController = weightController;
            currentAnimationIndex = nextAnimationIndex = 0;
            time = 0;

        }
        public void Process(Playable owner, FrameData info) {

            time += info.deltaTime;

            if (currentAnimationIndex == nextAnimationIndex) {
                nextAnimationIndex = GetNextNode(currentAnimationIndex,randomWeights);
            } else {
                var currentInfo = animationInfos[currentAnimationIndex];

                if (!isInTransition) {
                    if (IsTimeToTransition(time, currentInfo)) {
                        var nextAnimationName = animationInfos[nextAnimationIndex].name;
                        weightController.ChangeWeight(currentInfo, nextAnimationName);
                        isInTransition = true;
                    }
                }

                if (IsEndOfTransition(time, currentInfo)) {
                    currentAnimationIndex = nextAnimationIndex;
                    isInTransition = false;
                    time = 0;
                    weightController.Stop();
                }
            }
        }
        private int GetNextNode(int currentIndex, List<int> randomWeights) {

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

        private bool IsTimeToTransition(float time, AnimationInfo info) {

            if (time >= info.animationLength - info.transitionDuration) {
                return true;
            }

            return false;
        }

        public bool IsEndOfTransition(float time, AnimationInfo info) {

            if (time >= info.animationLength) {
                return true;
            }

            return false;
        }
    }
}