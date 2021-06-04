using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {
    public class Brain : PlayableBehaviour {

        public Dictionary<string, IController> AnimControllers;

        public void Initialize() {

            AnimControllers = new Dictionary<string, IController>();
        }

        override public void PrepareFrame(Playable owner, FrameData info) {

            foreach (var item in AnimControllers) {
                item.Value.ProcessLogic(owner, info);
            }
        }
    }
}