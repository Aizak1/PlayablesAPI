using System.Collections.Generic;
using UnityEngine.Playables;

namespace animator {
    public class Brain : PlayableBehaviour {

        public Dictionary<string, IController> AnimControllers;

        public void Initialize() {

            AnimControllers = new Dictionary<string, IController>();
        }

        override public void PrepareFrame(Playable owner, FrameData info) {

            foreach (var item in AnimControllers) {
                item.Value.Process(owner, info);
            }
        }
    }
}