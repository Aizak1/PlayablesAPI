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
        private PlayableNode nextNode;

        private void Update() {
            if (currentNode == null) {
                return;
            }

            if (controller.OpenCircle.HasValue) {
                currentNode = controller.OpenCircle.Value.UpdateNodeState(currentNode);

            } else if (controller.CloseCircle.HasValue) {
                currentNode = controller.CloseCircle.Value.UpdateNodeState(currentNode, rootNode);

            } else if (controller.Random.HasValue) {
                if (nextNode == null || nextNode == currentNode) {
                    nextNode = new PlayableNode();
                    nextNode = controller.Random.Value.GetRandomNode(currentNode, rootNode);
                }
                currentNode = controller.Random.Value.UpdateNodeState(currentNode, nextNode);
            }
        }

        public void Setup(string firstNodeName, Command[] commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }
            Animator animator = GetComponent<Animator>();
            graph = PlayableGraph.Create();
            var mainMixer = AnimationLayerMixerPlayable.Create(graph);

            Dictionary<string, Playable> parents = new Dictionary<string, Playable>();
            parents.Add(firstNodeName, mainMixer);

            var output = AnimationPlayableOutput.Create(graph, firstNodeName, animator);
            int source = output.GetSourceOutputPort();
            output.SetSourcePlayable(mainMixer);

            nextNode = null;
            currentNode = new PlayableNode();
            rootNode = currentNode;

            for (int i = 0; i < commands.Length; i++) {
                if (commands[i].AddInput.HasValue) {
                    AddInputCommand inputCommand = commands[i].AddInput.Value;
                    if (inputCommand.AnimationInput.HasValue) {
                        var inputAnim = inputCommand.AnimationInput.Value;
                        Playable parent = parents[inputAnim.Parent];

                        if (inputAnim.AnimationClip.HasValue) {
                            if (!rootNode.PlayableClip.IsNull()) {
                                currentNode.Next = new PlayableNode();
                                currentNode = currentNode.Next;
                            }

                            string name = inputAnim.AnimationClip.Value.Name;
                            float duration = inputAnim.AnimationClip.Value.TransitionDuration;
                            AnimationClip clip = resource.animations[name];
                            AnimationClipPlayable animation =
                                AnimationClipPlayable.Create(graph, clip);
                            parent.AddInput(animation, source);
                            float length = animation.GetAnimationClip().length;

                            currentNode.Parent = parent;
                            currentNode.PlayableClip = animation;
                            currentNode.TransitionDuration = duration;
                            currentNode.PlayableClip.SetTime(length);

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
                    controller = commands[i].AddContoller.Value.Controller;
                }
            }

            currentNode = rootNode;
            currentNode.PlayableClip.SetTime(0);
            currentNode.Parent.SetInputWeight(currentNode.PlayableClip, 1);
            graph.Play();
        }

        private void OnDestroy() {
            if (graph.IsValid()) {
                graph.Destroy();
            }
        }
    }
}