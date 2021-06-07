using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using animationJobs;
using Unity.Collections;

namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesAnimator : MonoBehaviour {

        [SerializeField]
        private Resource resource;

        private List<IJobTemp> jobsTemp = new List<IJobTemp>();

        private PlayableGraph graph;
        private Animator animator;
        public Brain brain;

        private Dictionary<string, GraphNode> graphNodes;

        public void Setup(List<Command> commands) {
            if (graph.IsValid()) {
                graph.Destroy();
                foreach (var item in jobsTemp) {
                    item.DisposeData();
                }
                jobsTemp = new List<IJobTemp>();
            }

            animator = GetComponent<Animator>();

            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            graphNodes = new Dictionary<string, GraphNode>();

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
                if (!graphNodes.ContainsKey(item)) {
                    Debug.LogError("No such animationName");
                    continue;
                }
                var clip = graphNodes[item].input.Value;
                if (clip.GetOutput(0).IsNull()) {
                    Debug.LogError("Parent must be AnimationLayerMixer");
                    continue;
                }

                if (!clip.GetOutput(0).IsPlayableOfType<AnimationLayerMixerPlayable>()) {
                    Debug.LogError("Parent must be AnimationLayerMixer");
                    continue;
                }

                var layerMixer = (AnimationLayerMixerPlayable)clip.GetOutput(0);

                layerMixer.SetLayerMaskFromAvatarMask((uint)graphNodes[item].portIndex, mask);

                layerMixer.SetLayerAdditive((uint)graphNodes[item].portIndex, isAdditive);
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

                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Playable with {name} is already exists");
                    return;
                }
                if (!resource.animations.ContainsKey(clipName)) {
                    Debug.LogError($"Invalid clip name in {name}");
                    return;
                }

                AnimationClip clip = resource.animations[clipName];

                AnimationClipPlayable animation = AnimationClipPlayable.Create(graph, clip);
                ConnectNodeToParent(parent, animation, initialWeight);

                if (brain == null) {
                    return;
                }
                animation.SetTime(clip.length);

                var graphNode = new GraphNode() {
                    input = animation,
                    portIndex = GetPortIndex(parent)
                };

                graphNodes.Add(name, graphNode);


            } else if (animationInput.AnimationMixer.HasValue) {

                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Mixer with name {name} is already exists");
                    return;
                }

                Playable playable = AnimationMixerPlayable.Create(graph);

                ConnectNodeToParent(parent, playable, initialWeight);
                var playableParent = new GraphNode {
                    input = playable,
                    portIndex = GetPortIndex(parent)

                };
                graphNodes.Add(name, playableParent);

            } else if (animationInput.AnimationLayerMixer.HasValue) {
                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Layer Mixer with name {name} is already exists");
                    return;
                }

                var playable = AnimationLayerMixerPlayable.Create(graph);

                ConnectNodeToParent(parent, playable, initialWeight);
                var playableParent = new GraphNode {
                    input = playable,
                    portIndex = GetPortIndex(parent)

                };

                graphNodes.Add(name, playableParent);


            } else if (animationInput.AnimationJob.HasValue) {

                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Job with name {name} is already exists");
                    return;
                }

                var job = animationInput.AnimationJob.Value;

                if (job.LookAtJob.HasValue) {

                    animator.fireEvents = false;

                    var lookAt = job.LookAtJob.Value;
                    if (!resource.effectors.ContainsKey(lookAt.effectorName)) {
                        Debug.LogError($"No such effector in resources {lookAt.effectorName}");
                        return;
                    }

                    string[] parts = lookAt.jointPath.Split('/');

                    if (!resource.models.ContainsKey(parts[0])) {
                        Debug.LogError("No such model");
                        return;
                    }

                    var mainJoint = resource.models[parts[0]];
                    for (int i = 1; i < parts.Length; i++) {
                        var child = mainJoint.Find(parts[i]);
                        if (child == null) {
                            Debug.LogError($"No such part in the model {parts[i]}");
                            return;
                        }
                        mainJoint = child;
                    }

                    var targetTransform = resource.effectors[lookAt.effectorName].transform;

                    var lookAtJob = new LookAtJob() {
                        joint = animator.BindStreamTransform(mainJoint),
                        target = animator.BindSceneTransform(targetTransform),
                        axis = new Vector3(lookAt.axisX, lookAt.axisY, lookAt.axisZ),
                        maxAngle = lookAt.maxAngle,
                        minAngle = lookAt.minAngle
                    };

                    Playable lookAtPlayable = AnimationScriptPlayable.Create(graph, lookAtJob);
                    ConnectNodeToParent(parent, lookAtPlayable, initialWeight);

                    var playableParent = new GraphNode {
                        input = lookAtPlayable,
                        portIndex = GetPortIndex(parent)
                    };

                    graphNodes.Add(name, playableParent);


                } else if (job.TwoBoneIKJob.HasValue) {
                    var twoBoneIkInput = job.TwoBoneIKJob.Value;

                    if (!resource.effectors.ContainsKey(twoBoneIkInput.effectorName)) {
                        Debug.LogError($"No effector in resources:{twoBoneIkInput.effectorName}");
                        return;
                    }
                    var separator = '/';
                    string[] parts = twoBoneIkInput.jointPath.Split(separator);

                    if (!resource.models.ContainsKey(parts[0])) {
                        Debug.LogError("No such model");
                        return;
                    }

                    var mainJoint = resource.models[parts[0]];
                    for (int i = 1; i < parts.Length; i++) {
                        var child = mainJoint.Find(parts[i]);
                        if (child == null) {
                            Debug.LogError($"No such part in the model {parts[i]}");
                            return;
                        }
                        mainJoint = child;
                    }
                    var endJoint = mainJoint;
                    var effector = resource.effectors[twoBoneIkInput.effectorName];
                    Transform midJoint = endJoint.parent;
                    Transform topJoint = midJoint.parent;
                    var obj = Instantiate(effector, endJoint.position, endJoint.rotation);
                    var twoBoneIKJob = new TwoBoneIKJob();

                    twoBoneIKJob.Setup(animator, topJoint, midJoint, endJoint, obj.transform);

                    Playable twoBone = AnimationScriptPlayable.Create(graph, twoBoneIKJob);
                    ConnectNodeToParent(parent, twoBone, initialWeight);

                    var playableParent = new GraphNode {
                        input = twoBone,
                        portIndex = GetPortIndex(parent)
                    };

                    var twoBoneJobData = new TwoBoneIKJobTemp() {
                        effector = obj
                    };

                    jobsTemp.Add(twoBoneJobData);
                    graphNodes.Add(name, playableParent);


                } else if (job.DampingJob.HasValue) {
                    var dampingJobInput = job.DampingJob.Value;
                    var separator = '/';
                    List<string> pathes= dampingJobInput.jointPathes;
                    List<string[]> jointsParts = new List<string[]>();
                    for (int i = 0; i < pathes.Count; i++) {
                        var jointParts = pathes[i].Split(separator);
                        jointsParts.Add(jointParts);
                    }
                    List<Transform> joints = new List<Transform>();
                    foreach (var jointParts in jointsParts) {
                        if (!resource.models.ContainsKey(jointParts[0])) {
                            Debug.LogError("No such model");
                            return;
                        }
                        var mainJoint = resource.models[jointParts[0]];
                        for (int i = 1; i < jointParts.Length; i++) {
                            var child = mainJoint.Find(jointParts[i]);
                            if (child == null) {
                                Debug.LogError($"No such part in the model {jointParts[i]}");
                                return;
                            }
                            mainJoint = child;
                        }
                        joints.Add(mainJoint);
                    }
                    var numJoints = joints.Count;

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

                    var dampingJobData = new DampingJobTemp {
                        Handles = handles,
                        LocalPositions = localPositions,
                        LocalRotations = localRotations,
                        Positions = positions,
                        Velocities = velocities
                    };

                    jobsTemp.Add(dampingJobData);
                    ConnectNodeToParent(parent, dampingPlayable, initialWeight);

                    var playableParent = new GraphNode {
                        input = dampingPlayable,
                        portIndex = GetPortIndex(parent)
                    };

                    graphNodes.Add(name, playableParent);


                }

            } else if (animationInput.AnimationBrain.HasValue) {

                if (graphNodes.ContainsKey(name)) {
                    Debug.LogError($"Brain with name {name} is already exists");
                    return;
                }

                var brainNode = ScriptPlayable<Brain>.Create(graph);

                brain = brainNode.GetBehaviour();
                brain.Initialize();
                ConnectNodeToParent(parent, brainNode,initialWeight);

                var playableParent = new GraphNode {
                    input = brainNode,
                    portIndex = GetPortIndex(parent)
                };

                graphNodes.Add(name, playableParent);

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

            var name = controller.name;

            if (controller.WeightController.HasValue) {

                var newController = new WeightControllerExecutor {
                    isFree = true
                };

                brain.AnimControllers.Add(name, newController);

            } else if (controller.CircleController.HasValue) {

                var weightControllerName = controller.CircleController.Value.weightControllerName;

                if (!brain.AnimControllers.ContainsKey(weightControllerName)) {
                    return;
                }

                var executor =
                    (WeightControllerExecutor)brain.AnimControllers[weightControllerName];

                if (!executor.isFree) {
                    return;
                }

                executor.isFree = false;

                var infos = new List<AnimationInfo>();
                var animations = new Dictionary<string, Playable>();

                foreach (var item in controller.CircleController.Value.animationInfos) {

                    if (!graphNodes.ContainsKey(item.name)) {
                        Debug.LogError($"No animation with name {item.name}");
                        continue;
                    }

                    if (item.transitionDuration > item.animationLength) {
                        Debug.LogError("Transition duration can't be greater then length");
                        continue;
                    }
                    var playable = graphNodes[item.name].input.Value;
                    if (!playable.IsPlayableOfType<AnimationClipPlayable>()) {
                        Debug.LogError("Controllers should have only animations");
                        continue;
                    }
                    var clipPlayable = (AnimationClipPlayable)playable;
                    if(clipPlayable.GetAnimationClip().length < item.animationLength) {
                        Debug.LogError("animationLenght must be less than clip length");
                        continue;
                    }

                    infos.Add(item);
                    animations.Add(item.name, playable);
                }

                bool isClose = controller.CircleController.Value.isClose;

                var newController = new CircleControllerExecutor();
                newController.Setup(executor, infos, isClose, animations);

                brain.AnimControllers.Add(controller.name, newController);

                var firstAnim = animations[infos[0].name];
                firstAnim.SetTime(0);
                firstAnim.GetOutput(0).SetInputWeight(firstAnim,1);

            } else if (controller.RandomController.HasValue) {

                var weightControllerName = controller.RandomController.Value.weightControllerName;

                if (!brain.AnimControllers.ContainsKey(weightControllerName)) {
                    return;
                }

                var executor =
                    (WeightControllerExecutor)brain.AnimControllers[weightControllerName];

                if (!executor.isFree) {
                    return;
                }

                executor.isFree = false;

                var infos = new List<AnimationInfo>();
                var animations = new Dictionary<string, Playable>();

                foreach (var item in controller.RandomController.Value.animationInfos) {

                    if (!graphNodes.ContainsKey(item.name)) {
                        continue;
                    }

                    infos.Add(item);
                    animations.Add(item.name, graphNodes[item.name].input.Value);
                }

                var randomWeights = controller.RandomController.Value.randomWeights;

                var newController = new RandomControllerExecutor();
                newController.Setup(executor, infos, randomWeights, animations);

                brain.AnimControllers.Add(controller.name, newController);

                var firstAnim = animations[infos[0].name];
                firstAnim.SetTime(0);
                firstAnim.GetOutput(0).SetInputWeight(firstAnim, 1);
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
            if (parent.input.HasValue) {
                parent.input.Value.AddInput(playable, 0);
                parent.input.Value.SetInputWeight(playable, weight);

            } else {
                parent.output.Value.SetSourcePlayable(playable);
                parent.output.Value.SetWeight(weight);
            }
        }

        private int GetPortIndex(GraphNode parent) {
            if (parent.input.HasValue) {
                return parent.input.Value.GetInputCount() - 1;
            } else {
                return 0;
            }
        }

        private void OnDestroy() {
            if (graph.IsValid()) {
                graph.Destroy();
                foreach (var item in jobsTemp) {
                    item.DisposeData();
                }
            }
        }
    }
}