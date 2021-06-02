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

        private Dictionary<string, GraphNode> graphNodes;
        private Dictionary<string, ClipNode> clipNodes;

        public void Setup(List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
            }

            animator = GetComponent<Animator>();

            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            graphNodes = new Dictionary<string, GraphNode>();
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
            } else if (command.ChangeControllersState.HasValue) {
                ProcessChangeControllerCommand(command);
            } else if (command.ChangeWeight.HasValue) {
                ProcessChangeWeightCommand(command);
            } else if (command.SetLayerMask.HasValue) {
                ProcessSetLayerMask(command);
            }
        }

        private void ProcessSetLayerMask(Command command) {
            var setLayerMask = command.SetLayerMask.Value;
            if (!resource.masks.ContainsKey(setLayerMask.maskName)) {
                Debug.LogError("No such mask in resources");
                return;
            }
            var mask = resource.masks[setLayerMask.maskName];
            bool isAdditive = setLayerMask.isAdditive;
            foreach (var item in setLayerMask.animationNames) {
                if (!clipNodes.ContainsKey(item)) {
                    Debug.LogError("No such animationName");
                    continue;
                }
                var clipNode = clipNodes[item];
                if (clipNode.parent.output.HasValue) {
                    Debug.LogError("You need a layerMixer to set layer mask");
                    continue;
                }
                var input = clipNode.parent.input.Value;
                if (!input.IsPlayableOfType<AnimationLayerMixerPlayable>()) {
                    Debug.LogError("You need a layerMixer to set layer mask");
                    continue;
                }

                var layerMixer = (AnimationLayerMixerPlayable)input;

                layerMixer.SetLayerMaskFromAvatarMask((uint)clipNode.portIndex, mask);

                layerMixer.SetLayerAdditive((uint)clipNode.portIndex, isAdditive);
            }
        }

        private void ProcessChangeWeightCommand(Command command) {
            var changeWeight = command.ChangeWeight.Value;
            if (!graphNodes.ContainsKey(changeWeight.name)) {
                return;
            }
            if (!graphNodes.ContainsKey(changeWeight.parent)) {
                return;
            }

            if(changeWeight.weight < 0 || changeWeight.weight > 1) {
                Debug.LogError("Weight must be greater than zero and less than one");
                return;
            }

            var node = graphNodes[changeWeight.name];
            var parent = graphNodes[changeWeight.parent];

            if (!node.input.HasValue) {
                Debug.LogError("Incorrect input Node");
                return;
            }

            if (!parent.input.HasValue) {
                parent.output.Value.SetWeight(changeWeight.weight);
            } else {
                parent.input.Value.SetInputWeight(node.input.Value, changeWeight.weight);
            }
        }

        private void ProcessChangeControllerCommand(Command command) {
            var changeControllersState = command.ChangeControllersState.Value;
            var controllerNames = changeControllersState.controllerNames;
            if (changeControllersState.EnableControllers.HasValue) {
                foreach (var item in controllerNames) {
                    if (!brain.AnimControllers.ContainsKey(item)) {
                        Debug.LogError($"No controller with name {item} ");
                        continue;
                    }
                    brain.AnimControllers[item].Enable();
                }

            } else if (changeControllersState.DisableControllers.HasValue) {
                foreach (var item in controllerNames) {
                    if (!brain.AnimControllers.ContainsKey(item)) {
                        Debug.LogError($"No controller with name {item} ");
                        continue;
                    }
                    brain.AnimControllers[item].Disable();
                }
            }
        }

        private void ProcessAddInputCommand(Command command) {
            AddInputCommand inputCommand = command.AddInput.Value;
            var animationInput = inputCommand.AnimationInput;

            if (!graphNodes.ContainsKey(animationInput.parent)) {
                Debug.LogError($"Invalid parent name: {animationInput.parent}");
                return;
            }

            GraphNode parent = graphNodes[animationInput.parent];
            string name = animationInput.name;
            float initialWeight = animationInput.initialWeight;

            if (initialWeight < 0 || initialWeight > 1) {
                Debug.LogError("Weight must be greater than zero and less than one");
                return;
            }

            if (animationInput.AnimationClip.HasValue) {
                string clipName = animationInput.AnimationClip.Value.clipName;
                if (clipNodes.ContainsKey(name)) {
                    Debug.LogError($"Animation {name} is already exists");
                    return;
                }
                if (!resource.animations.ContainsKey(clipName)) {
                    Debug.LogError($"Invalid clip name in {name}");
                    return;
                }

                AnimationClip clip = resource.animations[clipName];
                float duration = animationInput.AnimationClip.Value.transitionDuration;
                float length = clip.length;

                if (length < duration) {
                    Debug.LogError($"Invalid transition duration in {name}");
                    return;
                }

                AnimationClipPlayable animation = AnimationClipPlayable.Create(graph, clip);
                ConnectNodeToParent(parent, animation, initialWeight);
                parent.inputCount++;
                graphNodes[animationInput.parent] = parent;

                var newAnimation = new ClipNode {
                    parent = parent,
                    playableClip = animation,
                    transitionDuration = duration,
                    animationLength = length,
                    portIndex = parent.inputCount - 1
                };

                if (brain == null) {
                    return;
                }
                newAnimation.playableClip.SetTime(length);
                clipNodes.Add(name, newAnimation);

                var graphNode = new GraphNode() {
                    input = animation
                };
                graphNodes.Add(name, graphNode);


            } else if (animationInput.AnimationMixer.HasValue) {

                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Mixer with name {name} is already exists");
                    return;
                }

                Playable playable = AnimationMixerPlayable.Create(graph);

                var playableParent = new GraphNode {
                    input = playable
                };

                graphNodes.Add(name, playableParent);

                ConnectNodeToParent(parent, playable, initialWeight);
                parent.inputCount++;
                graphNodes[animationInput.parent] = parent;

            } else if (animationInput.AnimationLayerMixer.HasValue) {
                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Layer Mixer with name {name} is already exists");
                    return;
                }

                var playable = AnimationLayerMixerPlayable.Create(graph);

                var playableParent = new GraphNode {
                    input = playable
                };

                graphNodes.Add(name, playableParent);

                ConnectNodeToParent(parent, playable, initialWeight);
                parent.inputCount++;
                graphNodes[animationInput.parent] = parent;

            } else if (animationInput.AnimationJob.HasValue) {

                if (graphNodes.ContainsKey(name)) {
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

                    var playableParent = new GraphNode {
                        input = lookAt
                    };

                    graphNodes.Add(name, playableParent);

                    ConnectNodeToParent(parent, lookAt, initialWeight);
                    parent.inputCount++;
                    graphNodes[animationInput.parent] = parent;

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

                    var playableParent = new GraphNode {
                        input = twoBone
                    };

                    graphNodes.Add(name, playableParent);

                    ConnectNodeToParent(parent, twoBone, initialWeight);
                    parent.inputCount++;
                    graphNodes[animationInput.parent] = parent;

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

                    var playableParent = new GraphNode {
                        input = dampingPlayable
                    };

                    graphNodes.Add(name, playableParent);

                    ConnectNodeToParent(parent, dampingPlayable, initialWeight);
                    parent.inputCount++;
                    graphNodes[animationInput.parent] = parent;

                }

            } else if (animationInput.AnimationBrain.HasValue) {

                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Brain with name {name} is already exists");
                    return;
                }

                var brainNode = ScriptPlayable<Brain>.Create(graph);

                brain = brainNode.GetBehaviour();
                brain.Initialize();

                var playableParent = new GraphNode {
                    input = brainNode
                };

                graphNodes.Add(name, playableParent);

                ConnectNodeToParent(parent, brainNode,initialWeight);
                parent.inputCount++;
                graphNodes[animationInput.parent] = parent;
            }
        }

        private void ProcessAddControllerCommand(Command command) {
            var controller = command.AddContoller.Value.AnimationController;
            if (brain.AnimControllers == null) {
                Debug.LogError("No Animation Brain");
                return;
            }
            if (brain.AnimControllers.ContainsKey(controller.name)) {
                Debug.LogError($"Controller with name {controller.name} is already exists");
                return;
            }
            if (controller.WeightController.HasValue) {
                var weightController = controller.WeightController.Value;
                var weightExecuter = new WeightControllerExecutor();
                if (weightController.CircleController.HasValue) {
                    var circleExecuter = new CircleControllerExecutor();
                    circleExecuter.isClose = weightController.CircleController.Value.isClose;
                    weightExecuter.additionalControllerExecuter = circleExecuter;

                } else if (weightController.RandomController.HasValue) {
                    var randomWeights = weightController.RandomController.Value.randomWeights;
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
                foreach (var item in controller.WeightController.Value.animationNames) {
                    if (!clipNodes.ContainsKey(item)) {
                        Debug.LogError("Unknown ClipNode Name");
                        continue;
                    }
                    animationNodes.Add(clipNodes[item]);
                }


                weightExecuter.animationNodes = animationNodes;
                weightExecuter.Disable();
                brain.AnimControllers.Add(controller.name, weightExecuter);


            } else {
                Debug.LogError("No such controller");
                return;
            }
        }

        private void ProcessAddOutputCommand(Command command) {
            var animOutput = command.AddOutput.Value.AnimationOutput;
            string outputName = animOutput.name;

            if (graphNodes.ContainsKey(outputName)) {
                Debug.LogError($"Output with name {outputName} is already exists");
                return;
            }

            var playableOut = AnimationPlayableOutput.Create(graph, outputName, animator);

            var parentPlayable = new GraphNode {
                output = playableOut
            };

            graphNodes.Add(outputName, parentPlayable);
        }

        private void ConnectNodeToParent(GraphNode parent, Playable playable, float weight) {
            if (!parent.input.HasValue) {
                parent.output.Value.SetSourcePlayable(playable);
                parent.output.Value.SetWeight(weight);
            } else {
                parent.input.Value.AddInput(playable, 0);
                parent.input.Value.SetInputWeight(playable, weight);
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