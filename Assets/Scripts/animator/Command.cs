using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {

    public struct Command {
        public AddInputCommand? AddInput;
        public AddControllerCommand? AddContoller;
        public AddOutputCommand? AddOutput;
    }

    public struct AddInputCommand {
        public AnimationInput AnimationInput;
    }

    public struct AddControllerCommand {
        public AnimationControllerInput ControllerInput;
    }

    public struct AddOutputCommand {
        public AnimationOutput AnimationOutput;
    }

}