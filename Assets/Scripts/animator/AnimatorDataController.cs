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
            ShowException("No json file");
        }

        if (animator == null) {
            ShowException("Animator is not attached");
        }

        animatorData = JsonUtility.FromJson<AnimatorData>(jsonFile.text);
        animator.AnimationClips = new List<AnimationClip>();
        foreach (string item in animatorData.animationsName) {

            string[] guidPath = AssetDatabase.FindAssets(item);
            string path = AssetDatabase.GUIDToAssetPath(guidPath[0]);

            AnimationClip animationClip =
                (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));

            animator.AnimationClips.Add(animationClip);
        }

        animator.Sequence = animatorData.sequence;
        animator.StartTransitionMultiplier = animatorData.startTransitionMultiplier;
        animator.IsLooping = animatorData.isLooping;
    }

    private void ShowException(string exceptionText) {
        enabled = false;
        Debug.LogError(exceptionText);
    }
}
