using System.Collections.Generic;

namespace animator {

    public interface IAdditionalController {
        public int GetNextNode(List<ClipNode> nodes, int currentIndex);
    }

}
