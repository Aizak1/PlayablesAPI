using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public struct ClipNode {
        public AnimationClipPlayable playableClip;
        public GraphNode parent;
        public float transitionDuration;
        public float animationLength;
        public int portIndex;
    }
}