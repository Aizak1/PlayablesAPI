using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator {
    public class JsonParser : MonoBehaviour {
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
            playablesAnimator.GraphDatas.Clear();
            foreach (var file in jsonFiles) {
                var graphData = JsonUtility.FromJson<GraphData>(file.text);
                playablesAnimator.GraphDatas.Add(graphData);
            }
        }
    }
}

