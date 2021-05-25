
namespace animator {
    public struct AnimationInput {
        public string Parent;
        public AnimationClipInput? AnimationClip;
        public AnimationMixerInput? AnimationMixer;
        public AnimationLayerMixerInput? AnimationLayerMixer;
        public AnimationJobInput? AnimationJob;
    }

    public struct AnimationJobInput {
        public string Name;
        public LookAtJobInput? LookAtJob;
        public TwoBoneIKJobInput? TwoBoneIKJob;
    }

    public struct LookAtJobInput {

    }
    public struct TwoBoneIKJobInput {

    }


    public struct AnimationClipInput {
        public string Name;
        public float TransitionDuration;
    }

    public struct AnimationMixerInput {
        public string Name;
    }

    public struct AnimationLayerMixerInput {
        public string Name;
    }
}

