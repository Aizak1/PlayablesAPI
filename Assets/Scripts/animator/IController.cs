using UnityEngine.Playables;

namespace animator {
    public interface IController {

        public void Process(Playable owner, FrameData info);
    }
}