using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public struct ClipNode {
        public AnimationClipPlayable PlayableClip;
        public PlayableParent Parent;
        public float TransitionDuration;
        public float AnimationLength;
    }
}