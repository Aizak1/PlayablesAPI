using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace animator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesComponent : MonoBehaviour {
        [SerializeField]
        private AnimationClip animationToPlay;
        private PlayableGraph graph;

        private AnimationMixerPlayable mixer;

        private void Start() {
            graph = PlayableGraph.Create();
            graph.Play();
            mixer = AnimationMixerPlayable.Create(graph);

            AnimationClipPlayable playableClip =
                AnimationClipPlayable.Create(graph, animationToPlay);


            AnimationPlayableOutput animOutput =
                AnimationPlayableOutput.Create(graph, "output",GetComponent<Animator>());

            mixer.AddInput(playableClip, animOutput.GetSourceOutputPort());

            animOutput.SetSourcePlayable(mixer);

            float weight = 1;
            int animationIndex = 0;

            mixer.SetInputWeight(animationIndex, weight);
        }


        private void OnDestroy() {
            graph.Destroy();
        }
    }
}


