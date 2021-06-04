using UnityEngine.Playables;

namespace animator {
    public interface IController {

        public void Enable();

        public void Disable();

        public void ProcessLogic(Playable owner, FrameData info);
    }
}