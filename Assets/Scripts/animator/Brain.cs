using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public class Brain : PlayableBehaviour {

        public Dictionary<string, AnimationController> AnimControllers;
        public List<string> ControllerNames;
        private int currentControllerIndex;

        public void Initialize() {

            AnimControllers = new Dictionary<string, AnimationController>();
            ControllerNames = new List<string>();
            currentControllerIndex = 0;
        }

        override public void PrepareFrame(Playable owner, FrameData info) {

            foreach (var item in ControllerNames) {
                ProcessController(item);
            }

        }

        public void ActivateControllers(string[] names) {
            foreach (var name in ControllerNames) {
                DeactivateController(name);
            }

            foreach (var name in names) {
                if (AnimControllers.ContainsKey(name)) {
                    ActivateController(name);
                }
            }
        }

        public void SwitchController(int newIndex) {
            if (ControllerNames.Count == 0) {
                return;
            }

            if (newIndex >= ControllerNames.Count) {
                newIndex = 0;
            } else if (newIndex < 0) {
                newIndex = ControllerNames.Count - 1;
            }

            DeactivateController(ControllerNames[currentControllerIndex]);
            ActivateController(ControllerNames[newIndex]);

            currentControllerIndex = newIndex;
        }

        private void DeactivateController(string name) {
            var controller = AnimControllers[name];

            if (!controller.isEnable) {
                return;
            }

            controller.isEnable = false;
            controller.CurrentAnimationIndex = 0;
            controller.NextAnimationIndex = 0;
            foreach (var item in controller.PlayableAnimations) {
                item.PlayableClip.SetTime(item.PlayableClip.GetAnimationClip().length);
                item.Parent.inputParent.SetInputWeight(item.PlayableClip, 0);
            }
            AnimControllers[name] = controller;
        }

        private void ActivateController(string name) {
            var controller = AnimControllers[name];

            if (controller.isEnable) {
                return;
            }

            controller.isEnable = true;

            var firstAnim = controller.PlayableAnimations[0];
            firstAnim.PlayableClip.SetTime(0);
            firstAnim.Parent.inputParent.SetInputWeight(firstAnim.PlayableClip, 1);
            AnimControllers[name] = controller;
        }

        private void ProcessController(string name) {
            var controller = AnimControllers[name];

            if (!controller.isEnable) {
                return;
            }

            if (controller.NextAnimationIndex >= controller.PlayableAnimations.Count
                || (controller.NextAnimationIndex == controller.CurrentAnimationIndex
                && controller.PlayableAnimations.Count != 1)){
                controller.NextAnimationIndex = GetNextAnimationIndex(controller);

            } else {
                var current = controller.PlayableAnimations[controller.CurrentAnimationIndex];

                if (!IsTimeToMakeTransition(current)) {
                    return;
                }

                var next = controller.PlayableAnimations[controller.NextAnimationIndex];

                MakeTransition(current, next);

                if (!IsEndOfTransition(current)) {
                    return;
                }

                controller.CurrentAnimationIndex = controller.NextAnimationIndex;
                current = controller.PlayableAnimations[controller.CurrentAnimationIndex];
                current.PlayableClip.SetTime(0);
            }
            AnimControllers[name] = controller;
        }

        private bool IsEndOfTransition(PlayableAnimation current) {
            float length = current.AnimationLength;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            if (currentClipTime >= length) {
                return true;
            }

            return false;
        }

        private void MakeTransition(PlayableAnimation current, PlayableAnimation next) {
            float length = current.AnimationLength;
            float duration = current.TransitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            float transitionTime = currentClipTime - startTransitionTime;
            float weight = transitionTime / duration;

            SpreadWeight(weight, current, next);
        }

        private void SpreadWeight(
            float weight,
            PlayableAnimation current,
            PlayableAnimation next
            ) {
            if (current.Parent.inputParent.IsNull()) {
                return;
            }
            current.Parent.inputParent.SetInputWeight(current.PlayableClip, 1 - weight);
            next.Parent.inputParent.SetInputWeight(next.PlayableClip, weight);
        }

        public bool IsTimeToMakeTransition(PlayableAnimation current) {
            float length = current.AnimationLength;
            float duration = current.TransitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            if (currentClipTime >= startTransitionTime) {
                return true;
            }

            return false;
        }

        private int GetNextAnimationIndex(AnimationController controller) {
            int nextIndex = 0;
            switch (controller.ControllerType) {
                case ControllerType.OpenCircle:
                    nextIndex = controller.CurrentAnimationIndex + 1;
                    break;
                case ControllerType.CloseCircle:
                    int currentAnimationIndex = controller.CurrentAnimationIndex;
                    int animationsCount = controller.PlayableAnimations.Count;
                    if (currentAnimationIndex + 1 >= animationsCount) {
                        nextIndex = 0;
                    } else {
                        nextIndex = controller.CurrentAnimationIndex + 1;
                    }
                    break;
                case ControllerType.Random:
                    nextIndex = GetRandomAnimationIndex(controller);
                    break;
            }

            return nextIndex;
        }

        private int GetRandomAnimationIndex(AnimationController controller) {
            int nextIndex = 0;
            int sum = 0;
            for (int i = 0; i < controller.RandomWeights.Count; i++) {
                if (i == controller.CurrentAnimationIndex) {
                    continue;
                }
                sum += controller.RandomWeights[i];
            }

            int randomNumber = Random.Range(0, sum + 1);
            int randomWeight = 0;

            for (int i = 0; i < controller.RandomWeights.Count; i++) {
                if (i == controller.CurrentAnimationIndex) {
                    continue;
                }
                randomWeight += controller.RandomWeights[i];
                if (randomNumber < randomWeight) {
                    nextIndex = i;
                    break;
                }
            }

            return nextIndex;
        }
    }
}