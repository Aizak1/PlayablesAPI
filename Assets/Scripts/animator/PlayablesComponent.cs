using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesComponent : MonoBehaviour {

        [SerializeField]
        private TextAsset jsonFile;
        [SerializeField]
        private Resource resource;
        [SerializeField]
        private AnimatorData animatorData;

        private LinkedListNode<AnimationClipPlayable> currentAnimationNode;
        private LinkedListNode<AnimationClipPlayable> nextAnimationNode;

        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;

        private LinkedList<AnimationClipPlayable> animationList;

        private float nextTransitionTime = 0;
        private float currentAnimationEndTime = 0;


        private void Start() {
            animationList = new LinkedList<AnimationClipPlayable>();
            graph = PlayableGraph.Create();
            graph.Play();
            mixer = AnimationMixerPlayable.Create(graph);

            if(jsonFile != null) {
                animatorData = JsonUtility.FromJson<AnimatorData>(jsonFile.text);
            }

            var playablesclips = new AnimationClipPlayable[animatorData.animationsName.Length];
            for (int i = 0; i < animatorData.animationsName.Length; i++) {
                var clip = resource.animationPairs[animatorData.animationsName[i]];

                if (clip == null) {
                    ShowException("There is such animation in resources");
                }

                playablesclips[i] = AnimationClipPlayable.Create(graph, clip);
                playablesclips[i].Pause();
            }


            if(animatorData.sequence == null || animatorData.sequence.Length == 0) {
                foreach (var item in playablesclips) {
                    animationList.AddLast(item);
                }
            } else {
                foreach (var number in animatorData.sequence) {
                    var animationPosition = number - 1;
                    if (animationPosition>= playablesclips.Length) {
                        ShowException("Invalid sequence");
                    }
                    animationList.AddLast(playablesclips[animationPosition]);

                }
            }

            if (animationList.Count == 0) {
                ShowException("AnimationClips list is empty");
            }


            if (animatorData.startTransitionMultiplier == 0) {
                animatorData.startTransitionMultiplier = 1;
            }

            AnimationPlayableOutput animOutput =
                AnimationPlayableOutput.Create(graph, "output", GetComponent<Animator>());
            var source = animOutput.GetSourceOutputPort();

            foreach (var item in animationList) {
                mixer.AddInput(item, source);
            }

            animOutput.SetSourcePlayable(mixer);

            currentAnimationNode = animationList.First;
            mixer.SetInputWeight(currentAnimationNode.Value, 1);
            currentAnimationNode.Value.Play();
            SetNewTransitionTime();

        }


        private void Update() {
            if (Time.time >= nextTransitionTime) {
                if (currentAnimationNode.Next == null) {
                    if (animatorData.isLooping) {
                        nextAnimationNode = animationList.First;
                    } else {
                        return;
                    }
                } else {
                    nextAnimationNode = currentAnimationNode.Next;
                }

                if (Time.time > currentAnimationEndTime) {
                    ChangeAnimation(nextAnimationNode);
                    return;
                }

                float weight = (Time.time - nextTransitionTime)
                        / (currentAnimationEndTime - nextTransitionTime);

                SpreadWeight(weight, currentAnimationNode, nextAnimationNode);
            }
        }

        private void SpreadWeight(float weight,
            LinkedListNode<AnimationClipPlayable> currentElement,
            LinkedListNode<AnimationClipPlayable> nextElenemt
            ){
            mixer.SetInputWeight(currentElement.Value, 1 - weight);
            mixer.SetInputWeight(nextElenemt.Value, weight);
        }

        private void ChangeAnimation(LinkedListNode<AnimationClipPlayable> nextElenemt) {
            mixer.SetInputWeight(currentAnimationNode.Value, 0);

            currentAnimationNode = nextElenemt;
            mixer.SetInputWeight(currentAnimationNode.Value, 1);
            currentAnimationNode.Value.SetTime(0);
            currentAnimationNode.Value.Play();

            SetNewTransitionTime();
        }

        private void SetNewTransitionTime() {
            nextTransitionTime = Time.time +
                currentAnimationNode.Value.GetAnimationClip().length *
                animatorData.startTransitionMultiplier;

            currentAnimationEndTime = Time.time +
                currentAnimationNode.Value.GetAnimationClip().length;
        }

        private void ShowException(string exceptionText) {
            enabled = false;
            Debug.LogError(exceptionText);
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}