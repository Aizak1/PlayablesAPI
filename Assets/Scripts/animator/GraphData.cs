
using System.Collections.Generic;

namespace animator {
    [System.Serializable]
    public struct GraphData {
        public NodeSetting[] inputNodes;
        public Modificators modificators;
        public List<int> randomWeights;
    }

    [System.Serializable]
    public struct NodeSetting {
        public string animation;
        public float transitionDuration;
    }
}

