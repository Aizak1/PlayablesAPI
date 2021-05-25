using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


namespace animator {
    public struct AnimationController {
        public OpenCircleController? OpenCircle;
        public CloseCircleController? CloseCircle;
        public RandomController? Random;
    }

    public struct OpenCircleController {
        public WeightController WeightController;

        public PlayableNode UpdateNodeState(PlayableNode current) {
            if (current.Next == null) {
                return null;
            }

            if (WeightController.IsEndOfTransition(current)) {
                current = WeightController.MoveOnNextNode(current);

            } else if (WeightController.IsTimeToMakeTransition(current)) {
                WeightController.MakeTransition(current, current.Next);
            }

            return current;
        }
    }

    public struct CloseCircleController {
        public WeightController WeightController;

        public PlayableNode UpdateNodeState(PlayableNode current, PlayableNode root) {
            if (current.Next == null) {
                current.Next = root;
            }

            if (WeightController.IsEndOfTransition(current)) {
                current = WeightController.MoveOnNextNode(current);

            } else if (WeightController.IsTimeToMakeTransition(current)) {
                WeightController.MakeTransition(current, current.Next);
            }

            return current;
        }
    }

    public struct RandomController {
        public WeightController WeightController;
        public List<int> Weights;

        public PlayableNode UpdateNodeState(PlayableNode current, PlayableNode next) {
            if (WeightController.IsTimeToMakeTransition(current)) {
                WeightController.MakeTransition(current, next);

                if (WeightController.IsEndOfTransition(current)) {
                    current = WeightController.MoveOnNextNode(current, next);
                }
            }

            return current;
        }

        public PlayableNode GetRandomNode(PlayableNode current, PlayableNode root) {
            int sum = 0;
            PlayableNode node = new PlayableNode {
                Next = root.Next,
                Parent = root.Parent,
                PlayableClip = root.PlayableClip,
                TransitionDuration = root.TransitionDuration
            };

            for (int i = 0; i < Weights.Count; i++) {
                sum += Weights[i];
            }

            int randomNumber = Random.Range(0, sum + 1);
            int randomWeight = 0;

            for (int i = 0; i < Weights.Count; i++) {
                randomWeight += Weights[i];
                if (randomNumber < randomWeight) {
                    break;
                }
                node = node.Next;
            }
            return node;
        }
    }

    public struct WeightController {

        private void SpreadWeight(float weight, PlayableNode current, PlayableNode next) {
            if (current.Parent.inputParent.IsNull()) {
                return;
            }
            current.Parent.inputParent.SetInputWeight(current.PlayableClip, 1 - weight);
            next.Parent.inputParent.SetInputWeight(next.PlayableClip, weight);
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

        public PlayableNode MoveOnNextNode(PlayableNode current) {
            current = current.Next;
            current.PlayableClip.SetTime(0);
            return current;
        }

        public PlayableNode MoveOnNextNode(PlayableNode current, PlayableNode next) {
            current = next;
            current.PlayableClip.SetTime(0);
            return current;
        }
    }
}
