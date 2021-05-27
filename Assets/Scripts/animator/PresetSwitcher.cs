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
        private ControllerGroup[] controllerGroups;

        private int groupIndex = -1;

        private void Update() {
            if (animator.Brain == null) {
                return;
            }

            if (controllerGroups == null) {
                return;
            }

            if (Input.GetKeyDown(nextPresetCode)) {
                groupIndex += 1;
                if (groupIndex >= controllerGroups.Length) {
                    groupIndex = 0;
                }
                animator.Brain.ActivateControllers(controllerGroups[groupIndex].ControllerNames);
            }
            if (Input.GetKeyDown(previousPresetCode)) {
                groupIndex -= 1;
                if (groupIndex < 0) {
                    groupIndex = controllerGroups.Length - 1;
                }
                animator.Brain.ActivateControllers(controllerGroups[groupIndex].ControllerNames);
            }
        }
    }

    [System.Serializable]
    public struct ControllerGroup {
        public string[] ControllerNames;
    }
}
