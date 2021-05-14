using System.Collections.Generic;
using UnityEngine;


namespace animator {

    public class Resource : MonoBehaviour {
        public Dictionary<string, AnimationClip> animationPairs
            = new Dictionary<string, AnimationClip>();
    }
}

