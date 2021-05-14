using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator {
    public class JsonParser : MonoBehaviour {
        [SerializeField]
        private PlayablesAnimator playablesAnimator;
        [SerializeField]
        private TextAsset jsonFile;
        private void Awake() {
            if (jsonFile == null) {
                Debug.LogError("There is no jsonFile");
                enabled = false;
                return;
            }
            playablesAnimator.graphData = JsonUtility.FromJson<GraphData>(jsonFile.text);
        }
    }
}

