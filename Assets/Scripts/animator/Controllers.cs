using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {
    public enum ControllerType {
        OpenCircle,
        CloseCircle,
        Random
    }

    public struct AnimationControllerInput {
        public string ControllerType;
        public string Name;
        public List<int> RandomWeights;
    }

    public struct AnimationController {
        public ControllerType ControllerType;
        public List<PlayableAnimation> PlayableAnimations;
        public List<int> RandomWeights;
        public int CurrentAnimationIndex;
        public int NextAnimationIndex;
        public bool isEnable;
    }
}
