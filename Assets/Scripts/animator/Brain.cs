using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public class Brain : PlayableBehaviour {

        public Dictionary<string, IController> AnimControllers;
        public List<string> ControllerNames;

        public void Initialize() {

            AnimControllers = new Dictionary<string, IController>();
            ControllerNames = new List<string>();
        }

        override public void PrepareFrame(Playable owner, FrameData info) {

            foreach (var item in AnimControllers) {
                item.Value.ProcessLogic(owner, info);
            }
        }
    }
}