using System.Collections.Generic;
using UnityEngine;
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
        public void Add(AnimationClipPlayable playable, float transitionDuration) {
            var node = new PlayableNode(playable, transitionDuration);
            if (Tail != null) {
                Tail.Next = node;
                Tail = node;
            } else {
                Head = node;
                Tail = node;
            }
            Count++;
        }

        public void Add(AnimationClipPlayable playable, float transitionDuration, int index) {
            var newNode = new PlayableNode(playable, transitionDuration);
            index = Mathf.Clamp(index, 1, Count);
            PlayableNode previousNode = FindNode(index - 1);
            if (previousNode == null || previousNode == Tail) {
                Add(playable, transitionDuration);
            } else {
                newNode.Next = previousNode.Next;
                previousNode.Next = newNode;
            }
        }

        public PlayableNode FindNode(int index) {
            if (index > Count) {
                return null;
            }

            var current = Head;
            for (int i = 0; i < index; i++) {
                current = current.Next;
            }
            return current;
        }

        public int FindNodeIndex(PlayableNode node) {
            int index = 0;
            var current = Head;
            while (current!=node) {
                current = current.Next;
                index++;
            }
            return index;
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
