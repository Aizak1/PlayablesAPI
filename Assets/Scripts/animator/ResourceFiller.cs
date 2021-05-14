using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator{
    public class ResourceFiller : MonoBehaviour {
        [SerializeField]
        private AnimationClip[] animationClips;

        [SerializeField]
        private Resource resourceToFill;

       private void Awake() {
            foreach (var item in animationClips) {
                resourceToFill.animationPairs.Add(item.name,item);
            }
        }

}
}

