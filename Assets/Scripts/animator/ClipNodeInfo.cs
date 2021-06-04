using UnityEngine.Animations;

namespace animator {
    public struct ClipNodeInfo {
        public AnimationClipPlayable playableClip;
        public GraphNode parent;
        public float transitionDuration;
        public float animationLength;
        public int portIndex;
    }
}