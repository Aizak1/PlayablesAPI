using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        public Option<InputData> GetInputDataFromJson(JSONType json) {
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
            var animationOutput = new AnimationOutput();

            if (!outputDict.ContainsKey(NAME)) {
                Debug.LogError("No Name field");
                return null;
            }

            if (outputDict[NAME].Str.IsNone()) {
                Debug.LogError("Name field is empty");
                return null;
            }

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
            var animationController = new AnimationController();

            if (!controllerDict.ContainsKey(NAME)) {
                Debug.LogError("No name field");
                return null;
            }

            if (controllerDict[NAME].Str.IsNone()) {
                Debug.LogError("name field is empty");
                return null;
            }
            animationController.name = controllerDict[NAME].Str.Peel();


            if (!controllerDict.ContainsKey(WEIGHT_CONTROLLER)) {
                Debug.LogError("No weightController field");
                return null;
            }

            if (controllerDict[WEIGHT_CONTROLLER].Obj.IsNone()) {
                Debug.LogError("weightController field is empty");
                return null;
            }

            var weightControllerDict = controllerDict[WEIGHT_CONTROLLER].Obj.Peel();
            var weightController = new WeightController();

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
            weightController.animationNames = animationNames;

            if (weightControllerDict.ContainsKey(CIRCLE_CONTROLLER)) {
                if (weightControllerDict[CIRCLE_CONTROLLER].Obj.IsNone()) {
                    Debug.LogError("circle controller field is empty");
                    return null;
                }
                var circleControllerDict = weightControllerDict[CIRCLE_CONTROLLER].Obj.Peel();
                var circleController = new CircleController();

                if (!circleControllerDict.ContainsKey(IS_CLOSE)) {
                    Debug.LogError("No isClose field");
                    return null;
                }

                if (circleControllerDict[IS_CLOSE].Bool.IsNone()) {
                    Debug.LogError("isClose field is empty");
                    return null;
                }

                circleController.isClose = circleControllerDict[IS_CLOSE].Bool.Peel();
                weightController.CircleController = circleController;


            } else if (weightControllerDict.ContainsKey(RANDOM_CONTROLLER)) {
                if (weightControllerDict[RANDOM_CONTROLLER].Obj.IsNone()) {
                    Debug.LogError("random controller field is empty");
                    return null;
                }
                var randomControllerDict = weightControllerDict[RANDOM_CONTROLLER].Obj.Peel();
                var randomController = new RandomController();

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

                randomController.randomWeights = weights;
                weightController.RandomController = randomController;

            } else {
                Debug.LogError("No controller in Weight controller");
            }


            animationController.WeightController = weightController;
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
            var animationInput = new AnimationInput();

            if (!animInputDict.ContainsKey(PARENT)) {
                Debug.LogError("No Parent field");
                return null;
            }

            if (animInputDict[PARENT].Str.IsNone()) {
                Debug.LogError("Parent field is empty");
                return null;
            }

            animationInput.parent = animInputDict[PARENT].Str.Peel();

            if (!animInputDict.ContainsKey(NAME)) {
                Debug.LogError("No Parent field");
                return null;
            }

            if (animInputDict[NAME].Str.IsNone()) {
                Debug.LogError("Parent field is empty");
                return null;
            }

            animationInput.name = animInputDict[NAME].Str.Peel();

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

            animationInput.initialWeight = initWeight;



            if (animInputDict.ContainsKey(ANIMATION_CLIP)) {

                if (animInputDict[ANIMATION_CLIP].Obj.IsNone()) {
                    Debug.LogError("AnimationClip field is empty");
                    return null;
                }

                var animClip = animInputDict[ANIMATION_CLIP].Obj.Peel();
                var animationClipInput = new AnimationClipInput();

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
                var clipName = animClip[CLIP_NAME].Str.Peel();

                animationClipInput.transitionDuration = duration;
                animationClipInput.clipName = clipName;
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
                    animationJobInput.LookAtJob = new LookAtJobInput();

                } else if (animationJobDict.ContainsKey(TWOBONE_IK_JOB)) {
                    animationJobInput.TwoBoneIKJob = new TwoBoneIKJobInput();

                } else if (animationJobDict.ContainsKey(DAMPING_JOB)) {
                    animationJobInput.DampingJob = new DampingJobInput();

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

            var controllersStateCommand = new ChangeControllersStateCommand();

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
            controllersStateCommand.controllerNames = names;


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

            var changeWeightCommand = new ChangeWeightCommand();

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

            changeWeightCommand.name = changeWeightDict[NAME].Str.Peel();
            changeWeightCommand.parent = changeWeightDict[PARENT].Str.Peel();
            changeWeightCommand.weight = weight;

            return changeWeightCommand;
        }

        private SetLayerMaskCommand? GetSetLayerMaskCommand(
           Dictionary<string, JSONType> layerMaskDict
           ) {

            var layerMaskCommand = new SetLayerMaskCommand();

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

            layerMaskCommand.maskName = layerMaskDict[MASK_NAME].Str.Peel();
            layerMaskCommand.isAdditive = layerMaskDict[IS_ADDITIVE].Bool.Peel();
            layerMaskCommand.animationNames = names;

            return layerMaskCommand;
        }
    }
}