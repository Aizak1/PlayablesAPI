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
        public AnimationInput animationInput;
    }

    public struct AddControllerCommand {
        public AnimationController animationController;
    }

    public struct AddOutputCommand {
        public AnimationOutput animationOutput;
    }

}