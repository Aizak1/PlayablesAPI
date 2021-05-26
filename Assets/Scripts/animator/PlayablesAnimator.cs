using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using animationJobs;
using System;

namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesAnimator : MonoBehaviour {

        [SerializeField]
        private Resource resource;
        [SerializeField]
        private AnimatorJobsSettings jobsSettings;
        private PlayableGraph graph;
        private Brain brain;
        private List<string> animationConrollersNames;

        Dictionary<string, PlayableParent> parents;

        private void Update() {
            if (brain.AnimControllers == null) {
                return;
            }

            foreach (var item in animationConrollersNames) {
                brain.ProcessController(item);
            }
        }

        public void Setup(List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }
            animationConrollersNames = new List<string>();
            Animator animator = GetComponent<Animator>();
            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            parents = new Dictionary<string, PlayableParent>();

            for (int i = 0; i < commands.Count; i++) {
                if (commands[i].AddInput.HasValue) {
                    AddInputCommand inputCommand = commands[i].AddInput.Value;
                    var animationInput = inputCommand.AnimationInput;
                    PlayableParent parent = parents[animationInput.Parent];
                    string name = animationInput.Name;

                    if (animationInput.AnimationClip.HasValue) {

                        float duration = animationInput.AnimationClip.Value.TransitionDuration;
                        AnimationClip clip = resource.animations[name];
                        AnimationClipPlayable animation =
                            AnimationClipPlayable.Create(graph, clip);
                        ConnectNodeToParent(parent, animation);
                        float length = animation.GetAnimationClip().length;

                        var newAnimation = new PlayableAnimation {
                            Parent = parent,
                            PlayableClip = animation,
                            TransitionDuration = duration,
                            AnimationLength = length
                        };

                        if (brain.AnimControllers == null || brain.AnimControllers.Count == 0) {
                            continue;
                        }
                        var controllerName = animationInput.AnimationClip.Value.ControllerName;
                        if (!brain.AnimControllers.ContainsKey(controllerName)) {
                            Debug.LogError("Invalid controller name");
                            return;
                        }
                        newAnimation.PlayableClip.SetTime(length);

                        brain.AnimControllers[controllerName].PlayableAnimations.Add(newAnimation);

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
                    }else if (animationInput.AnimationBrain.HasValue) {
                        brain = new Brain();
                        brain.AnimControllers = new Dictionary<string, AnimationController>();

                        AnimationScriptPlayable brainNode =
                            AnimationScriptPlayable.Create(graph,brain);

                        var playableParent = new PlayableParent();
                        playableParent.inputParent = brainNode;
                        parents.Add(name, playableParent);

                        ConnectNodeToParent(parent, brainNode);
                    }

                } else if (commands[i].AddContoller.HasValue) {
                    var input = commands[i].AddContoller.Value.ControllerInput;
                    var controller = new AnimationController();

                    if(!Enum.TryParse(input.ControllerType, out ControllerType type)) {
                        Debug.LogError("Invalid controller type");
                        return;
                    }
                    controller.ControllerType = type;

                    controller.PlayableAnimations = new List<PlayableAnimation>();

                    controller.CurrentAnimationIndex = 0;
                    controller.NextAnimationIndex = 0;

                    controller.RandomWeights = input.RandomWeights;

                    if (brain.AnimControllers == null) {
                        Debug.LogError("No Animation Brain");
                        return;
                    }
                    animationConrollersNames.Add(input.Name);
                    brain.AnimControllers.Add(input.Name, controller);

                } else if (commands[i].AddOutput.HasValue) {

                    var animOutput = commands[i].AddOutput.Value.AnimationOutput;
                    string outputName = animOutput.Name;
                    var playableOut= AnimationPlayableOutput.Create(graph, outputName, animator);
                    var parentPlayable = new PlayableParent();
                    parentPlayable.outputParent = playableOut;
                    parents.Add(outputName, parentPlayable);

                }
            }

            if (brain.AnimControllers != null && brain.AnimControllers.Count != 0) {
                foreach (var item in brain.AnimControllers) {
                    item.Value.PlayableAnimations[0].PlayableClip.SetTime(0);
                    item.Value.PlayableAnimations[0].Parent.inputParent.SetInputWeight(0, 1);
                }
            }

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