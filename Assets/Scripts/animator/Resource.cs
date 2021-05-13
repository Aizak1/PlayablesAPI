using System.Collections.Generic;
using UnityEngine;


namespace animator {
    public class Resource : MonoBehaviour {
        [SerializeField] private  AnimationClip[] animations;
        public Dictionary<string, AnimationClip> animationPairs;

        private void Awake() {
            animationPairs = new Dictionary<string, AnimationClip>();
            foreach (var item in animations) {
                animationPairs.Add(item.name, item);
            }
        }
    }
}

