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
            if (Input.GetKeyDown(nextPresetCode)) {
                animator.SwitchPreset(animator.NextPresetIndex + 1);
            }
            if (Input.GetKeyDown(previousPresetCode)) {
                animator.SwitchPreset(animator.NextPresetIndex - 1);
            }
        }
    }
}
