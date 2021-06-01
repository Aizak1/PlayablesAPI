
namespace animator {
    public struct AnimationInput {
        public string parent;
        public string name;
        public float initialWeight;
        public AnimationClipInput? animationClip;
        public AnimationMixerInput? animationMixer;
        public AnimationLayerMixerInput? animationLayerMixer;
        public AnimationJobInput? animationJob;
        public AnimationBrainInput? animationBrain;
    }

    public struct AnimationBrainInput {

    }

    public struct AnimationJobInput {
        public LookAtJobInput? lookAtJob;
        public TwoBoneIKJobInput? twoBoneIKJob;
        public DampingJobInput? dampingJob;
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

