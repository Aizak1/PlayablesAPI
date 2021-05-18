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
            }

            if (Input.GetKeyDown(previousPresetCode)) {
            }
        }
    }
}
