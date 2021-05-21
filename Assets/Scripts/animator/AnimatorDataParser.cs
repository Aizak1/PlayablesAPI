using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using vjp;

namespace animator {

    [System.Serializable]
    public struct InputData {
        public string firstNodeName;
        public Command? commands;
    }

    public class AnimatorDataParser : MonoBehaviour {
        [SerializeField]
        private PlayablesAnimator playablesAnimator;
        [SerializeField]
        private TextAsset jsonFile;
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (jsonFile == null) {
                    Debug.LogError("There is no jsonFile");
                    enabled = false;
                    return;
                }
                Command[] commands = new Command[4];

                AnimationClipInput animationClipInput = new AnimationClipInput();
                animationClipInput.Name = "Clapping";
                animationClipInput.TransitionDuration = 0.1f;

                AnimationInput animationInput = new AnimationInput();
                animationInput.AnimationClip = animationClipInput;
                animationInput.Parent = "main";

                AddInputCommand addInputCommand = new AddInputCommand();
                addInputCommand.AnimationInput = animationInput;

                Command animCommand = new Command();
                animCommand.AddInput = addInputCommand;



                commands[0] = animCommand;

                animationClipInput = new AnimationClipInput();
                animationClipInput.Name = "Flair";
                animationClipInput.TransitionDuration = 0.2f;

                animationInput = new AnimationInput();
                animationInput.AnimationClip = animationClipInput;
                animationInput.Parent = "main";

                addInputCommand = new AddInputCommand();
                addInputCommand.AnimationInput = animationInput;

                animCommand = new Command();
                animCommand.AddInput = addInputCommand;

                commands[1] = animCommand;

                animationClipInput = new AnimationClipInput();
                animationClipInput.Name = "Falling";
                animationClipInput.TransitionDuration = 0.3f;

                animationInput = new AnimationInput();
                animationInput.AnimationClip = animationClipInput;
                animationInput.Parent = "main";

                addInputCommand = new AddInputCommand();
                addInputCommand.AnimationInput = animationInput;

                animCommand = new Command();
                animCommand.AddInput = addInputCommand;

                commands[2] = animCommand;

                CloseCircleController closeController = new CloseCircleController();
                AnimationController animationController = new AnimationController();
                animationController.CloseCircle = closeController;
                AddControllerCommand addControllerCommand = new AddControllerCommand();
                addControllerCommand.Controller = animationController;
                animCommand = new Command();
                animCommand.AddContoller = addControllerCommand;
                commands[3] = animCommand;

                playablesAnimator.Setup("main", commands);
            }
        }
    }
}