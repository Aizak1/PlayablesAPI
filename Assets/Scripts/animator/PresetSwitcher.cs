using System.Collections.Generic;
using UnityEngine;


namespace animator {
    public class PresetSwitcher : MonoBehaviour {
        [SerializeField]
        private KeyCode nextPresetKey;
        [SerializeField]
        private KeyCode previousPresetKey;
        [SerializeField]
        private KeyCode setupGraphKey;
        [SerializeField]
        private PlayablesAnimator animator;
        [SerializeField]
        private ControllerGroup[] controllerGroups;

    }

    [System.Serializable]
    public struct ControllerGroup {
        public string[] ControllerNames;
    }
}
