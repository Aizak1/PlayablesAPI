
namespace animator {
    public struct AnimationInput {
        public string parent;
        public string name;
        public float initialWeight;
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
        public DampingJobInput? DampingJob;
    }

    public struct DampingJobInput {

    }

    public struct LookAtJobInput {

    }

    public struct TwoBoneIKJobInput {

    }

    public struct AnimationClipInput {
        public string clipName;
        public float transitionDuration;
    }

    public struct AnimationMixerInput {
    }

    public struct AnimationLayerMixerInput {
    }
}

