
namespace animator {
    [System.Serializable]
    public struct GraphData {
        public NodeSetting[] inputNodes;
    }

    [System.Serializable]
    public struct NodeSetting {
        public string animation;
        public float transitionDuration;
    }
}

