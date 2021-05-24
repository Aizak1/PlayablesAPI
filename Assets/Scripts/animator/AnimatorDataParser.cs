using System.Collections;
using System.Collections.Generic;
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
                } else {
                    JSONType type = typeRes.AsOk();
                    if (type.Obj.IsSome()) {
                        var obj = type.Obj.Peel();
                        if (obj.ContainsKey(INPUT_DATA)) {
                            JSONType json = obj[INPUT_DATA];
                            Option<InputData> optionInputData = LoadInputDataFromJSON(json);
                            if (optionInputData.IsSome()) {
                                var inputData = optionInputData.Peel();
                                var name = inputData.firstNodeName;
                                var commands = inputData.commands;
                                playablesAnimator.Setup(name, commands);
                            } else {
                                Debug.LogError("Incorrect input data");
                                return;
                            }
                        } else {
                            Debug.LogError("No Input Data field");
                            return;
                        }
                    } else {
                        Debug.LogError("JSON file is Empty");
                        return;
                    }
                }
            }
        }

        private Option<InputData> LoadInputDataFromJSON(JSONType json) {
            InputData inputData = new InputData();
            inputData.commands = new List<Command>();
            if (json.Obj.IsSome()) {
                var input = json.Obj.Peel();

                if (input.ContainsKey(FIRST_NODE_NAME)) {
                    if (input[FIRST_NODE_NAME].Str.IsSome()) {
                        string name = input[FIRST_NODE_NAME].Str.Peel();
                        inputData.firstNodeName = name;
                    } else {
                        Debug.LogError("FirstNodeName field is empty");
                        return Option<InputData>.None();
                    }
                } else {
                    Debug.LogError("No FirstNodeName field");
                    return Option<InputData>.None();
                }

                if (input.ContainsKey(COMMANDS)) {
                    if (input[COMMANDS].Arr.IsSome()) {
                        List<JSONType> commands = input[COMMANDS].Arr.Peel();

                        foreach (var item in commands) {
                            if (item.Obj.IsSome()) {
                                Command command = GetCommand(item.Obj.Peel());
                                inputData.commands.Add(command);
                            } else {
                                Debug.LogError("Wrong Command type");
                            }
                        }
                    } else {
                        Debug.LogError("Commands field is empty");
                        return Option<InputData>.None();
                    }
                } else {
                    Debug.LogError("No Commands field");
                    return Option<InputData>.None();
                }

            } else {
                Debug.LogError("Input Data field is Empty");
                return Option<InputData>.None();
            }

            return Option<InputData>.Some(inputData);
        }

        private Command GetCommand(Dictionary<string, JSONType> inputCommand) {
            Command command = new Command();
            if (inputCommand.ContainsKey(ADD_INPUT)) {
                if (inputCommand[ADD_INPUT].Obj.IsSome()) {
                    var dict = inputCommand[ADD_INPUT].Obj.Peel();
                    command.AddInput = GetAnimationInput(dict);
                } else {
                    Debug.LogError("AddInput field is empty");
                }

            } else if (inputCommand.ContainsKey(ADD_CONTROLLER)) {
                if (inputCommand[ADD_CONTROLLER].Obj.IsSome()) {
                    var dict = inputCommand[ADD_CONTROLLER].Obj.Peel();
                    command.AddContoller = GetController(dict);
                } else {
                    Debug.LogError("AddController field is empty");
                }

            } else {
                Debug.LogError("Unknown Add command field");
            }

            return command;
        }

        private AddControllerCommand? GetController(Dictionary<string, JSONType> addController) {
            var addControllerCommand = new AddControllerCommand();
            if (addController.ContainsKey(CONTROLLER)) {
                if (addController[CONTROLLER].Obj.IsSome()) {
                    var controllerDict = addController[CONTROLLER].Obj.Peel();
                    var animationController = new AnimationController();

                    if (controllerDict.ContainsKey(OPEN_CIRCLE)) {
                        animationController.OpenCircle = new OpenCircleController();

                    } else if (controllerDict.ContainsKey(CLOSE_CIRCLE)) {
                        animationController.CloseCircle = new CloseCircleController();

                    } else if (controllerDict.ContainsKey(RANDOM)) {
                        if (controllerDict[RANDOM].Obj.IsSome()) {
                            var randomDict = controllerDict[RANDOM].Obj.Peel();
                            var randomController = new RandomController();

                            if (randomDict.ContainsKey(WEIGHTS)) {
                                if (randomDict[WEIGHTS].Arr.IsSome()) {
                                    List<JSONType> weightList = randomDict[WEIGHTS].Arr.Peel();
                                    List<int> weights = new List<int>();

                                    foreach (var weight in weightList) {
                                        if (weight.Num.IsSome()) {
                                            weights.Add((int)weight.Num.Peel());
                                        } else {
                                            Debug.LogError("Weight is not a number");
                                            weights.Add(0);
                                        }
                                    }

                                    randomController.Weights = weights;

                                    animationController.Random = randomController;
                                } else {
                                    Debug.LogError("Weights field in Random Controller is empty");
                                    return null;
                                }
                            } else {
                                Debug.LogError("No Weights field in Random Controller");
                                return null;
                            }
                        }
                    } else {
                        Debug.LogError("Unknown AnimationController");
                        return null;
                    }

                    addControllerCommand.Controller = animationController;
                } else {
                    Debug.LogError("Controller field is empty");
                    return null;
                }
            } else {
                Debug.LogError("No Controller field");
                return null;
            }

            return addControllerCommand;
        }

        private AddInputCommand? GetAnimationInput(Dictionary<string, JSONType> inputItem) {
            var animInput = new AddInputCommand();

            if (inputItem.ContainsKey(PARENT)) {
                if (inputItem[PARENT].Str.IsSome()) {
                    animInput.Parent = inputItem[PARENT].Str.Peel();

                } else {
                    Debug.LogError("Parent field is empty");
                    return null;
                }
            } else {
                Debug.LogError("No Parent field");
                return null;
            }

            if (inputItem.ContainsKey(ANIMATION_CLIP)) {
                if (inputItem[ANIMATION_CLIP].Obj.IsSome()) {
                    var animClip = inputItem[ANIMATION_CLIP].Obj.Peel();
                    var animationClipInput = new AnimationClipInput();

                    if (animClip.ContainsKey(NAME)) {
                        if (animClip[NAME].Str.IsSome()) {
                            animationClipInput.Name = animClip[NAME].Str.Peel();
                        } else {
                            Debug.LogError("Animation Name field is empty");
                            return null;
                        }
                    } else {
                        Debug.LogError("No Animation Name field");
                        return null;
                    }

                    if (animClip.ContainsKey(TRANSITION_DURATION)) {
                        if (animClip[TRANSITION_DURATION].Num.IsSome()) {
                            float duration = (float)animClip[TRANSITION_DURATION].Num.Peel();
                            animationClipInput.TransitionDuration = duration;
                        } else {
                            Debug.LogError("Animation Transition Duration field is empty");
                            return null;
                        }
                    } else {
                        Debug.LogError("No Animation Transition Duration field");
                        return null;
                    }

                    animInput.AnimationClip = animationClipInput;
                } else {
                    Debug.LogError("AnimationClip field is empty");
                    return null;
                }

            } else if (inputItem.ContainsKey(ANIMATION_MIXER)) {
                if (inputItem[ANIMATION_MIXER].Obj.IsSome()) {
                    var animMixer = inputItem[ANIMATION_MIXER].Obj.Peel();
                    var animationMixerInput = new AnimationMixerInput();
                    if (animMixer.ContainsKey(NAME)) {
                        if (animMixer[NAME].Str.IsSome()) {
                            animationMixerInput.Name = animMixer[NAME].Str.Peel();
                        } else {
                            Debug.LogError("Animation Mixer Name field is Empty");
                            return null;
                        }
                    } else {
                        Debug.LogError("No Animation Mixer Name field");
                        return null;
                    }
                    animInput.AnimationMixer = animationMixerInput;

                } else {
                    Debug.LogError("Animation Mixer field is Empty");
                    return null;
                }

            } else if (inputItem.ContainsKey(ANIMATION_LAYER_MIXER)) {
                if (inputItem[ANIMATION_LAYER_MIXER].Obj.IsSome()) {

                    var animLayerMixer = inputItem[ANIMATION_LAYER_MIXER].Obj.Peel();
                    var animationMixerLayerInput = new AnimationLayerMixerInput();
                    if (animLayerMixer.ContainsKey(NAME)) {
                        if (animLayerMixer[NAME].Str.IsSome()) {
                            animationMixerLayerInput.Name = animLayerMixer[NAME].Str.Peel();
                        } else {
                            Debug.LogError("Animation Layer Mixer Name field is Empty");
                            return null;
                        }
                    } else {
                        Debug.LogError("No Animation Layer Mixer Name field");
                        return null;
                    }
                    animInput.AnimationLayerMixer = animationMixerLayerInput;
                } else {
                    Debug.LogError("Animation Layer Mixer field is Empty");
                    return null;
                }
            } else if (inputItem.ContainsKey(ANIMATION_JOB)) {
                if (inputItem[ANIMATION_JOB].Obj.IsSome()) {
                    var animationJobDict = inputItem[ANIMATION_JOB].Obj.Peel();
                    var animationJobInput = new AnimationJobInput();
                    if (animationJobDict.ContainsKey(NAME)) {
                        if (animationJobDict[NAME].Str.IsSome()) {
                            animationJobInput.Name = animationJobDict[NAME].Str.Peel();
                        }
                    }
                    if (animationJobDict.ContainsKey(LOOK_AT_JOB)) {
                        animationJobInput.LookAtJob = new LookAtJobInput();
                    }
                    animInput.AnimationJob = animationJobInput;
                }
            } else {
                Debug.LogError("Unknown AnimationInput");
                return null;
            }

            return animInput;
        }
    }
}