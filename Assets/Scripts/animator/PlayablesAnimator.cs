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

        public void Setup(string firstNodeName, List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }
            Animator animator = GetComponent<Animator>();
            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var mainMixer = AnimationLayerMixerPlayable.Create(graph);

            Dictionary<string, Playable> parents = new Dictionary<string, Playable>();
            parents.Add(firstNodeName, mainMixer);

            var output = AnimationPlayableOutput.Create(graph, firstNodeName, animator);
            int source = output.GetSourceOutputPort();
            output.SetSourcePlayable(mainMixer);

            nextNode = null;
            currentNode = new PlayableNode();
            rootNode = currentNode;

            for (int i = 0; i < commands.Count; i++) {
                if (commands[i].AddInput.HasValue) {
                    AddInputCommand inputCommand = commands[i].AddInput.Value;

                    var animationInput = inputCommand.AnimationInput;
                    Playable parent = parents[animationInput.Parent];

                    if (animationInput.AnimationClip.HasValue) {
                        if (!rootNode.PlayableClip.IsNull()) {
                            currentNode.Next = new PlayableNode();
                            currentNode = currentNode.Next;
                        }

                        string name = animationInput.AnimationClip.Value.Name;
                        float duration = animationInput.AnimationClip.Value.TransitionDuration;
                        AnimationClip clip = resource.animations[name];
                        AnimationClipPlayable animation =
                            AnimationClipPlayable.Create(graph, clip);
                        parent.AddInput(animation, source);
                        float length = animation.GetAnimationClip().length;

                        currentNode.Parent = parent;
                        currentNode.PlayableClip = animation;
                        currentNode.TransitionDuration = duration;
                        currentNode.PlayableClip.SetTime(length);

                    } else if (animationInput.AnimationMixer.HasValue) {
                        string name = animationInput.AnimationMixer.Value.Name;
                        Playable playable = AnimationMixerPlayable.Create(graph);
                        parents.Add(name, playable);
                        parent.AddInput(playable, source);

                    } else if (animationInput.AnimationLayerMixer.HasValue) {
                        string name = animationInput.AnimationLayerMixer.Value.Name;
                        Playable playable = AnimationLayerMixerPlayable.Create(graph);
                        parents.Add(name, playable);
                        parent.AddInput(playable, source);
                    } else if (animationInput.AnimationJob.HasValue) {

                        var job = animationInput.AnimationJob.Value;
                        string jobName = job.Name;

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
                            parents.Add(jobName, lookAt);
                            parent.AddInput(lookAt, source);
                        }else if (job.TwoBoneIKJob.HasValue) {

                            var endJoint = jobsSettings.TwoBoneIKSettings.EndJoint;
                            var effector = jobsSettings.TwoBoneIKSettings.EffectorModel;
                            Transform midJoint = endJoint.parent;
                            Transform topJoint = midJoint.parent;
                            var obj = Instantiate(effector, endJoint.position, endJoint.rotation);
                            var twoBoneIKJob = new TwoBoneIKJob();

                            var tranform = obj.transform;
                            twoBoneIKJob.Setup(animator, topJoint, midJoint, endJoint, tranform);

                            Playable twoBone = AnimationScriptPlayable.Create(graph, twoBoneIKJob);
                            parents.Add(jobName, twoBone);
                            parent.AddInput(twoBone, source);
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