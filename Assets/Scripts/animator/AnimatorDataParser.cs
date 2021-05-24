using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using vjp;

namespace animator {

    [System.Serializable]
    public struct InputData {
        public string firstNodeName;
        public List<Command> commands;
    }

    public class AnimatorDataParser : MonoBehaviour {
        [SerializeField]
        private PlayablesAnimator playablesAnimator;
        [SerializeField]
        private TextAsset jsonFile;

        private const string INPUT_DATA = "InputData";
        private const string FIRST_NODE_NAME = "FirstNodeName";
        private const string COMMANDS = "Commands";
        private const string ADD_INPUT = "AddInput";
        private const string ADD_CONTROLLER = "AddController";
        private const string CONTROLLER = "Controller";
        private const string OPEN_CIRCLE = "OpenCircle";
        private const string CLOSE_CIRCLE = "CloseCircle";
        private const string RANDOM = "Random";
        private const string WEIGHTS = "Weights";
        private const string PARENT = "Parent";
        private const string ANIMATION_CLIP = "AnimationClip";
        private const string NAME = "Name";
        private const string TRANSITION_DURATION = "TransitionDuration";
        private const string ANIMATION_MIXER = "AnimationMixer";
        private const string ANIMATION_LAYER_MIXER = "AnimationLayerMixer";
        private const string ANIMATION_JOB = "AnimationJob";
        private const string LOOK_AT_JOB = "LookAtJob";
        private const string TWOBONE_IK_JOB = "TwoBoneIKJob";

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (jsonFile == null) {
                    Debug.LogError("There is no jsonFile");
                    return;
                }

                Result<JSONType, JSONError> typeRes = VJP.Parse(jsonFile.text, 1024);

                if (typeRes.IsErr()) {
                    JSONError error = typeRes.AsErr();
                    Debug.LogError(error.type);
                    return;
                }

                JSONType type = typeRes.AsOk();

                if (type.Obj.IsNone()) {
                    Debug.LogError("JSON file is Empty");
                    return;
                }

                var obj = type.Obj.Peel();

                if (!obj.ContainsKey(INPUT_DATA)) {
                    Debug.LogError("No Input Data field");
                    return;
                }

                JSONType json = obj[INPUT_DATA];
                Option<InputData> optionInputData = LoadInputDataFromJSON(json);

                if (optionInputData.IsNone()) {
                    Debug.LogError("Incorrect input data");
                    return;
                }

                var inputData = optionInputData.Peel();
                var name = inputData.firstNodeName;
                var commands = inputData.commands;
                playablesAnimator.Setup(name, commands);
            }
        }

        private Option<InputData> LoadInputDataFromJSON(JSONType json) {
            InputData inputData = new InputData();
            inputData.commands = new List<Command>();

            if (json.Obj.IsNone()) {
                Debug.LogError("Input Data field is Empty");
                return Option<InputData>.None();
            }

            var input = json.Obj.Peel();

            if (!input.ContainsKey(FIRST_NODE_NAME)) {
                Debug.LogError("No FirstNodeName field");
                return Option<InputData>.None();
            }

            if (input[FIRST_NODE_NAME].Str.IsNone()) {
                Debug.LogError("FirstNodeName field is empty");
                return Option<InputData>.None();
            }

            string name = input[FIRST_NODE_NAME].Str.Peel();
            inputData.firstNodeName = name;

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

            } else {
                Debug.LogError("Unknown Add command field");
            }

            return command;
        }

        private AddControllerCommand? GetController(Dictionary<string, JSONType> addController) {
            var addControllerCommand = new AddControllerCommand();

            if (!addController.ContainsKey(CONTROLLER)) {
                Debug.LogError("No Controller field");
                return null;
            }

            if (addController[CONTROLLER].Obj.IsNone()) {
                Debug.LogError("Controller field is empty");
                return null;
            }

            var controllerDict = addController[CONTROLLER].Obj.Peel();
            var animationController = new AnimationController();

            if (controllerDict.ContainsKey(OPEN_CIRCLE)) {
                animationController.OpenCircle = new OpenCircleController();

            } else if (controllerDict.ContainsKey(CLOSE_CIRCLE)) {
                animationController.CloseCircle = new CloseCircleController();

            } else if (controllerDict.ContainsKey(RANDOM)) {

                if (controllerDict[RANDOM].Obj.IsNone()) {
                    Debug.LogError("Random Controller field is empty");
                    return null;
                }

                var randomDict = controllerDict[RANDOM].Obj.Peel();
                var randomController = new RandomController();

                if (!randomDict.ContainsKey(WEIGHTS)) {
                    Debug.LogError("No Weights field in Random Controller");
                    return null;
                }

                if (randomDict[WEIGHTS].Arr.IsNone()) {
                    Debug.LogError("Weights field in Random Controller is empty");
                    return null;
                }

                List<JSONType> weightList = randomDict[WEIGHTS].Arr.Peel();
                List<int> weights = new List<int>();

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

                randomController.Weights = weights;

                animationController.Random = randomController;

            } else {
                Debug.LogError("Unknown AnimationController");
                return null;
            }

            addControllerCommand.Controller = animationController;

            return addControllerCommand;
        }

        private AddInputCommand? GetAnimationInput(Dictionary<string, JSONType> inputItem) {
            var animInput = new AddInputCommand();

            if (!inputItem.ContainsKey(PARENT)) {
                Debug.LogError("No Parent field");
                return null;
            }

            if (inputItem[PARENT].Str.IsNone()) {
                Debug.LogError("Parent field is empty");
                return null;
            }

            animInput.Parent = inputItem[PARENT].Str.Peel();


            if (inputItem.ContainsKey(ANIMATION_CLIP)) {

                if (inputItem[ANIMATION_CLIP].Obj.IsNone()) {
                    Debug.LogError("AnimationClip field is empty");
                    return null;
                }

                var animClip = inputItem[ANIMATION_CLIP].Obj.Peel();
                var animationClipInput = new AnimationClipInput();

                if (!animClip.ContainsKey(NAME)) {
                    Debug.LogError("No Animation Name field");
                    return null;
                }

                if (animClip[NAME].Str.IsNone()) {
                    Debug.LogError("Animation Name field is empty");
                    return null;
                }

                animationClipInput.Name = animClip[NAME].Str.Peel();

                if (!animClip.ContainsKey(TRANSITION_DURATION)) {
                    Debug.LogError("No Animation Transition Duration field");
                    return null;
                }

                if (animClip[TRANSITION_DURATION].Num.IsNone()) {
                    Debug.LogError("Animation Transition Duration field is empty");
                    return null;
                }

                string tempDuration = animClip[TRANSITION_DURATION].Num.Peel();
                CultureInfo ci = CultureInfo.InvariantCulture;
                if (!float.TryParse(tempDuration, NumberStyles.Any, ci, out float duration)) {
                    Debug.LogError("Animation Transition Duration isn't number");
                    return null;
                }

                animationClipInput.TransitionDuration = duration;

                animInput.AnimationClip = animationClipInput;

            } else if (inputItem.ContainsKey(ANIMATION_MIXER)) {

                if (inputItem[ANIMATION_MIXER].Obj.IsNone()) {
                    Debug.LogError("Animation Mixer field is Empty");
                    return null;
                }

                var animMixer = inputItem[ANIMATION_MIXER].Obj.Peel();
                var animationMixerInput = new AnimationMixerInput();

                if (!animMixer.ContainsKey(NAME)) {
                    Debug.LogError("No Animation Mixer Name field");
                    return null;
                }

                if (animMixer[NAME].Str.IsNone()) {
                    Debug.LogError("Animation Mixer Name field is Empty");
                    return null;
                }

                animationMixerInput.Name = animMixer[NAME].Str.Peel();

                animInput.AnimationMixer = animationMixerInput;

            } else if (inputItem.ContainsKey(ANIMATION_LAYER_MIXER)) {

                if (inputItem[ANIMATION_LAYER_MIXER].Obj.IsNone()) {
                    Debug.LogError("Animation Layer Mixer field is Empty");
                    return null;
                }

                var animLayerMixer = inputItem[ANIMATION_LAYER_MIXER].Obj.Peel();
                var animationMixerLayerInput = new AnimationLayerMixerInput();

                if (!animLayerMixer.ContainsKey(NAME)) {
                    Debug.LogError("No Animation Layer Mixer Name field");
                    return null;
                }

                if (animLayerMixer[NAME].Str.IsNone()) {
                    Debug.LogError("Animation Layer Mixer Name field is Empty");
                    return null;
                }

                animationMixerLayerInput.Name = animLayerMixer[NAME].Str.Peel();

                animInput.AnimationLayerMixer = animationMixerLayerInput;

            } else if (inputItem.ContainsKey(ANIMATION_JOB)) {

                if (inputItem[ANIMATION_JOB].Obj.IsNone()) {
                    Debug.LogError("Animation Job field is Empty");
                    return null;
                }

                var animationJobDict = inputItem[ANIMATION_JOB].Obj.Peel();
                var animationJobInput = new AnimationJobInput();

                if (!animationJobDict.ContainsKey(NAME)) {
                    Debug.LogError("No Animation Job Name field");
                    return null;
                }

                if (animationJobDict[NAME].Str.IsNone()) {
                    Debug.LogError("Animation Job Name field is Empty");
                    return null;
                }

                animationJobInput.Name = animationJobDict[NAME].Str.Peel();

                if (animationJobDict.ContainsKey(LOOK_AT_JOB)) {
                    animationJobInput.LookAtJob = new LookAtJobInput();

                } else if (animationJobDict.ContainsKey(TWOBONE_IK_JOB)) {
                    animationJobInput.TwoBoneIKJob = new TwoBoneIKJobInput();
                }

                animInput.AnimationJob = animationJobInput;

            } else {
                Debug.LogError("Unknown AnimationInput");
                return null;
            }

            return animInput;
        }
    }
}