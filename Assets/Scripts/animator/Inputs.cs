using System.Collections.Generic;
using UnityEngine;

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
        public List<string> jointPathes;
    }

    public struct LookAtJobInput {
        public string jointPath;

        public float axisX;
        public float axisY;
        public float axisZ;

        public string effectorName;
        public float minAngle;
        public float maxAngle;


    }

    public struct TwoBoneIKJobInput {
        public string jointPath;
        public string effectorName;
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

