using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewASM", menuName = "Core Systems/Animation State Mapper")]
public class AnimationStateMapperSO : ScriptableObject
{
    [Tooltip("Name of the object type (e.g., Player, Enemy, Boss)")]
    public string objectTypeName;

    [SerializeField] private List<AnimationStateMap> animationStateMaps = new();
    
    public string PathToSaveGeneratedClass = "Assets/Scripts/Other Systems/Animation State Keys/";

    private Dictionary<string, string> animationStateDictionary;

    public List<AnimationStateMap> GetAnimationStateMaps() => animationStateMaps;

    public string GetGeneratedClassName() => objectTypeName + "AnimationKeys";

    public string this[string key]
    {
        get
        {
            if (animationStateDictionary.TryGetValue(key, out string value))
                return value;
            Debug.LogWarning($"Key '{key}' not found in AnimationStateMapper.");
            return default;
        }
        set
        {
            if (animationStateDictionary.ContainsKey(key))
            {
                animationStateDictionary[key] = value;
                var map = animationStateMaps.Find(m => m.key == key);
                if (map != null)
                    map.value = value;
            }
            else
            {
                Debug.LogWarning($"Key '{key}' does not exist in AnimationStateMapper.");
            }
        }
    }

    private void OnValidate()
    {
        AutoGenerateObjectTypeName();
        // Auto-populate value from clip name if missing
        foreach (var map in animationStateMaps)
        {
            if (map.clip != null && string.IsNullOrEmpty(map.value))
                map.value = map.clip.name;
        }

        // Remove maps with null clips
        animationStateMaps.RemoveAll(m => m.clip == null);

        // Remove duplicate clips
        var seenClips = new HashSet<AnimationClip>();
        animationStateMaps = animationStateMaps
            .Where(m => seenClips.Add(m.clip))
            .ToList();

        InitializeDictionary();
    }

    private void AutoGenerateObjectTypeName()
    {
        if (string.IsNullOrEmpty(objectTypeName))
        {
            if (name.EndsWith("ASM"))
            {
                objectTypeName = name.Replace("ASM", "");
            }
            else
            {
                Debug.LogWarning("Please set the object type name for the AnimationStateMapperSO.");
            }
        }
    }

    private void OnEnable() => InitializeDictionary();

    private void InitializeDictionary()
    {
        animationStateDictionary = new Dictionary<string, string>();
        foreach (var map in animationStateMaps)
        {
            if (map.isIncludedInUsage && !string.IsNullOrEmpty(map.key) && !animationStateDictionary.ContainsKey(map.key))
                animationStateDictionary.Add(map.key, map.value);
            Debug.Log("Added animation state map: " + map.key + " -> " + map.value);
        }
    }
    [ContextMenu("Refresh Dictionary")]
    private void RefreshDictionary()
    {
        InitializeDictionary();
    }
}

[Serializable]
public class AnimationStateMap
{
    public AnimationClip clip;
    [Tooltip("Logical key (will become a constant in the generated AnimationKeys class)")]
    public string key;
    [Tooltip("Animator state name")]
    public string value;
    [Tooltip("Include this animation in usage")]
    public bool isIncludedInUsage = true;

    public AnimationStateMap(AnimationClip clip)
    {
        this.clip = clip;
        key = "";
        value = clip != null ? clip.name : "";
    }
}