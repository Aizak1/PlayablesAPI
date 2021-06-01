using UnityEngine.Playables;


namespace animator {
    public interface IController {
        public void ProcessLogic(Playable owner, FrameData info);
    }
}

