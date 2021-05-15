
namespace animator {
    [System.Serializable]
    public struct GraphData {
        public NodeSetting[] inputNodes;
        public Modificators modificators;
        public int[] weights;
    }

    [System.Serializable]
    public struct NodeSetting {
        public string animation;
        public float transitionDuration;
    }
}

