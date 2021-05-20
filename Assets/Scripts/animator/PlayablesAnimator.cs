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

        private PlayableGraph graph;
        private AnimationController controller;

        private PlayableNode rootNode;
        private PlayableNode currentNode;

        private void Update() {
            if (controller.OpenCircle.HasValue) {
                controller.OpenCircle.Value.Play(currentNode);
            } else if (controller.CloseCircle.HasValue) {

            } else if (controller.Random.HasValue) {

            }
        }

        public void Setup(string firstNodeName, Command[] commands) {
            Animator animator = GetComponent<Animator>();
            graph = PlayableGraph.Create();
            var mainMixer = AnimationLayerMixerPlayable.Create(graph);

            Dictionary<string, Playable> parents = new Dictionary<string, Playable>();
            parents.Add(firstNodeName, mainMixer);

            var output = AnimationPlayableOutput.Create(graph, firstNodeName, animator);
            int source = output.GetSourceOutputPort();
            output.SetSourcePlayable(mainMixer);

            currentNode = new PlayableNode();
            rootNode = currentNode;

            for (int i = 0; i < commands.Length; i++) {
                if (commands[i].AddInput.HasValue) {
                    AddInputCommand inputCommand = commands[i].AddInput.Value;
                    if (inputCommand.AnimationInput.HasValue) {
                        var inputAnim = inputCommand.AnimationInput.Value;
                        Playable parent = parents[inputAnim.Parent];

                        if (inputAnim.AnimationClip.HasValue) {
                            string name = inputAnim.AnimationClip.Value.Name;
                            float duration = inputAnim.AnimationClip.Value.TransitionDuration;
                            AnimationClip clip = resource.animations[name];
                            AnimationClipPlayable animation =
                                AnimationClipPlayable.Create(graph, clip);
                            parent.AddInput(animation, source);

                            currentNode.Parent = parent;
                            currentNode.PlayableClip = animation;
                            currentNode.TransitionDuration = duration;
                            currentNode.Next = new PlayableNode();
                            currentNode = currentNode.Next;

                        } else if (inputAnim.AnimationMixer.HasValue) {
                            string name = inputAnim.AnimationMixer.Value.Name;
                            Playable playable = AnimationMixerPlayable.Create(graph);
                            parents.Add(name, playable);
                            parent.AddInput(playable, source);

                        } else if (inputAnim.AnimationLayerMixer.HasValue) {
                            string name = inputAnim.AnimationLayerMixer.Value.Name;
                            Playable playable = AnimationLayerMixerPlayable.Create(graph);
                            parents.Add(name, playable);
                            parent.AddInput(playable, source);
                        }
                    }

                } else if (commands[i].AddContoller.HasValue) {
                    AnimationController controller = commands[i].AddContoller.Value.Controller;
                    this.controller = controller;
                }
            }
            rootNode.Parent.SetInputWeight(rootNode.PlayableClip, 1);
            graph.Play();
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}