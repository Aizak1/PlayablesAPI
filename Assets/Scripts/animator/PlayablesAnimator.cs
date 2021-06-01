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
        public Brain brain;

        private Dictionary<string, PlayableParent> parents;
        private Dictionary<string, ClipNode> clipNodes;

        public void Setup(List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }

            animator = GetComponent<Animator>();

            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            parents = new Dictionary<string, PlayableParent>();
            clipNodes = new Dictionary<string, ClipNode>();

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
            var animationInput = inputCommand.animationInput;

            if (!parents.ContainsKey(animationInput.parent)) {
                Debug.LogError($"Invalid parent name: {animationInput.parent}");
                return;
            }

            PlayableParent parent = parents[animationInput.parent];
            string name = animationInput.name;
            float initialWeight = animationInput.initialWeight;

            if (animationInput.animationClip.HasValue) {
                string clipName = animationInput.animationClip.Value.clipName;
                if (clipNodes.ContainsKey(name)) {
                    Debug.LogError($"Animation {name} is already exists");
                    return;
                }
                if (!resource.animations.ContainsKey(clipName)) {
                    Debug.LogError($"Invalid clip name in {name}");
                    return;
                }

                AnimationClip clip = resource.animations[clipName];
                float duration = animationInput.animationClip.Value.transitionDuration;
                float length = clip.length;

                if (length < duration) {
                    Debug.LogError($"Invalid transition duration in {name}");
                    return;
                }

                AnimationClipPlayable animation = AnimationClipPlayable.Create(graph, clip);
                ConnectNodeToParent(parent, animation, initialWeight);

                var newAnimation = new ClipNode {
                    Parent = parent,
                    PlayableClip = animation,
                    TransitionDuration = duration,
                    AnimationLength = length
                };

                if (brain == null) {
                    return;
                }
                newAnimation.PlayableClip.SetTime(length);
                clipNodes.Add(name, newAnimation);

            } else if (animationInput.animationMixer.HasValue) {

                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Mixer with name {name} is already exists");
                    return;
                }

                Playable playable = AnimationMixerPlayable.Create(graph);

                var playableParent = new PlayableParent {
                    inputParent = playable
                };

                parents.Add(name, playableParent);

                ConnectNodeToParent(parent, playable, initialWeight);

            } else if (animationInput.animationLayerMixer.HasValue) {
                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Layer Mixer with name {name} is already exists");
                    return;
                }

                var playable = AnimationLayerMixerPlayable.Create(graph);

                var playableParent = new PlayableParent {
                    inputParent = playable
                };

                parents.Add(name, playableParent);

                ConnectNodeToParent(parent, playable, initialWeight);

            } else if (animationInput.animationJob.HasValue) {

                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Job with name {name} is already exists");
                    return;
                }

                var job = animationInput.animationJob.Value;

                if (job.lookAtJob.HasValue) {

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

                    ConnectNodeToParent(parent, lookAt, initialWeight);

                } else if (job.twoBoneIKJob.HasValue) {

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

                    ConnectNodeToParent(parent, twoBone, initialWeight);

                } else if (job.dampingJob.HasValue) {

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

                    ConnectNodeToParent(parent, dampingPlayable, initialWeight);

                }

            } else if (animationInput.animationBrain.HasValue) {

                if (parents.ContainsKey(name)) {
                    Debug.LogError($"Brain with name {name} is already exists");
                    return;
                }

                var brainNode = ScriptPlayable<Brain>.Create(graph);

                brain = brainNode.GetBehaviour();
                brain.Initialize();

                var playableParent = new PlayableParent {
                    inputParent = brainNode
                };

                parents.Add(name, playableParent);

                ConnectNodeToParent(parent, brainNode,initialWeight);
            }
        }

        private void ProcessAddControllerCommand(Command command) {
            var controller = command.AddContoller.Value.animationController;
            if (brain.AnimControllers == null) {
                Debug.LogError("No Animation Brain");
                return;
            }
            if (brain.AnimControllers.ContainsKey(controller.name)) {
                Debug.LogError($"Controller with name {controller.name} is already exists");
                return;
            }
            if (controller.weightController.HasValue) {
                var weightController = controller.weightController.Value;
                var weightExecuter = new WeightControllerExecutor();
                if (weightController.circleController.HasValue) {
                    var circleExecuter = new CircleControllerExecutor();
                    circleExecuter.isClose = weightController.circleController.Value.isClose;
                    weightExecuter.additionalControllerExecuter = circleExecuter;

                } else if (weightController.randomController.HasValue) {
                    var randomWeights = weightController.randomController.Value.randomWeights;
                    if (randomWeights == null) {
                        Debug.LogError($"No randomWeights on {controller.name}");
                        return;
                    }
                    var randomExecuter = new RandomControllerExecutor();
                    randomExecuter.randomWeights = randomWeights;
                    weightExecuter.additionalControllerExecuter = randomExecuter;
                } else {
                    Debug.LogError("Unknown additionalController");
                    return;
                }

                List<ClipNode> animationNodes = new List<ClipNode>();
                foreach (var item in controller.weightController.Value.animationNames) {
                    if (!clipNodes.ContainsKey(item)) {
                        Debug.LogError("Unknown ClipNode Name");
                        continue;
                    }
                    animationNodes.Add(clipNodes[item]);
                }
                animationNodes[0].PlayableClip.SetTime(0);

                weightExecuter.animationNodes = animationNodes;
                brain.AnimControllers.Add(controller.name, weightExecuter);


            } else {
                Debug.LogError("No such controller");
                return;
            }
        }

        private void ProcessAddOutputCommand(Command command) {
            var animOutput = command.AddOutput.Value.animationOutput;
            string outputName = animOutput.name;

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

        private void ConnectNodeToParent(PlayableParent parent, Playable playable, float weight) {
            if (parent.inputParent.IsNull()) {
                parent.outputParent.SetSourcePlayable(playable);
            } else {
                parent.inputParent.AddInput(playable, 0);
                parent.inputParent.SetInputWeight(playable, weight);
            }
        }

        public PlayableGraph GraphPeel() {
            return graph;
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