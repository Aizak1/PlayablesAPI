using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {
    public struct Preset {
        public PlayableNodeList AnimationList;
        public Modificators Modificators;
        public List<int> randomWeights;
    }

    [RequireComponent(typeof(Animator))]
    public class PlayablesAnimator : MonoBehaviour {

        [SerializeField]
        private Resource resource;

        [SerializeField]
        private float presetTransitionTime;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private int source;

        private Preset[] presets;
        private PlayableNode currentNode;
        private PlayableNode nextNode;

        private float startTransitionTime;
        private float endTransitionTime;

        public List<GraphData> GraphDatas;

        public int CurrentPresetIndex = 0;
        public int NextPresetIndex = 0;

        private void Start() {
            graph = PlayableGraph.Create();
            mixer = AnimationMixerPlayable.Create(graph);
            AnimationPlayableOutput animOutput =
                AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());

            source = animOutput.GetSourceOutputPort();
            animOutput.SetSourcePlayable(mixer);

            presets = new Preset[GraphDatas.Count];
            foreach (var data in GraphDatas) {
                PlayableNodeList animationList = CreateAnimationList(data, resource);

                foreach (var item in animationList) {
                    mixer.AddInput(item.PlayableClip, source);
                }

                if (data.modificators.isLooping) {
                    animationList.Tail.Next = animationList.Head;
                }

                presets[CurrentPresetIndex].AnimationList = animationList;
                presets[CurrentPresetIndex].Modificators = data.modificators;
                presets[CurrentPresetIndex].randomWeights = data.randomWeights;

                CurrentPresetIndex++;
            }

            CurrentPresetIndex = 0;
            currentNode = presets[CurrentPresetIndex].AnimationList.Head;
            mixer.SetInputWeight(currentNode.PlayableClip, 1);
            currentNode.PlayableClip.SetTime(0);

            var duration = currentNode.TransitionDuration;
            var length = currentNode.PlayableClip.GetAnimationClip().length;
            startTransitionTime = CalculateTransitionStartTime(length, duration);
            endTransitionTime = length;

            graph.Play();
        }

        private void Update() {
            float time = (float)currentNode.PlayableClip.GetTime();

            if (nextNode == null) {
                nextNode = TakeNextNode(currentNode);
                if (NextPresetIndex != CurrentPresetIndex) {

                    var length = currentNode.PlayableClip.GetAnimationClip().length;
                    var transitionTime = presetTransitionTime;
                    startTransitionTime = CalculateTransitionStartTime(length, transitionTime);
                    CurrentPresetIndex = NextPresetIndex;

                }
            }

            if (time >= startTransitionTime && nextNode != null) {
                if (time > endTransitionTime) {

                    currentNode = MoveOnNextNode(currentNode, nextNode);
                    nextNode = null;

                    var duration = currentNode.TransitionDuration;
                    var length = currentNode.PlayableClip.GetAnimationClip().length;
                    startTransitionTime = CalculateTransitionStartTime(length, duration);
                    endTransitionTime = length;

                    currentNode.PlayableClip.SetTime(0);

                    return;
                }

                float weight = (time - startTransitionTime) /
                        (endTransitionTime - startTransitionTime);

                SpreadWeight(weight, currentNode, nextNode);
            }
        }

        private PlayableNodeList CreateAnimationList(GraphData graphData, Resource resource) {
            PlayableNodeList animationList = new PlayableNodeList();
            foreach (var item in graphData.inputNodes) {
                AnimationClipPlayable playableClip = CreatePlayableClip(resource, item);
                animationList.Add(playableClip, item.transitionDuration);
            }
            return animationList;
        }

        private AnimationClipPlayable CreatePlayableClip(Resource resource, NodeSetting setting) {
            var clip = resource.animationPairs[setting.animation];
            var playableClip = AnimationClipPlayable.Create(graph, clip);
            playableClip.SetTime(clip.length);
            return playableClip;
        }

        private PlayableNode TakeNextNode(PlayableNode currentNode) {
            if (NextPresetIndex != CurrentPresetIndex) {
                return presets[NextPresetIndex].AnimationList.Head;
            }

            if (presets[CurrentPresetIndex].Modificators.isRandom) {
                return TakeRandomNode(currentNode, presets[CurrentPresetIndex]);
            }

            if (currentNode.Next != null) {
                return currentNode.Next;
            }

            return null;
        }

        private PlayableNode TakeRandomNode(PlayableNode currentNode,Preset currentPreset) {
            int sum = 0;
            int currentNodeIndex = currentPreset.AnimationList.FindNodeIndex(currentNode);
            for (int i = 0; i < currentPreset.randomWeights.Count; i++) {
                if (i == currentNodeIndex) {
                    continue;
                }
                sum += currentPreset.randomWeights[i];
            }

            int index = 0;
            int randomNumber = Random.Range(0, sum + 1);
            int randomWeight = 0;
            for (int i = 0; i < currentPreset.randomWeights.Count; i++) {
                if (i == currentNodeIndex) {
                    continue;
                }
                randomWeight += currentPreset.randomWeights[i];
                if (randomNumber < randomWeight) {
                    index = i;
                    break;
                }
            }
            return currentPreset.AnimationList.FindNode(index);
        }

        private void SpreadWeight(float weight, PlayableNode currentNode, PlayableNode nextNode) {
            mixer.SetInputWeight(currentNode.PlayableClip, 1 - weight);
            mixer.SetInputWeight(nextNode.PlayableClip, weight);
        }

        private PlayableNode MoveOnNextNode(PlayableNode currentNode, PlayableNode nextNode) {
            mixer.SetInputWeight(currentNode.PlayableClip, 0);
            mixer.SetInputWeight(nextNode.PlayableClip, 1);

            return nextNode;
        }

        private float CalculateTransitionStartTime(float length, float transitionDuration) {
            return length - transitionDuration;
        }

        public void SwitchPreset(int index) {
            if (index >= 0 && index < presets.Length) {
                NextPresetIndex = index;
            }
        }

        public void AddNodeAtPosition(
            int presetIndex, int position, NodeSetting nodeSetting, int randomWeight
            ) {
            var playableClip = CreatePlayableClip(resource, nodeSetting);
            var duration = nodeSetting.transitionDuration;
            presets[presetIndex].AnimationList.Add(playableClip, duration, position);
            if (presets[presetIndex].Modificators.isLooping) {
                var tail = presets[presetIndex].AnimationList.Tail;
                var head = presets[presetIndex].AnimationList.Head;
                tail.Next = head;
            }
            if (presets[presetIndex].Modificators.isRandom) {
                presets[presetIndex].randomWeights.Insert(position, randomWeight);
            }
            mixer.AddInput(playableClip, source);
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}