using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace playablesAnimator {

    [RequireComponent(typeof(Animator))]
    public class PlayablesComponent : MonoBehaviour {
        [SerializeField]
        private AnimationClip animationToPlay;
        private PlayableGraph graph;

        private AnimationMixerPlayable mixer;

        private void Start() {
            graph = PlayableGraph.Create();
            graph.Play();

            AnimationClipPlayable playableClip =
                AnimationClipPlayable.Create(graph, animationToPlay);

            AnimationPlayableOutput animOutput =
                AnimationPlayableOutput.Create(graph, "output",GetComponent<Animator>());



            animOutput.SetSourcePlayable(playableClip);
        }

        private void OnDestroy() {
            graph.Destroy();
        }
    }
}


