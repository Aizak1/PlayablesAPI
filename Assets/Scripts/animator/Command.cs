using System.Collections.Generic;

namespace animator {

    public struct Command {
        public AddInputCommand? AddInput;
        public AddControllerCommand? AddContoller;
        public AddOutputCommand? AddOutput;

        public ChangeControllersStateCommand? ChangeControllersState;
        public ChangeWeightCommand? ChangeWeight;
        public SetLayerMaskCommand? SetLayerMask;
    }

    public struct AddInputCommand {
        public AnimationInput AnimationInput;
    }

    public struct AddControllerCommand {
        public AnimationController AnimationController;
    }

    public struct AddOutputCommand {
        public AnimationOutput AnimationOutput;
    }

    public struct ChangeControllersStateCommand {
        public EnableControllersCommand? EnableControllers;
        public DisableControllersCommand? DisableControllers;

        public List<string> controllerNames;
    }

    public struct ChangeWeightCommand {
        public string name;
        public string parent;
        public float weight;
    }

    public struct SetLayerMaskCommand {
        public string maskName;
        public bool isAdditive;
        public List<string> animationNames;
    }

    public struct EnableControllersCommand {
    }

    public struct DisableControllersCommand {
    }
}