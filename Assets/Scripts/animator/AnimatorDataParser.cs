using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using vjp;

namespace animator {

    [System.Serializable]
    public struct InputData {
        public List<Command> Commands;
    }

    public class AnimatorDataParser : MonoBehaviour {

        private const string INPUT_DATA = "InputData";
        private const string COMMANDS = "Commands";

        private const string ADD_INPUT = "AddInput";
        private const string ADD_CONTROLLER = "AddController";
        private const string ADD_OUTPUT = "AddOutput";
        private const string CHANGE_CONTROLLERS_STATE = "ChangeControllersState";
        private const string CHANGE_WEIGHT = "ChangeWeight";
        private const string SET_LAYER_MASK = "SetLayerMask";

        private const string PARENT = "parent";
        private const string NAME = "name";

        private const string ANIMATION_CLIP = "AnimationClip";
        private const string CLIP_NAME = "clipName";
        private const string MASK_NAME = "maskName";
        private const string TRANSITION_DURATION = "transitionDuration";
        private const string IS_ADDITIVE = "isAdditive";

        private const string ANIMATION_INPUT = "AnimationInput";
        private const string ANIMATION_OUTPUT = "AnimationOutput";
        private const string ANIMATION_MIXER = "AnimationMixer";
        private const string ANIMATION_LAYER_MIXER = "AnimationLayerMixer";
        private const string ANIMATION_BRAIN = "AnimationBrain";

        private const string ANIMATION_JOB = "AnimationJob";
        private const string LOOK_AT_JOB = "LookAtJob";
        private const string TWOBONE_IK_JOB = "TwoBoneIKJob";
        private const string DAMPING_JOB = "DampingJob";

        private const string JOINT_PATH = "jointPath";
        private const string JOINT_PATHES = "jointPathes";

        private const string AXIS_X = "axisX";
        private const string AXIS_Y = "axisY";
        private const string AXIS_Z = "axisZ";

        private const string EFFECTOR_NAME = "effectorName";
        private const string MIN_ANGLE = "minAngle";
        private const string MAX_ANGLE = "maxAngle";

        private const string ANIMATION_CONTROLLER = "AnimationController";
        private const string RANDOM_WEIGHTS = "randomWeights";
        private const string IS_CLOSE = "isClose";
        private const string ANIMATION_NAMES = "animationNames";

        private const string WEIGHT_CONTROLLER = "WeightController";
        private const string CIRCLE_CONTROLLER = "CircleController";
        private const string RANDOM_CONTROLLER = "RandomController";

        private const string INITIAL_WEIGHT = "initialWeight";
        private const string WEIGHT = "weight";

        private const string ENABLE_CONTROLLERS = "EnableControllers";
        private const string DISABLE_CONTROLLERS = "DisableControllers";
        private const string CONTROLLER_NAMES = "controllerNames";

        public Option<InputData> LoadInputData(string text) {
            Result<JSONType, JSONError> typeRes = VJP.Parse(text, 1024);

            if (typeRes.IsErr()) {
                JSONError error = typeRes.AsErr();
                Debug.LogError(error.type);
                return Option<InputData>.None();
            }

            JSONType type = typeRes.AsOk();

            if (type.Obj.IsNone()) {
                Debug.LogError("JSON file is Empty");
                return Option<InputData>.None();
            }

            var obj = type.Obj.Peel();

            if (!obj.ContainsKey(INPUT_DATA)) {
                Debug.LogError("No Input Data field");
                return Option<InputData>.None();
            }

            JSONType json = obj[INPUT_DATA];
            Option<InputData> optionInputData = GetInputDataFromJson(json);

            if (optionInputData.IsNone()) {
                Debug.LogError("Incorrect input data");
                return Option<InputData>.None();
            }

            return optionInputData;
        }

        private Option<InputData> GetInputDataFromJson(JSONType json) {
            InputData inputData = new InputData {
                Commands = new List<Command>()
            };

            if (json.Obj.IsNone()) {
                Debug.LogError("Input Data field is Empty");
                return Option<InputData>.None();
            }

            var input = json.Obj.Peel();

            if (!input.ContainsKey(COMMANDS)) {
                Debug.LogError("No Commands field");
                return Option<InputData>.None();
            }

            if (input[COMMANDS].Arr.IsNone()) {
                Debug.LogError("Commands field is empty");
                return Option<InputData>.None();
            }

            List<JSONType> commands = input[COMMANDS].Arr.Peel();

            foreach (var item in commands) {

                if (item.Obj.IsNone()) {
                    Debug.LogError("Wrong Command type");
                    continue;
                }

                Command command = GetCommand(item.Obj.Peel());
                inputData.Commands.Add(command);
            }

            return Option<InputData>.Some(inputData);
        }

        private Command GetCommand(Dictionary<string, JSONType> inputCommand) {
            Command command = new Command();
            if (inputCommand.ContainsKey(ADD_INPUT)) {

                if (inputCommand[ADD_INPUT].Obj.IsNone()) {
                    Debug.LogError("AddInput field is empty");
                    return command;
                }

                var dict = inputCommand[ADD_INPUT].Obj.Peel();
                command.AddInput = GetAnimationInput(dict);

            } else if (inputCommand.ContainsKey(ADD_CONTROLLER)) {

                if (inputCommand[ADD_CONTROLLER].Obj.IsNone()) {
                    Debug.LogError("AddController field is empty");
                    return command;
                }

                var dict = inputCommand[ADD_CONTROLLER].Obj.Peel();
                command.AddContoller = GetController(dict);

            }else if (inputCommand.ContainsKey(ADD_OUTPUT)) {

                if (inputCommand[ADD_OUTPUT].Obj.IsNone()) {
                    Debug.LogError("AddOutput field is empty");
                    return command;
                }

                var dict = inputCommand[ADD_OUTPUT].Obj.Peel();
                command.AddOutput = GetOutput(dict);

            } else if (inputCommand.ContainsKey(CHANGE_CONTROLLERS_STATE)) {

                if (inputCommand[CHANGE_CONTROLLERS_STATE].Obj.IsNone()) {
                    Debug.LogError("AddController field is empty");
                    return command;
                }

                var dict = inputCommand[CHANGE_CONTROLLERS_STATE].Obj.Peel();
                command.ChangeControllersState = GetControllersStateCommand(dict);

            } else if (inputCommand.ContainsKey(CHANGE_WEIGHT)) {

                if (inputCommand[CHANGE_WEIGHT].Obj.IsNone()) {
                    Debug.LogError("AddController field is empty");
                    return command;
                }

                var dict = inputCommand[CHANGE_WEIGHT].Obj.Peel();
                command.ChangeWeight = GetChangeWeightCommand(dict);

            } else if (inputCommand.ContainsKey(SET_LAYER_MASK)) {

                if (inputCommand[SET_LAYER_MASK].Obj.IsNone()) {
                    Debug.LogError("AddController field is empty");
                    return command;
                }

                var dict = inputCommand[SET_LAYER_MASK].Obj.Peel();
                command.SetLayerMask = GetSetLayerMaskCommand(dict);

            } else {
                Debug.LogError("Unknown Add command field");
            }

            return command;
        }

        private AddOutputCommand? GetOutput(Dictionary<string, JSONType> outputcommandDict) {
            var addOutputCommand = new AddOutputCommand();

            if (!outputcommandDict.ContainsKey(ANIMATION_OUTPUT)) {
                Debug.LogError("No AnimationOutput field");
                return null;
            }

            if (outputcommandDict[ANIMATION_OUTPUT].Obj.IsNone()) {
                Debug.LogError("AnimationOutput field is empty");
                return null;
            }

            var outputDict = outputcommandDict[ANIMATION_OUTPUT].Obj.Peel();

            if (!outputDict.ContainsKey(NAME)) {
                Debug.LogError("No Name field");
                return null;
            }

            if (outputDict[NAME].Str.IsNone()) {
                Debug.LogError("Name field is empty");
                return null;
            }

            var animationOutput = new AnimationOutput();
            animationOutput.name = outputDict[NAME].Str.Peel();
            addOutputCommand.AnimationOutput = animationOutput;

            return addOutputCommand;
        }

        private AddControllerCommand? GetController(Dictionary<string, JSONType> addController) {
            var addControllerCommand = new AddControllerCommand();

            if (!addController.ContainsKey(ANIMATION_CONTROLLER)) {
                Debug.LogError("No animationController field");
                return null;
            }

            if (addController[ANIMATION_CONTROLLER].Obj.IsNone()) {
                Debug.LogError("animationController field is empty");
                return null;
            }

            var controllerDict = addController[ANIMATION_CONTROLLER].Obj.Peel();

            if (!controllerDict.ContainsKey(NAME)) {
                Debug.LogError("No name field");
                return null;
            }

            if (controllerDict[NAME].Str.IsNone()) {
                Debug.LogError("name field is empty");
                return null;
            }

            if (!controllerDict.ContainsKey(WEIGHT_CONTROLLER)) {
                Debug.LogError("No weightController field");
                return null;
            }

            if (controllerDict[WEIGHT_CONTROLLER].Obj.IsNone()) {
                Debug.LogError("weightController field is empty");
                return null;
            }

            var weightControllerDict = controllerDict[WEIGHT_CONTROLLER].Obj.Peel();

            if (!weightControllerDict.ContainsKey(ANIMATION_NAMES)) {
                Debug.LogError("No animation names field");
                return null;
            }

            if (weightControllerDict[ANIMATION_NAMES].Arr.IsNone()) {
                Debug.LogError("animation names field is empty");
                return null;
            }

            List<string> animationNames = new List<string>();
            List<JSONType> names = weightControllerDict[ANIMATION_NAMES].Arr.Peel();

            foreach (var name in names) {
                if (name.Str.IsNone()) {
                    Debug.LogError("Wrong Name");
                    continue;
                }
                animationNames.Add(name.Str.Peel());
            }

            var weightController = new WeightController {
                animationNames = animationNames
            };

            if (weightControllerDict.ContainsKey(CIRCLE_CONTROLLER)) {
                if (weightControllerDict[CIRCLE_CONTROLLER].Obj.IsNone()) {
                    Debug.LogError("circle controller field is empty");
                    return null;
                }

                var circleControllerDict = weightControllerDict[CIRCLE_CONTROLLER].Obj.Peel();

                if (!circleControllerDict.ContainsKey(IS_CLOSE)) {
                    Debug.LogError("No isClose field");
                    return null;
                }

                if (circleControllerDict[IS_CLOSE].Bool.IsNone()) {
                    Debug.LogError("isClose field is empty");
                    return null;
                }

                var circleController = new CircleController {
                    isClose = circleControllerDict[IS_CLOSE].Bool.Peel()
                };

                weightController.CircleController = circleController;

            } else if (weightControllerDict.ContainsKey(RANDOM_CONTROLLER)) {
                if (weightControllerDict[RANDOM_CONTROLLER].Obj.IsNone()) {
                    Debug.LogError("random controller field is empty");
                    return null;
                }
                var randomControllerDict = weightControllerDict[RANDOM_CONTROLLER].Obj.Peel();

                if (!randomControllerDict.ContainsKey(RANDOM_WEIGHTS)) {
                    Debug.LogError("No random weights field");
                    return null;
                }

                if (randomControllerDict[RANDOM_WEIGHTS].Arr.IsNone()) {
                    Debug.LogError("random weights field is empty");
                    return null;
                }

                List<int> weights = new List<int>();
                List<JSONType> weightList = randomControllerDict[RANDOM_WEIGHTS].Arr.Peel();

                foreach (var weight in weightList) {

                    if (weight.Num.IsNone()) {
                        Debug.LogError("Weight is not a number");
                        weights.Add(0);
                        continue;
                    }

                    if (!int.TryParse(weight.Num.Peel(), out int num)) {
                        Debug.LogError("Weight is not a number");
                        weights.Add(0);
                        continue;
                    }

                    weights.Add(num);
                }

                var randomController = new RandomController {
                    randomWeights = weights
                };

                weightController.RandomController = randomController;

            } else {
                Debug.LogError("No controller in Weight controller");
            }

            var animationController = new AnimationController {
                name = controllerDict[NAME].Str.Peel(),
                WeightController = weightController
            };

            addControllerCommand.AnimationController = animationController;

            return addControllerCommand;
        }

        private AddInputCommand? GetAnimationInput(Dictionary<string, JSONType> inputItem) {
            var animInput = new AddInputCommand();

            if (!inputItem.ContainsKey(ANIMATION_INPUT)) {
                Debug.LogError("No AnimationInput field");
                return null;
            }

            if (inputItem[ANIMATION_INPUT].Obj.IsNone()) {
                Debug.LogError("AnimationInput field is empty");
                return null;
            }

            var animInputDict = inputItem[ANIMATION_INPUT].Obj.Peel();

            if (!animInputDict.ContainsKey(PARENT)) {
                Debug.LogError("No Parent field");
                return null;
            }

            if (animInputDict[PARENT].Str.IsNone()) {
                Debug.LogError("Parent field is empty");
                return null;
            }

            if (!animInputDict.ContainsKey(NAME)) {
                Debug.LogError("No Parent field");
                return null;
            }

            if (animInputDict[NAME].Str.IsNone()) {
                Debug.LogError("Parent field is empty");
                return null;
            }

            if (!animInputDict.ContainsKey(INITIAL_WEIGHT)) {
                Debug.LogError("No initialWeight field");
                return null;
            }

            if (animInputDict[INITIAL_WEIGHT].Num.IsNone()) {
                Debug.LogError("initialWeight field is empty ");
                return null;
            }

            string tempInitWeight = animInputDict[INITIAL_WEIGHT].Num.Peel();
            CultureInfo ci = CultureInfo.InvariantCulture;

            if (!float.TryParse(tempInitWeight, NumberStyles.Any, ci, out float initWeight)) {
                Debug.LogError("Animation Transition Duration isn't number");
                return null;
            }

            var animationInput = new AnimationInput {
                parent = animInputDict[PARENT].Str.Peel(),
                name = animInputDict[NAME].Str.Peel(),
                initialWeight = initWeight
            };

            if (animInputDict.ContainsKey(ANIMATION_CLIP)) {

                if (animInputDict[ANIMATION_CLIP].Obj.IsNone()) {
                    Debug.LogError("AnimationClip field is empty");
                    return null;
                }

                var animClip = animInputDict[ANIMATION_CLIP].Obj.Peel();

                if (!animClip.ContainsKey(TRANSITION_DURATION)) {
                    Debug.LogError("No Animation Transition Duration field");
                    return null;
                }

                if (animClip[TRANSITION_DURATION].Num.IsNone()) {
                    Debug.LogError("Animation Transition Duration field is empty");
                    return null;
                }

                string tempDuration = animClip[TRANSITION_DURATION].Num.Peel();

                if (!float.TryParse(tempDuration, NumberStyles.Any, ci, out float duration)) {
                    Debug.LogError("Animation Transition Duration isn't number");
                    return null;
                }

                if (!animClip.ContainsKey(CLIP_NAME)) {
                    Debug.LogError("No Clip Name field");
                    return null;
                }

                if (animClip[CLIP_NAME].Str.IsNone()) {
                    Debug.LogError("Clip Name field is empty");
                    return null;
                }

                var animationClipInput = new AnimationClipInput {
                    transitionDuration = duration,
                    clipName = animClip[CLIP_NAME].Str.Peel()
                };

                animationInput.AnimationClip = animationClipInput;
                animInput.AnimationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_MIXER)) {

                var animationMixerInput = new AnimationMixerInput();
                animationInput.AnimationMixer = animationMixerInput;
                animInput.AnimationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_LAYER_MIXER)) {

                var animationMixerLayerInput = new AnimationLayerMixerInput();
                animationInput.AnimationLayerMixer = animationMixerLayerInput;
                animInput.AnimationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_JOB)) {

                if (animInputDict[ANIMATION_JOB].Obj.IsNone()) {
                    Debug.LogError("Animation Job field is Empty");
                    return null;
                }

                var animationJobDict = animInputDict[ANIMATION_JOB].Obj.Peel();
                var animationJobInput = new AnimationJobInput();

                if (animationJobDict.ContainsKey(LOOK_AT_JOB)) {
                    if (animationJobDict[LOOK_AT_JOB].Obj.IsNone()) {
                        Debug.LogError("Look At Job field is empty");
                        return null;
                    }

                    var lookAtJobDict = animationJobDict[LOOK_AT_JOB].Obj.Peel();

                    if (!lookAtJobDict.ContainsKey(JOINT_PATH)) {
                        Debug.LogError("No joint path field ");
                        return null;
                    }

                    if (lookAtJobDict[JOINT_PATH].Str.IsNone()) {
                        Debug.LogError("joint path field is empty");
                    }

                    if (!lookAtJobDict.ContainsKey(AXIS_X)) {
                        Debug.LogError("No xAxis field");
                        return null;
                    }

                    if (lookAtJobDict[AXIS_X].Num.IsNone()) {
                        Debug.LogError("xAxis field is empty");
                        return null;
                    }

                    if (!lookAtJobDict.ContainsKey(AXIS_Y)) {
                        Debug.LogError("No yAxis field");
                        return null;
                    }

                    if (lookAtJobDict[AXIS_Y].Num.IsNone()) {
                        Debug.LogError("yAxis field is empty");
                        return null;
                    }

                    if (!lookAtJobDict.ContainsKey(AXIS_Z)) {
                        Debug.LogError("No zAxis field");
                        return null;
                    }

                    if (lookAtJobDict[AXIS_Z].Num.IsNone()) {
                        Debug.LogError("zAxis field is empty");
                        return null;
                    }

                    if (!lookAtJobDict.ContainsKey(EFFECTOR_NAME)) {
                        Debug.LogError("No effectorName field ");
                        return null;
                    }

                    if (lookAtJobDict[EFFECTOR_NAME].Str.IsNone()) {
                        Debug.LogError("effectorName field is empty");
                    }

                    if (!lookAtJobDict.ContainsKey(MIN_ANGLE)) {
                        Debug.LogError("No minAngle field");
                        return null;
                    }

                    if (lookAtJobDict[MIN_ANGLE].Num.IsNone()) {
                        Debug.LogError("minAngle field is empty");
                        return null;
                    }

                    if (!lookAtJobDict.ContainsKey(MAX_ANGLE)) {
                        Debug.LogError("No maxAngle field");
                        return null;
                    }

                    if (lookAtJobDict[MAX_ANGLE].Num.IsNone()) {
                        Debug.LogError("maxAngle field is empty");
                        return null;
                    }

                    string tempX = lookAtJobDict[AXIS_X].Num.Peel();
                    string tempY = lookAtJobDict[AXIS_Y].Num.Peel();
                    string tempZ = lookAtJobDict[AXIS_Z].Num.Peel();
                    string tempMinAngle = lookAtJobDict[MIN_ANGLE].Num.Peel();
                    string tempMaxAngle = lookAtJobDict[MAX_ANGLE].Num.Peel();

                    if (!float.TryParse(tempX, NumberStyles.Any, ci, out float xAxis)) {
                        Debug.LogError("xAxis isn't number");
                        return null;
                    }

                    if (!float.TryParse(tempY, NumberStyles.Any, ci, out float yAxis)) {
                        Debug.LogError("yAxis isn't number");
                        return null;
                    }

                    if (!float.TryParse(tempZ, NumberStyles.Any, ci, out float zAxis)) {
                        Debug.LogError("zAxis isn't number");
                        return null;
                    }

                    if (!float.TryParse(tempMinAngle, NumberStyles.Any, ci, out float minAngle)) {
                        Debug.LogError("min angle isn't number");
                        return null;
                    }

                    if (!float.TryParse(tempMaxAngle, NumberStyles.Any, ci, out float maxAngle)) {
                        Debug.LogError("maxAngle  isn't number");
                        return null;
                    }

                    var lookAtJob = new LookAtJobInput {
                        jointPath = lookAtJobDict[JOINT_PATH].Str.Peel(),
                        axisX = xAxis,
                        axisY = yAxis,
                        axisZ = zAxis,
                        effectorName = lookAtJobDict[EFFECTOR_NAME].Str.Peel(),
                        minAngle = minAngle,
                        maxAngle = maxAngle
                    };

                    animationJobInput.LookAtJob = lookAtJob;

                } else if (animationJobDict.ContainsKey(TWOBONE_IK_JOB)) {

                    if (animationJobDict[TWOBONE_IK_JOB].Obj.IsNone()) {
                        Debug.LogError("TwoBoneIkJob field is empty");
                        return null;
                    }

                    var twoBoneIkDict = animationJobDict[TWOBONE_IK_JOB].Obj.Peel();

                    if (!twoBoneIkDict.ContainsKey(JOINT_PATH)) {
                        Debug.LogError("No joint path field ");
                        return null;
                    }

                    if (twoBoneIkDict[JOINT_PATH].Str.IsNone()) {
                        Debug.LogError("joint path field is empty");
                    }

                    if (!twoBoneIkDict.ContainsKey(EFFECTOR_NAME)) {
                        Debug.LogError("No effectorName field ");
                        return null;
                    }

                    if (twoBoneIkDict[EFFECTOR_NAME].Str.IsNone()) {
                        Debug.LogError("effectorName field is empty");
                    }

                    var twoBoneIKInput = new TwoBoneIKJobInput {
                        jointPath = twoBoneIkDict[JOINT_PATH].Str.Peel(),
                        effectorName = twoBoneIkDict[EFFECTOR_NAME].Str.Peel()
                    };

                    animationJobInput.TwoBoneIKJob = twoBoneIKInput;


                } else if (animationJobDict.ContainsKey(DAMPING_JOB)) {
                    if (animationJobDict[DAMPING_JOB].Obj.IsNone()) {
                        Debug.LogError("Look At Job field is empty");
                        return null;
                    }

                    var dampingJobDict = animationJobDict[DAMPING_JOB].Obj.Peel();

                    if (!dampingJobDict.ContainsKey(JOINT_PATHES)) {
                        Debug.LogError("No joint pathes field");
                        return null;
                    }

                    if (dampingJobDict[JOINT_PATHES].Arr.IsNone()) {
                        Debug.LogError("joint pathes field is empty");
                        return null;
                    }

                    List<string> jointPathes = new List<string>();
                    List<JSONType> pathes = dampingJobDict[JOINT_PATHES].Arr.Peel();

                    foreach (var name in pathes) {
                        if (name.Str.IsNone()) {
                            Debug.LogError("Wrong path");
                            continue;
                        }
                        jointPathes.Add(name.Str.Peel());
                    }

                    var dampingJobInput = new DampingJobInput {
                        jointPathes = jointPathes
                    };

                    animationJobInput.DampingJob = dampingJobInput;

                } else {
                    Debug.LogError("Unknown job");
                    return null;
                }

                animationInput.AnimationJob = animationJobInput;
                animInput.AnimationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_BRAIN)) {

                var animationBrainInput = new AnimationBrainInput();
                animationInput.AnimationBrain = animationBrainInput;
                animInput.AnimationInput = animationInput;

            } else {
                Debug.LogError("Unknown AnimationInput");
                return null;
            }

            return animInput;
        }

        private ChangeControllersStateCommand? GetControllersStateCommand(
            Dictionary<string, JSONType> controllerStateCommandDict
            ) {

            if (!controllerStateCommandDict.ContainsKey(CONTROLLER_NAMES)) {
                Debug.LogError("No animationNames field");
                return null;
            }

            if (controllerStateCommandDict[CONTROLLER_NAMES].Arr.IsNone()) {
                Debug.LogError("animationNames field is empty");
                return null;
            }

            List<string> names = new List<string>();
            foreach (var name in controllerStateCommandDict[CONTROLLER_NAMES].Arr.Peel()) {
                if (name.Str.IsNone()) {
                    Debug.LogError("Wrong Name");
                    continue;
                }
                names.Add(name.Str.Peel());
            }

            var controllersStateCommand = new ChangeControllersStateCommand {
                controllerNames = names
            };

            if (controllerStateCommandDict.ContainsKey(ENABLE_CONTROLLERS)) {

                var enableContollersCommand = new EnableControllersCommand();
                controllersStateCommand.EnableControllers = enableContollersCommand;

            } else if (controllerStateCommandDict.ContainsKey(DISABLE_CONTROLLERS)) {

                var disableControllerCommand = new DisableControllersCommand();
                controllersStateCommand.DisableControllers = disableControllerCommand;

            } else {
                Debug.LogError("Unknown controller state command");
                return null;
            }

            return controllersStateCommand;
        }

        private ChangeWeightCommand? GetChangeWeightCommand(
           Dictionary<string, JSONType> changeWeightDict
           ) {

            if (!changeWeightDict.ContainsKey(NAME)) {
                Debug.LogError("No name field");
                return null;
            }

            if (changeWeightDict[NAME].Str.IsNone()) {
                Debug.LogError("name field is empty");
                return null;
            }

            if (!changeWeightDict.ContainsKey(PARENT)) {
                Debug.LogError("No parent field");
                return null;
            }

            if (changeWeightDict[PARENT].Str.IsNone()) {
                Debug.LogError("parent field is empty");
                return null;
            }

            if (!changeWeightDict.ContainsKey(WEIGHT)) {
                Debug.LogError("No weight field");
                return null;
            }

            if (changeWeightDict[WEIGHT].Num.IsNone()) {
                Debug.LogError("weight field is empty");
                return null;
            }

            string tempWeight = changeWeightDict[WEIGHT].Num.Peel();
            CultureInfo ci = CultureInfo.InvariantCulture;

            if (!float.TryParse(tempWeight, NumberStyles.Any, ci, out float weight)) {
                Debug.LogError("Animation Transition Duration isn't number");
                return null;
            }

            var changeWeightCommand = new ChangeWeightCommand {
                name = changeWeightDict[NAME].Str.Peel(),
                parent = changeWeightDict[PARENT].Str.Peel(),
                weight = weight
            };

            return changeWeightCommand;
        }

        private SetLayerMaskCommand? GetSetLayerMaskCommand(
           Dictionary<string, JSONType> layerMaskDict
           ) {

            if (!layerMaskDict.ContainsKey(MASK_NAME)) {
                Debug.LogError("No mask name field");
                return null;
            }

            if (layerMaskDict[MASK_NAME].Str.IsNone()) {
                Debug.LogError("Mask name field is empty");
                return null;
            }

            if (!layerMaskDict.ContainsKey(IS_ADDITIVE)) {
                Debug.LogError("No isAdditive field");
                return null;
            }

            if (layerMaskDict[IS_ADDITIVE].Bool.IsNone()) {
                Debug.LogError("isAdditive field is empty");
                return null;
            }

            if (!layerMaskDict.ContainsKey(ANIMATION_NAMES)) {
                Debug.LogError("No animationNames field");
                return null;
            }

            if (layerMaskDict[ANIMATION_NAMES].Arr.IsNone()) {
                Debug.LogError("animationNames field is empty");
                return null;
            }

            List<string> names = new List<string>();
            foreach (var name in layerMaskDict[ANIMATION_NAMES].Arr.Peel()) {
                if (name.Str.IsNone()) {
                    Debug.LogError("Wrong Name");
                    continue;
                }
                names.Add(name.Str.Peel());
            }

            var layerMaskCommand = new SetLayerMaskCommand {
                maskName = layerMaskDict[MASK_NAME].Str.Peel(),
                isAdditive = layerMaskDict[IS_ADDITIVE].Bool.Peel(),
                animationNames = names
            };

            return layerMaskCommand;
        }
    }
}