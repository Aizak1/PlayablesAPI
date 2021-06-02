
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator {
    public class GraphLoadTester : MonoBehaviour {
        [SerializeField]
        private KeyCode setupKey;
        [SerializeField]
        private KeyCode addKey;
        [SerializeField]
        private PlayablesAnimator playablesAnimator;
        [SerializeField]
        private AnimatorDataParser parser;
        [SerializeField]
        private TextAsset jsonFile;

        private bool isSetup;
        private bool isAdd;

        private void Update() {
            if (Input.GetKeyDown(setupKey)) {
                isSetup = true;
            }

            if (Input.GetKeyDown(addKey)) {
                isAdd = true;
            }

            if (isSetup || isAdd) {
                if (jsonFile == null) {
                    Debug.LogError("There is no jsonFile");
                    return;
                }
                var optionInputData = parser.LoadInputData(jsonFile.text);
                if (optionInputData.IsNone()) {
                    Debug.LogError("Input data is empty");
                    return;
                }
                var inputData = optionInputData.Peel();
                var commands = inputData.Commands;
                if (isSetup) {

                    playablesAnimator.Setup(commands);
                    isSetup = false;

                } else {
                    playablesAnimator.AddNewCommands(commands);
                    isAdd = false;
                }
            }

        }
    }
}

