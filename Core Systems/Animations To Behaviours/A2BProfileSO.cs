using UnityEngine;

[CreateAssetMenu(fileName = "A2BProfile", menuName = "Core Systems/Animation To Behaviour/Profile", order = 1)]
public class A2BProfileSO : ScriptableObject
{
    [Tooltip("Frame index (0-based) in the clip when this event should fire")]
    [SerializeField] private int triggerFrame = 0;

    [Tooltip("Total frames in the clip.")]
    [SerializeField] private int totalFrames = 0;

    [Tooltip("Normalized time (0..1) relative to clip length)")]
    public float NormalizedTime = 0f;

    private void OnValidate()
    {
        // ensure non-negative frameIndex
        if (triggerFrame < 0)
        {
            Debug.LogWarning($"A2BProfileSO '{name}': triggerFrame cannot be negative. Resetting to 0.");
            triggerFrame = 0;
        }
        CalculateNormalizedTime();
    }

    private void CalculateNormalizedTime()
    {
        if (totalFrames > 0)
        {
            NormalizedTime = Mathf.Clamp01((float)triggerFrame / (float)totalFrames);
        }
        else
        {
            NormalizedTime = 0f;
        }
    }
}