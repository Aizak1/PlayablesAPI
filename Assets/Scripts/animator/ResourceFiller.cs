using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator{
    public class ResourceFiller : MonoBehaviour {
        [SerializeField]
        private AnimationClip[] animationClips;
        [SerializeField]
        private AvatarMask[] avatarMasks;

        [SerializeField]
        private Resource resourceToFill;

        private void Awake() {
            foreach (var item in animationClips) {
                resourceToFill.animations.Add(item.name,item);
            }
            foreach (var item in avatarMasks) {
                resourceToFill.masks.Add(item.name, item);
            }
        }
    }
}

