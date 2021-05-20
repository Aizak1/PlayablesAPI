using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public class PlayableNode {
        public AnimationClipPlayable PlayableClip;
        public float TransitionDuration;
        public Playable Parent;
        public PlayableNode Next;

    }
}
