using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public class PlayableNode {
        public  Playable Playable;
        public float TransitionDuration;
        public PlayableNode Next;

        public PlayableNode(AnimationClipPlayable playable, float transitionDuration) {
            Playable = playable;
            TransitionDuration = transitionDuration;
        }
    }
}
