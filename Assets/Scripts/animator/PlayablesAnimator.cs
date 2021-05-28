using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using animationJobs;
using System;
using Unity.Collections;

namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesAnimator : MonoBehaviour {

        [SerializeField]
        private Resource resource;

        [SerializeField]
        private AnimatorJobsSettings jobsSettings;
        private List<DampingJobData> dampingJobsData = new List<DampingJobData>();

        private PlayableGraph graph;
        private Animator animator;
        public Brain Brain;

        Dictionary<string, PlayableParent> parents;

        public void Setup(List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }

            animator = GetComponent<Animator>();

            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            parents = new Dictionary<string, PlayableParent>();

            for (int i = 0; i < commands.Count; i++) {
                ProcessCommand(commands[i]);
            }

            graph.Play();
        }

        public void AddNewCommands(List<Command> commands) {
            if (!graph.IsValid()) {
                return;
            }

            for (int i = 0; i < commands.Count; i++) {
                ProcessCommand(commands[i]);
            }
        }

        private void ProcessCommand(Command command) {
            if (command.AddInput.HasValue) {
                ProcessAddInputCommand(command);

            } else if (command.AddContoller.HasValue) {
                ProcessAddControllerCommand(command);

            } else if (command.AddOutput.HasValue) {
                ProcessAddOutputCommand(command);

            }
        }

        private void ProcessAddInputCommand(Command command) {
            AddInputCommand inputCommand = command.AddInput.Value;
            var animationInput = inputCommand.AnimationInput;

            if (!parents.ContainsKey(animationInput.Parent)) {
                Debug.LogError($"Invalid parent name: {animationInput.Parent}");
                return;
            }

            PlayableParent parent = parents[animationInput.Parent];
            string name = animationInput.Name;

            if (animationInput.AnimationClip.HasValue) {

                AnimationClip clip = resource.animations[name];
                float duration = animationInput.AnimationClip.Value.TransitionDuration;
                float length = clip.length;

                if (length < duration) {
                    Debug.LogError($"Invalid transition duration in {name}");
                    return;
                }

                var controllerName = animationInput.AnimationClip.Value.ControllerName;
                if (Brain != null) {
                    if (!Brain.AnimControllers.ContainsKey(controllerName)) {
                        Debug.LogError($"Invalid controller name {controllerName} in {name}");
                        return;
                    }
                }

                AnimationClipPlayable animation =
                    AnimationClipPlayable.Create(graph, clip);
                ConnectNodeToParent(parent, animation);

                if (!parent.inputParent.IsNull()) {
                    if (parent.inputParent.IsPlayableOfType<AnimationLayerMixerPlayable>()) {

                        var maskName = animationInput.AnimationClip.Value.MaskName;
                        var layerMixer = (AnimationLayerMixerPlayable)parent.inputParent;
                        var layerIndex = (uint)layerMixer.GetInputCount() - 1;

                        var isAdditive = animationInput.AnimationClip.Value.IsAdditive;
                        layerMixer.SetLayerAdditive(layerIndex, isAdditive);

                        if (!string.IsNullOrEmpty(maskName)) {
                            if (resource.masks.ContainsKey(maskName)) {

                                var mask = resource.masks[maskName];
                                layerMixer.SetLayerMaskFromAvatarMask(layerIndex, mask);

                            } else {
                                Debug.LogError($"No mask with name {maskName}");

                            }
                        }
                    }
                }

                var newAnimation = new PlayableAnimation {
                    Parent = parent,
                    PlayableClip = animation,
                    TransitionDuration = duration,
                    AnimationLength = length
                };

                if (Brain == null || Brain.AnimControllers.Count == 0) {
                    return;
                }

                newAnimation.PlayableClip.SetTime(length);

                Brain.AnimControllers[controllerName].PlayableAnimations.Add(newAnimation);

            } else if (animationInput.AnimationMixer.HasValue) {

                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Mixer with name {name} is already exists");
                    return;
                }

                Playable playable = AnimationMixerPlayable.Create(graph);

                var playableParent = new PlayableParent {
                    inputParent = playable
                };

                parents.Add(name, playableParent);

                ConnectNodeToParent(parent, playable);

            } else if (animationInput.AnimationLayerMixer.HasValue) {
                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Layer Mixer with name {name} is already exists");
                    return;
                }

                var playable = AnimationLayerMixerPlayable.Create(graph);

                var playableParent = new PlayableParent {
                    inputParent = playable
                };

                parents.Add(name, playableParent);

                ConnectNodeToParent(parent, playable);

            } else if (animationInput.AnimationJob.HasValue) {

                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Job with name {name} is already exists");
                    return;
                }

                var job = animationInput.AnimationJob.Value;

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

                    var playableParent = new PlayableParent {
                        inputParent = lookAt
                    };

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

                    var playableParent = new PlayableParent {
                        inputParent = twoBone
                    };

                    parents.Add(name, playableParent);

                    ConnectNodeToParent(parent, twoBone);

                } else if (job.DampingJob.HasValue) {

                    var joints = jobsSettings.DampingJobSettings.Joints;
                    var numJoints = joints.Length;

                    var handles = new NativeArray<TransformStreamHandle>(
                        numJoints,
                        Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);

                    var localPositions = new NativeArray<Vector3>(
                        numJoints,
                        Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);
                    var localRotations = new NativeArray<Quaternion>(
                        numJoints,
                        Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);

                    var positions = new NativeArray<Vector3>(
                        numJoints,
                        Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);

                    for (var i = 0; i < numJoints; ++i) {
                        handles[i] = animator.BindStreamTransform(joints[i]);
                        localPositions[i] = joints[i].localPosition;
                        localRotations[i] = joints[i].localRotation;
                        positions[i] = joints[i].position;
                    }

                    var velocities = new NativeArray<Vector3>(numJoints, Allocator.Persistent);

                    var dampingJob = new DampingJob() {
                        rootHandle = animator.BindStreamTransform(transform),
                        jointHandles = handles,
                        localPositions = localPositions,
                        localRotations = localRotations,
                        positions = positions,
                        velocities = velocities
                    };

                    var dampingPlayable = AnimationScriptPlayable.Create(graph, dampingJob);

                    var dampingJobData = new DampingJobData {
                        ScriptPlayable = dampingPlayable,
                        Handles = handles,
                        LocalPositions = localPositions,
                        LocalRotations = localRotations,
                        Positions = positions,
                        Velocities = velocities
                    };

                    dampingJobsData.Add(dampingJobData);

                    var playableParent = new PlayableParent {
                        inputParent = dampingPlayable
                    };

                    parents.Add(name, playableParent);

                    ConnectNodeToParent(parent, dampingPlayable);

                }

            } else if (animationInput.AnimationBrain.HasValue) {

                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Brain with name {name} is already exists");
                    return;
                }

                var brainNode = ScriptPlayable<Brain>.Create(graph);

                Brain = brainNode.GetBehaviour();
                Brain.Initialize();

                var playableParent = new PlayableParent {
                    inputParent = brainNode
                };

                parents.Add(name, playableParent);

                ConnectNodeToParent(parent, brainNode);
            }
        }

        private void ProcessAddControllerCommand(Command command) {
            var input = command.AddContoller.Value.ControllerInput;

            if (Brain.AnimControllers.ContainsKey(input.Name)) {
                Debug.LogError($"Controller with name {input.Name} is already exists");
                return;
            }

            var controller = new AnimationController();

            if (!Enum.TryParse(input.ControllerType, out ControllerType type)) {
                Debug.LogError($"Invalid controller type {input.ControllerType}");
                return;
            }

            controller.ControllerType = type;

            controller.PlayableAnimations = new List<PlayableAnimation>();
            controller.RandomWeights = input.RandomWeights;

            controller.CurrentAnimationIndex = 0;
            controller.NextAnimationIndex = 0;

            controller.isEnable = false;

            if (Brain.AnimControllers == null) {
                Debug.LogError("No Animation Brain");
                return;
            }

            Brain.AnimControllers.Add(input.Name, controller);
            Brain.ControllerNames.Add(input.Name);
        }

        private void ProcessAddOutputCommand(Command command) {
            var animOutput = command.AddOutput.Value.AnimationOutput;
            string outputName = animOutput.Name;

            if (parents.ContainsKey(outputName)) {
                Debug.LogError($"Output with name {outputName} is already exists");
                return;
            }

            var playableOut = AnimationPlayableOutput.Create(graph, outputName, animator);

            var parentPlayable = new PlayableParent {
                outputParent = playableOut
            };

            parents.Add(outputName, parentPlayable);
        }

        private void ConnectNodeToParent(PlayableParent parent, Playable playable) {
            if (parent.inputParent.IsNull()) {
                parent.outputParent.SetSourcePlayable(playable);
            } else {
                bool isAnimation = playable.IsPlayableOfType<AnimationClipPlayable>();
                parent.inputParent.AddInput(playable, 0);
                if (!isAnimation) {
                    parent.inputParent.SetInputWeight(playable, 1);
                }


            }
        }

        private void OnDestroy() {
            if (graph.IsValid()) {
                foreach (var item in dampingJobsData) {
                    item.DisposeData();
                }
                graph.Destroy();
            }
        }
    }
}