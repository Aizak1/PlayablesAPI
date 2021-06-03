using System.Collections.Generic;

namespace animator {

    public interface IAdditionalController {
        public int GetNextNode(List<ClipNodeInfo> nodes, int currentIndex);
    }

}
