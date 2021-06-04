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

    public struct WeightControllerExecutor : IController {
        private Playable currentAnimation;
        private Playable nextAnimation;

        private AnimationInfo currentInfo;

        private bool isActive;
        public bool isFree;

        public void Process(Playable owner, FrameData info) {

            if (!isActive) {
                return;
            }

            float length = currentInfo.animationLength;
            float duration = currentInfo.transitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)currentAnimation.GetTime();

            float transitionTime = currentClipTime - startTransitionTime;
            float weight = transitionTime / duration;

            currentAnimation.GetOutput(0).SetInputWeight(currentAnimation, 1 - weight);
            nextAnimation.GetOutput(0).SetInputWeight(nextAnimation, weight);
        }

        public void Setup(
            Playable currentAnimation,
            Playable nextAnimation,
            AnimationInfo currentInfo
            ) {

            this.currentAnimation = currentAnimation;
            this.nextAnimation = nextAnimation;
            this.currentInfo = currentInfo;
            isActive = true;
        }

        public void Reset() {
            isActive = false;
        }
    }

    public struct CircleControllerExecutor : IController {
        private bool isClose;

        private Dictionary<string, Playable> animations;
        private List<AnimationInfo> animationInfos;

        private WeightControllerExecutor weightController;

        private int currentAnimationIndex;
        private int nextAnimationIndex;

        private bool isInTransition;

        public void Setup(
            WeightControllerExecutor weightController,
            List<AnimationInfo> animationInfos,
            bool isClose,
            Dictionary<string, Playable> animations
            ) {

            this.isClose = isClose;
            this.animationInfos = animationInfos;
            this.weightController = weightController;
            this.animations = animations;
            currentAnimationIndex = nextAnimationIndex = 0;
            animations[animationInfos[0].name].SetTime(0);
        }

        public void Process(Playable owner, FrameData info) {

            if (currentAnimationIndex == nextAnimationIndex) {
                nextAnimationIndex = GetNextNode(animationInfos, currentAnimationIndex);
            } else {
                var animationInfo = animationInfos[currentAnimationIndex];
                var animation = animations[animationInfo.name];
                if (IsTimeToTransition(animation, animationInfo)) {
                    if (!isInTransition) {
                        var nextAnimation = animations[animationInfos[nextAnimationIndex].name];
                        weightController.Setup(animation, nextAnimation, animationInfo);

                        isInTransition = true;
                    }
                    weightController.Process(owner, info);

                    if (IsEndOfAnimation(animation, animationInfo)) {
                        currentAnimationIndex = nextAnimationIndex;
                        animations[animationInfos[currentAnimationIndex].name].SetTime(0);
                        isInTransition = false;
                        weightController.Reset();
                    }
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

        private bool IsTimeToTransition(Playable animation, AnimationInfo info) {

            if (animation.GetTime() >= info.animationLength - info.transitionDuration) {
                return true;
            }

            return false;
        }

        public bool IsEndOfAnimation(Playable animation, AnimationInfo info) {

            if (animation.GetTime() >= info.animationLength) {
                return true;
            }

            return false;
        }
    }

    public struct RandomControllerExecutor : IController {
        private List<int> randomWeights;

        private Dictionary<string, Playable> animations;
        private List<AnimationInfo> animationInfos;

        private WeightControllerExecutor weightController;

        private int currentAnimationIndex;
        private int nextAnimationIndex;

        private bool isInTransition;

        public void Setup(
            WeightControllerExecutor weightController,
            List<AnimationInfo> animationInfos,
            List<int> randomWeights,
            Dictionary<string, Playable> animations
            ) {

            this.randomWeights = randomWeights;
            this.animationInfos = animationInfos;
            this.weightController = weightController;
            this.animations = animations;
            currentAnimationIndex = nextAnimationIndex = 0;
            animations[animationInfos[0].name].SetTime(0);
        }
        public void Process(Playable owner, FrameData info) {

            if (currentAnimationIndex == nextAnimationIndex) {
                nextAnimationIndex = GetNextNode(currentAnimationIndex, randomWeights);
            }

            var animationInfo = animationInfos[currentAnimationIndex];
            var animation = animations[animationInfo.name];

            if (IsTimeToTransition(animation, animationInfo)) {
                if (!isInTransition) {
                    var nextAnimation = animations[animationInfos[nextAnimationIndex].name];
                    weightController.Setup(animation, nextAnimation, animationInfo);

                    isInTransition = true;
                }
                weightController.Process(owner, info);

                if (IsEndOfAnimation(animation, animationInfo)) {
                    currentAnimationIndex = nextAnimationIndex;
                    animations[animationInfos[currentAnimationIndex].name].SetTime(0);
                    isInTransition = false;
                    weightController.Reset();
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

        private bool IsTimeToTransition(Playable animation, AnimationInfo info) {

            if (animation.GetTime() >= info.animationLength - info.transitionDuration) {
                return true;
            }

            return false;
        }

        public bool IsEndOfAnimation(Playable animation, AnimationInfo info) {

            if (animation.GetTime() >= info.animationLength) {
                return true;
            }

            return false;
        }
    }
}