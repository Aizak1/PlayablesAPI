using UnityEngine.Playables;

namespace animator {
    public struct GraphNode {
        public Playable? input;
        public PlayableOutput? output;
        public int inputCount;
    }
}