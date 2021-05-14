using System.Collections.Generic;
using UnityEngine.Animations;

namespace animator {
    public class PlayableNode {
        public AnimationClipPlayable PlayableClip;
        public float TransitionDuration;
        public PlayableNode Next;

        public PlayableNode(AnimationClipPlayable playable, float transitionDuration) {
            PlayableClip = playable;
            TransitionDuration = transitionDuration;
        }
    }

    public class PlayableNodeList {

        public PlayableNode Head;
        public PlayableNode Tail;
        public int Count;

        public PlayableNodeList() {
            Head = null;
            Tail = null;
            Count = 0;
        }
        public void Add(AnimationClipPlayable playable, float transitionTime) {
            var node = new PlayableNode(playable, transitionTime);
            if (Tail != null) {
                Tail.Next = node;
                Tail = node;
            } else {
                Head = node;
                Tail = node;
            }
            Count++;
        }

        public PlayableNode FindPlayableNode(int index) {
            if (index > Count) {
                return null;
            }

            var current = Head;
            for (int i = 0; i < index; i++) {
                current = current.Next;
            }
            return current;
        }

        public IEnumerator<PlayableNode> GetEnumerator() {
            PlayableNode current = Head;
            while (current != null) {
                yield return current;
                current = current.Next;
            }
        }
    }
}
