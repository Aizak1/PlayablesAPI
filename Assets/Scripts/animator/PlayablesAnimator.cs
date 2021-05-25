using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using animationJobs;


namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesAnimator : MonoBehaviour {

        [SerializeField]
        private Resource resource;
        [SerializeField]
        private AnimatorJobsSettings jobsSettings;
        private PlayableGraph graph;
        private AnimationController controller;

        private PlayableNode rootNode;
        private PlayableNode currentNode;
        private PlayableNode nextNode;

        Dictionary<string, PlayableParent> parents;

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

        public void Setup(List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }
            Animator animator = GetComponent<Animator>();
            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            nextNode = null;
            parents = new Dictionary<string, PlayableParent>();
            currentNode = new PlayableNode();
            rootNode = currentNode;

            for (int i = 0; i < commands.Count; i++) {
                if (commands[i].AddInput.HasValue) {
                    AddInputCommand inputCommand = commands[i].AddInput.Value;
                    var animationInput = inputCommand.AnimationInput;
                    PlayableParent parent = parents[animationInput.Parent];
                    string name = animationInput.Name;

                    if (animationInput.AnimationClip.HasValue) {
                        if (!rootNode.PlayableClip.IsNull()) {
                            currentNode.Next = new PlayableNode();
                            currentNode = currentNode.Next;
                        }
                        float duration = animationInput.AnimationClip.Value.TransitionDuration;
                        AnimationClip clip = resource.animations[name];
                        AnimationClipPlayable animation =
                            AnimationClipPlayable.Create(graph, clip);
                        ConnectNodeToParent(parent, animation);
                        float length = animation.GetAnimationClip().length;

                        currentNode.Parent = parent;
                        currentNode.PlayableClip = animation;
                        currentNode.TransitionDuration = duration;
                        currentNode.PlayableClip.SetTime(length);

                    } else if (animationInput.AnimationMixer.HasValue) {

                        Playable playable = AnimationMixerPlayable.Create(graph);
                        var playableParent = new PlayableParent();
                        playableParent.inputParent = playable;
                        parents.Add(name, playableParent);

                        ConnectNodeToParent(parent, playable);

                    } else if (animationInput.AnimationLayerMixer.HasValue) {
                        Playable playable = AnimationLayerMixerPlayable.Create(graph);

                        var playableParent = new PlayableParent();
                        playableParent.inputParent = playable;
                        parents.Add(name, playableParent);

                        ConnectNodeToParent(parent, playable);

                    } else if (animationInput.AnimationJob.HasValue) {

                        var job = animationInput.AnimationJob.Value;
                        string jobName = name;

                        if (job.LookAtJob.HasValue) {
                            animator.fireEvents = false;
                            var lookAtSettings = jobsSettings.LookAtSettings;
                            var targetTransform = lookAtSettings.Target.transform;
                            var axisVector = lookAtSettings.GetAxisVector(lookAtSettings.Axis);

                            var lookAtJob = new LookAtJob() {
                                joint = animator.BindStreamTransform(lookAtSettings.Joint),
                                target = animator.BindSceneTransform(targetTransform),
                                axis = axisVector,
                                maxAngle = lookAtSettings.MaxAngle,
                                minAngle = lookAtSettings.MinAngle
                            };

                            Playable lookAt = AnimationScriptPlayable.Create(graph, lookAtJob);

                            var playableParent = new PlayableParent();
                            playableParent.inputParent = lookAt;
                            parents.Add(name, playableParent);

                            ConnectNodeToParent(parent, lookAt);

                        } else if (job.TwoBoneIKJob.HasValue) {

                            var endJoint = jobsSettings.TwoBoneIKSettings.EndJoint;
                            var effector = jobsSettings.TwoBoneIKSettings.EffectorModel;
                            Transform midJoint = endJoint.parent;
                            Transform topJoint = midJoint.parent;
                            var obj = Instantiate(effector, endJoint.position, endJoint.rotation);
                            var twoBoneIKJob = new TwoBoneIKJob();

                            var tranform = obj.transform;
                            twoBoneIKJob.Setup(animator, topJoint, midJoint, endJoint, tranform);

                            Playable twoBone = AnimationScriptPlayable.Create(graph, twoBoneIKJob);

                            var playableParent = new PlayableParent();
                            playableParent.inputParent = twoBone;
                            parents.Add(name, playableParent);

                            ConnectNodeToParent(parent, twoBone);
                        }
                    }

                } else if (commands[i].AddContoller.HasValue) {
                    controller = commands[i].AddContoller.Value.Controller;
                } else if (commands[i].AddOutput.HasValue) {

                    var animOutput = commands[i].AddOutput.Value.AnimationOutput;
                    string outputName = animOutput.Name;
                    var playableOut= AnimationPlayableOutput.Create(graph, outputName, animator);
                    var parentPlayable = new PlayableParent();
                    parentPlayable.outputParent = playableOut;
                    parents.Add(outputName, parentPlayable);

                }
            }

            currentNode = rootNode;
            if (currentNode.Parent.inputParent.IsNull()) {
                currentNode.Parent.outputParent.SetWeight(1);
            } else {
                currentNode.Parent.inputParent.SetInputWeight(currentNode.PlayableClip, 1);
            }
            currentNode.PlayableClip.SetTime(0);
            graph.Play();
        }

        private void ConnectNodeToParent(PlayableParent parent, Playable playable) {
            if (parent.inputParent.IsNull()) {
                parent.outputParent.SetSourcePlayable(playable);
            } else {
                parent.inputParent.AddInput(playable, 0);
            }
        }

        private void OnDestroy() {
            if (graph.IsValid()) {
                graph.Destroy();
            }
        }
    }
}