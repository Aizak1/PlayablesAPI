using System.Collections.Generic;
using UnityEngine.Playables;

namespace animator {
    public class Brain : PlayableBehaviour {

        public Dictionary<string, IController> animControllers;
        public List<string> controllerNames;

        public void Initialize() {

            animControllers = new Dictionary<string, IController>();
            controllerNames = new List<string>();
        }

        override public void PrepareFrame(Playable owner, FrameData info) {

            foreach (var item in controllerNames) {
                animControllers[item].Process(owner, info);
            }
        }
    }
}