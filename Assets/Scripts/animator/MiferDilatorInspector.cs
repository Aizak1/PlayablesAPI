using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace animator {
    [CustomEditor(typeof(MixerDilator))]
    public class MiferDilatorInspector : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            MixerDilator mixerDilator = (MixerDilator)target;
            if (GUILayout.Button("Add Node")) {
                mixerDilator.AddNodeOnButton();
            }
        }
    }
}