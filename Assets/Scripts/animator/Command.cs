using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {

    [System.Serializable]
    public struct Command {
        public AddInputCommand? AddInput;
        public AddControllerCommand? AddContoller;
    }

    [System.Serializable]
    public struct AddInputCommand {
        public AnimationInput? AnimationInput;
    }

    [System.Serializable]
    public struct AddControllerCommand {
        public AnimationController Controller;
    }

    [System.Serializable]
    public struct AnimationInput {
        public string Parent;
        public AnimationClipInput? AnimationClip;
        public AnimationMixerInput? AnimationMixer;
        public AnimationLayerMixerInput? AnimationLayerMixer;
    }

    [System.Serializable]
    public struct AnimationClipInput {
        public string Name;
        public float TransitionDuration;
    }

    [System.Serializable]
    public struct AnimationMixerInput {
        public string Name;
    }

    [System.Serializable]
    public struct AnimationLayerMixerInput {
        public string Name;
    }

    [System.Serializable]
    public struct AnimationController {
        public OpenCircleController? OpenCircle;
        public CloseCircleController? CloseCircle;
        public RandomController? Random;
    }

    [System.Serializable]
    public struct OpenCircleController {
        public WeightController WeightController;

        public void Play(PlayableNode current) {
            if (WeightController.IsTimeToMakeTransition(current)) {
                WeightController.MakeTransition(current, current.Next);
            }
            if (WeightController.IsEndOfTransition(current) && current.Next != null) {
                current = current.Next;
            }
        }
    }

    [System.Serializable]
    public struct CloseCircleController {
        public WeightController WeightController;
    }

    [System.Serializable]
    public struct RandomController {
        public WeightController WeightController;
        public int[] Weights;
    }

    [System.Serializable]
    public struct WeightController {
        private void SpreadWeight(float weight, PlayableNode current, PlayableNode next) {
            current.Parent.SetInputWeight(current.PlayableClip, 1 - weight);
            next.Parent.SetInputWeight(next.PlayableClip, weight);
        }

        public void MakeTransition(PlayableNode current, PlayableNode next) {
            float length = current.PlayableClip.GetAnimationClip().length;
            float duration = current.TransitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            float transitionTime = currentClipTime - startTransitionTime;
            float weight = transitionTime / duration;

            SpreadWeight(weight, current, next);
        }

        public bool IsTimeToMakeTransition(PlayableNode current) {
            float length = current.PlayableClip.GetAnimationClip().length;
            float duration = current.TransitionDuration;
            float startTransitionTime = length - duration;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            if (currentClipTime >= startTransitionTime) {
                return true;
            }

            return false;
        }

        public bool IsEndOfTransition(PlayableNode current) {
            float length = current.PlayableClip.GetAnimationClip().length;
            float currentClipTime = (float)current.PlayableClip.GetTime();

            if (currentClipTime >= length) {
                return true;
            }

            return false;
        }
    }


}


