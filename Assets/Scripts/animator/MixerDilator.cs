using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace animator {
    public class MixerDilator : MonoBehaviour {
        [SerializeField]
        private PlayablesAnimator animator;
        [SerializeField]
        private int presetIndex;
        [SerializeField]
        private int position;
        [SerializeField]
        private NodeSetting nodeSetting;
        [SerializeField]
        private int randomWeight;

        public void AddNodeOnButton() {
            animator.AddNodeAtPosition(presetIndex, position, nodeSetting, randomWeight);
            presetIndex = position = randomWeight = 0;
            nodeSetting.animation = null;
            nodeSetting.transitionDuration = 0;
        }
    }
}

