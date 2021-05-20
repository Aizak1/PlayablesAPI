using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator {

    [System.Serializable]
    public struct InputData {
        public string FirstNodeName;
        public Command? Commands;
    }

    public class AnimatorDataParser : MonoBehaviour {
        [SerializeField]
        private PlayablesAnimator playablesAnimator;
        [SerializeField]
        private TextAsset jsonFile;
        private void Start() {
            if (jsonFile == null) {
                Debug.LogError("There is no jsonFile");
                enabled = false;
                return;
            }

            AnimationClipInput animationClipInput = new AnimationClipInput();
            animationClipInput.Name = "Clapping";
            animationClipInput.TransitionDuration = 0.5f;

            AnimationInput animationInput = new AnimationInput();
            animationInput.AnimationClip = animationClipInput;
            animationInput.Parent = "main";

            AddInputCommand addInputCommand = new AddInputCommand();
            addInputCommand.AnimationInput = animationInput;

            Command animCommand = new Command();
            animCommand.AddInput = addInputCommand;

            Command[] commands = new Command[1];
            commands[0] = animCommand;
            playablesAnimator.Setup("main", commands);

        }
    }
}

