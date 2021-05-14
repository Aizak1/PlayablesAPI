using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace animator {
    [System.Serializable]
    public struct AnimatorData {
        public string[] animationsName;
        public int[] sequence;
        public float startTransitionMultiplier;
        public bool isLooping;
        public bool isRandom;
    }
}

