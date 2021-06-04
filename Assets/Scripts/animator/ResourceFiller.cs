using UnityEngine;

namespace animator{
    public class ResourceFiller : MonoBehaviour {
        [SerializeField]
        private AnimationClip[] animationClips;
        [SerializeField]
        private AvatarMask[] avatarMasks;
        [SerializeField]
        private GameObject[] effectors;
        [SerializeField]
        private Transform[] models;

        [SerializeField]
        private Resource resourceToFill;

        private void Awake() {
            foreach (var item in animationClips) {
                resourceToFill.animations.Add(item.name,item);
            }

            foreach (var item in avatarMasks) {
                resourceToFill.masks.Add(item.name, item);
            }

            foreach (var item in effectors) {
                resourceToFill.effectors.Add(item.name, item);
            }

            foreach (var item in models) {
                resourceToFill.models.Add(item.name, item);
            }

        }
    }
}