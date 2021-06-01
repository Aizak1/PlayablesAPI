using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using vjp;

namespace animator {

    [System.Serializable]
    public struct InputData {
        public List<Command> commands;
    }

    public class AnimatorDataParser : MonoBehaviour {


        private const string INPUT_DATA = "InputData";
        private const string COMMANDS = "commands";

        private const string ADD_INPUT = "AddInput";
        private const string ADD_CONTROLLER = "AddController";
        private const string ADD_OUTPUT = "AddOutput";

        private const string PARENT = "parent";
        private const string NAME = "name";

        private const string ANIMATION_CLIP = "animationClip";
        private const string CLIP_NAME = "clipName";
        private const string MASK_NAME = "maskName";
        private const string TRANSITION_DURATION = "transitionDuration";
        private const string IS_ADDITIVE = "isAdditive";

        private const string ANIMATION_INPUT = "animationInput";
        private const string ANIMATION_OUTPUT = "animationOutput";
        private const string ANIMATION_MIXER = "animationMixer";
        private const string ANIMATION_LAYER_MIXER = "animationLayerMixer";
        private const string ANIMATION_BRAIN = "animationBrain";

        private const string ANIMATION_JOB = "animationJob";
        private const string LOOK_AT_JOB = "lookAtJob";
        private const string TWOBONE_IK_JOB = "twoBoneIKJob";
        private const string DAMPING_JOB = "dampingJob";

        private const string ANIMATION_CONTROLLER = "animationController";
        private const string RANDOM_WEIGHTS = "randomWeights";
        private const string IS_CLOSE = "isClose";
        private const string ANIMATION_NAMES = "animationNames";

        private const string WEIGHT_CONTROLLER = "weightController";
        private const string CIRCLE_CONTROLLER = "circleController";
        private const string RANDOM_CONTROLLER = "randomController";

        private const string INITIAL_WEIGHT = "initialWeight";


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
                commands = new List<Command>()
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
                inputData.commands.Add(command);
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
            addOutputCommand.animationOutput = animationOutput;

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
                weightController.circleController = circleController;


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
                weightController.randomController = randomController;

            } else {
                Debug.LogError("No controller in Weight controller");
            }


            animationController.weightController = weightController;
            addControllerCommand.animationController = animationController;

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
                animationInput.animationClip = animationClipInput;
                animInput.animationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_MIXER)) {

                var animationMixerInput = new AnimationMixerInput();
                animationInput.animationMixer = animationMixerInput;
                animInput.animationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_LAYER_MIXER)) {

                var animationMixerLayerInput = new AnimationLayerMixerInput();

                animationInput.animationLayerMixer = animationMixerLayerInput;
                animInput.animationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_JOB)) {

                if (animInputDict[ANIMATION_JOB].Obj.IsNone()) {
                    Debug.LogError("Animation Job field is Empty");
                    return null;
                }

                var animationJobDict = animInputDict[ANIMATION_JOB].Obj.Peel();
                var animationJobInput = new AnimationJobInput();

                if (animationJobDict.ContainsKey(LOOK_AT_JOB)) {
                    animationJobInput.lookAtJob = new LookAtJobInput();

                } else if (animationJobDict.ContainsKey(TWOBONE_IK_JOB)) {
                    animationJobInput.twoBoneIKJob = new TwoBoneIKJobInput();

                } else if (animationJobDict.ContainsKey(DAMPING_JOB)) {
                    animationJobInput.dampingJob = new DampingJobInput();

                } else {
                    Debug.LogError("Unknown job");
                    return null;

                }

                animationInput.animationJob = animationJobInput;
                animInput.animationInput = animationInput;

            } else if (animInputDict.ContainsKey(ANIMATION_BRAIN)) {
                var animationBrainInput = new AnimationBrainInput();
                animationInput.animationBrain = animationBrainInput;
                animInput.animationInput = animationInput;

            } else {
                Debug.LogError("Unknown AnimationInput");
                return null;
            }


            return animInput;
        }
    }
}