using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator {
    public class AnimatorDataParser : MonoBehaviour {
        [SerializeField]
        private PlayablesAnimator playablesAnimator;
        [SerializeField]
        private TextAsset[] jsonFiles;
        private void Awake() {
            if (jsonFiles == null) {
                Debug.LogError("There is no jsonFile");
                enabled = false;
                return;
            }
        }
    }
}

