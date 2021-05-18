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

        [SerializeField]
        private float presetTransitionTime;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private int source;

        private PlayableNode rootNode;
        private PlayableNode currentNode;

        private float startTransitionTime;
        private float endTransitionTime;



        public void Setup(Command[] commands) {
            graph = PlayableGraph.Create();
            for (int i = 0; i < commands.Length; i++) {
                if (commands[i].AddInput.HasValue) {
                    AddInputCommand inputCommand = commands[i].AddInput.Value;
                    if (inputCommand.AnimationInput.HasValue) {
                        var inputAnim = inputCommand.AnimationInput.Value;
                        if (inputAnim.AnimationClip.HasValue) {

                        } else if (inputAnim.AnimationMixer.HasValue) {

                        } else if (inputAnim.AnimationLayerMixer.HasValue) {

                        }
                    }

                } else if (commands[i].AddOutput.HasValue) {

                } else if(commands[i].AddContoller.HasValue) {

                }
            }


            graph.Play();
        }

        private PlayableNode TakeNextNode(PlayableNode currentNode) {
            if (currentNode.Next != null) {
                return currentNode.Next;
            }

            return null;
        }

        private void SpreadWeight(float weight, PlayableNode currentNode, PlayableNode nextNode) {
            mixer.SetInputWeight(currentNode.Playable, 1 - weight);
            mixer.SetInputWeight(nextNode.Playable, weight);
        }

        private PlayableNode MoveOnNextNode(PlayableNode currentNode, PlayableNode nextNode) {
            mixer.SetInputWeight(currentNode.Playable, 0);
            mixer.SetInputWeight(nextNode.Playable, 1);

            return nextNode;
        }

    }
}