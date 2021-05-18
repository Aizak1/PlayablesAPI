using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace animator {
    public struct Command {
        public AddOutputCommand? AddOutput;
        public AddInputCommand? AddInput;
        public AddControllerCommand? AddContoller;
    }

    public struct AddInputCommand {
        public string parent;
        public AnimationInput? AnimationInput;
    }

    public struct AddOutputCommand {
        public string parent;
    }

    public struct AddControllerCommand {

    }
    public struct AnimationInput {
        public AnimationClipInput? AnimationClip;
        public AnimationMixerInput? AnimationMixer;
        public AnimationLayerMixerInput? AnimationLayerMixer;
    }

    public struct AnimationClipInput {
        string name;
        float transitionDuration;
    }
    public struct AnimationMixerInput { }

    public struct AnimationLayerMixerInput { }


}


