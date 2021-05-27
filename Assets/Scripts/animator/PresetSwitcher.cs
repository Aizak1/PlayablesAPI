using System.Collections.Generic;
using UnityEngine;


namespace animator {
    public class PresetSwitcher : MonoBehaviour {
        [SerializeField]
        private KeyCode nextPresetCode;
        [SerializeField]
        private KeyCode previousPresetCode;
        [SerializeField]
        private PlayablesAnimator animator;
        [SerializeField]
        private ControllerGroupName[] controllerGroupNames;

        private int groupIndex;

        private void Update() {
            if (animator.Brain == null) {
                return;
            }

            if (controllerGroupNames == null) {
                return;
            }

            if (Input.GetKeyDown(nextPresetCode)) {
                groupIndex += 1;
                if (groupIndex >= controllerGroupNames.Length) {
                    groupIndex = 0;
                }
                animator.Brain.ActivateArrayOfControllers(controllerGroupNames[groupIndex].names);
            }
            if (Input.GetKeyDown(previousPresetCode)) {
                groupIndex -= 1;
                if (groupIndex < 0) {
                    groupIndex = controllerGroupNames.Length - 1;
                }
                animator.Brain.ActivateArrayOfControllers(controllerGroupNames[groupIndex].names);
            }
        }
    }

    [System.Serializable]
    public struct ControllerGroupName {
        public string[] names;
    }
}
