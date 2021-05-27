using UnityEngine;


namespace animator {
    public class PresetSwitcher : MonoBehaviour {
        [SerializeField]
        private KeyCode nextPresetCode;
        [SerializeField]
        private KeyCode previousPresetCode;
        [SerializeField]
        private PlayablesAnimator animator;
        private void Update() {
            if (animator.Brain == null) {
                return;
            }
            if (Input.GetKeyDown(nextPresetCode)) {
                animator.Brain.ActivateNextController();
            }
            if (Input.GetKeyDown(previousPresetCode)) {
                animator.Brain.ActivatePreviousController();
            }
        }
    }
}
