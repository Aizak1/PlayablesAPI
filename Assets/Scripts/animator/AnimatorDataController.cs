using animator;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class AnimatorDataController : MonoBehaviour
{
    private AnimatorData animatorData;
    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private PlayablesComponent animator;

    private void Awake() {
        if (jsonFile == null) {
            enabled = false;
            throw new UnityException("No json file");
        }

        if (animator == null) {
            enabled = false;
            throw new UnityException("No animator attached");
        }

        animatorData = JsonUtility.FromJson<AnimatorData>(jsonFile.text);
        List<AnimationClip> animationClips = new List<AnimationClip>();
        foreach (string item in animatorData.animationsName) {

            string[] guidPath = AssetDatabase.FindAssets(item);
            string path = AssetDatabase.GUIDToAssetPath(guidPath[0]);

            AnimationClip animationClip =
                (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));

            animationClips.Add(animationClip);
        }

        animator.AnimationClips = animationClips;
        animator.Sequence = animatorData.sequence;
        animator.StartTransitionMultiplier = animatorData.startTransitionMultiplier;
        animator.IsLooping = animatorData.isLooping;
    }
}
