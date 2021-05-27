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

            Brain.ActivateFirstController();

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
                AddInputCommand inputCommand = command.AddInput.Value;
                var animationInput = inputCommand.AnimationInput;
                if (!parents.ContainsKey(animationInput.Parent)) {
                    Debug.LogError("Invalid controller name");
                    return;
                }
                PlayableParent parent = parents[animationInput.Parent];
                string name = animationInput.Name;

                if (animationInput.AnimationClip.HasValue) {

                    float duration = animationInput.AnimationClip.Value.TransitionDuration;
                    AnimationClip clip = resource.animations[name];

                    var controllerName = animationInput.AnimationClip.Value.ControllerName;
                    if (!Brain.AnimControllers.ContainsKey(controllerName)) {
                        Debug.LogError("Invalid controller name");
                        return;
                    }
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

                    if (Brain.AnimControllers == null || Brain.AnimControllers.Count == 0) {
                        return;
                    }
                    newAnimation.PlayableClip.SetTime(length);

                    Brain.AnimControllers[controllerName].PlayableAnimations.Add(newAnimation);

                } else if (animationInput.AnimationMixer.HasValue) {

                    if (parents.ContainsKey(name)) {
                        Debug.LogError("This node is already exists");
                        return;
                    }

                    Playable playable = AnimationMixerPlayable.Create(graph);
                    var playableParent = new PlayableParent();
                    playableParent.inputParent = playable;
                    parents.Add(name, playableParent);

                    ConnectNodeToParent(parent, playable);

                } else if (animationInput.AnimationLayerMixer.HasValue) {
                    if (parents.ContainsKey(name)) {
                        Debug.LogError("This node is already exists");
                        return;
                    }

                    Playable playable = AnimationLayerMixerPlayable.Create(graph);

                    var playableParent = new PlayableParent();
                    playableParent.inputParent = playable;
                    parents.Add(name, playableParent);

                    ConnectNodeToParent(parent, playable);

                } else if (animationInput.AnimationJob.HasValue) {

                    if (parents.ContainsKey(name)) {
                        Debug.LogError("This node is already exists");
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
                        Debug.LogError("This node is already exists");
                        return;
                    }

                    var brainNode =
                        ScriptPlayable<Brain>.Create(graph);

                    Brain = brainNode.GetBehaviour();
                    Brain.Initialize();

                    var playableParent = new PlayableParent {
                        inputParent = brainNode
                    };
                    parents.Add(name, playableParent);

                    ConnectNodeToParent(parent, brainNode);
                }

            } else if (command.AddContoller.HasValue) {
                var input = command.AddContoller.Value.ControllerInput;

                if (Brain.AnimControllers.ContainsKey(input.Name)) {
                    Debug.LogError("This node is already exists");
                    return;
                }

                var controller = new AnimationController();

                if (!Enum.TryParse(input.ControllerType, out ControllerType type)) {
                    Debug.LogError("Invalid controller type");
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

            } else if (command.AddOutput.HasValue) {
                var animOutput = command.AddOutput.Value.AnimationOutput;
                string outputName = animOutput.Name;
                if (parents.ContainsKey(outputName)) {
                    Debug.LogError("This node is already exists");
                    return;
                }
                var playableOut = AnimationPlayableOutput.Create(graph, outputName, animator);
                var parentPlayable = new PlayableParent {
                    outputParent = playableOut
                };
                parents.Add(outputName, parentPlayable);

            }
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