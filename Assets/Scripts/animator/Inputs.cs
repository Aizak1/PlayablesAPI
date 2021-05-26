
namespace animator {
    public struct AnimationInput {
        public string Parent;
        public string Name;
        public AnimationClipInput? AnimationClip;
        public AnimationMixerInput? AnimationMixer;
        public AnimationLayerMixerInput? AnimationLayerMixer;
        public AnimationJobInput? AnimationJob;
        public AnimationBrainInput? AnimationBrain;
    }

    public struct AnimationBrainInput {

    }

    public struct AnimationJobInput {
        public LookAtJobInput? LookAtJob;
        public TwoBoneIKJobInput? TwoBoneIKJob;
    }

    public struct LookAtJobInput {

    }

    public struct TwoBoneIKJobInput {

    }

    public struct AnimationClipInput {
        public float TransitionDuration;
        public string ControllerName;
    }

    public struct AnimationMixerInput {
    }

    public struct AnimationLayerMixerInput {
    }
}

