using System.Collections.Generic;
using UnityEngine;


namespace animator {

    public class Resource : MonoBehaviour {
        public Dictionary<string, AnimationClip> animations
            = new Dictionary<string, AnimationClip>();
    }
}

