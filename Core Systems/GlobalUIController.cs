using UnityEngine;

/// <summary>
/// Global static UI controller for managing UI groups with Canvas Group components
/// Provides centralized show/hide functionality for UI groups throughout the game
/// </summary>
public static class GlobalUIController
{
    #region Show/Hide Group Methods
    
    /// <summary>
    /// Show a UI group by making it visible and interactable
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component to show</param>
    /// <param name="fadeTime">Optional fade duration (0 = instant)</param>
    public static void ShowGroup(CanvasGroup canvasGroup, float fadeTime = 0f)
    {
        if (canvasGroup == null)
        {
            return;
        }
        
        if (fadeTime <= 0f)
        {
            // Instant show
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // Fade in (requires MonoBehaviour for coroutine)
            var fadeHelper = GetOrCreateFadeHelper();
            fadeHelper.FadeGroup(canvasGroup, 1f, fadeTime);
        }
        
    }
    
    /// <summary>
    /// Hide a UI group by making it invisible and non-interactable
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component to hide</param>
    /// <param name="fadeTime">Optional fade duration (0 = instant)</param>
    public static void HideGroup(CanvasGroup canvasGroup, float fadeTime = 0f)
    {
        if (canvasGroup == null)
        {
            return;
        }
        
        if (fadeTime <= 0f)
        {
            // Instant hide
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            // Fade out (requires MonoBehaviour for coroutine)
            var fadeHelper = GetOrCreateFadeHelper();
            fadeHelper.FadeGroup(canvasGroup, 0f, fadeTime);
        }
    }
    
    #endregion
    
    #region Toggle Methods
    
    /// <summary>
    /// Toggle a UI group's visibility state
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component to toggle</param>
    /// <param name="fadeTime">Optional fade duration (0 = instant)</param>
    public static void ToggleGroup(CanvasGroup canvasGroup, float fadeTime = 0f)
    {
        if (canvasGroup == null)
        {
            return;
        }
        
        // Check if group is currently visible
        bool isVisible = canvasGroup.alpha > 0.5f && canvasGroup.interactable;
        
        if (isVisible)
        {
            HideGroup(canvasGroup, fadeTime);
        }
        else
        {
            ShowGroup(canvasGroup, fadeTime);
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if a UI group is currently visible
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component to check</param>
    /// <returns>True if group is visible and interactable</returns>
    public static bool IsGroupVisible(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return false;
        return canvasGroup.alpha > 0.5f && canvasGroup.interactable;
    }
    
    /// <summary>
    /// Set a group's alpha without changing interactable state
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component</param>
    /// <param name="alpha">Alpha value (0-1)</param>
    public static void SetGroupAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup == null)
        {
            return;
        }
        
        canvasGroup.alpha = Mathf.Clamp01(alpha);
    }
    
    /// <summary>
    /// Initialize a group to hidden state (useful for setup)
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component to initialize</param>
    public static void InitializeGroupHidden(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Initialize a group to visible state (useful for setup)
    /// </summary>
    /// <param name="canvasGroup">Canvas Group component to initialize</param>
    public static void InitializeGroupVisible(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    #endregion
    
    #region Fade Helper (for fade animations)
    
    private static UIFadeHelper fadeHelper;
    
    /// <summary>
    /// Get or create the fade helper for animations
    /// </summary>
    private static UIFadeHelper GetOrCreateFadeHelper()
    {
        if (fadeHelper == null)
        {
            var helperObject = new GameObject("GlobalUI_FadeHelper");
            fadeHelper = helperObject.AddComponent<UIFadeHelper>();
            Object.DontDestroyOnLoad(helperObject);
        }
        
        return fadeHelper;
    }
    
    #endregion
}

/// <summary>
/// Helper MonoBehaviour for fade animations (since static class can't run coroutines)
/// </summary>
public class UIFadeHelper : MonoBehaviour
{
    /// <summary>
    /// Fade a canvas group to target alpha over time
    /// </summary>
    public void FadeGroup(CanvasGroup canvasGroup, float targetAlpha, float fadeTime)
    {
        StartCoroutine(FadeCoroutine(canvasGroup, targetAlpha, fadeTime));
    }
    
    private System.Collections.IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float targetAlpha, float fadeTime)
    {
        if (canvasGroup == null) yield break;
        
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        // Set initial interactable state
        if (targetAlpha > 0f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaledDeltaTime instead of deltaTime
            float progress = elapsed / fadeTime;
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }
        
        // Final values
        canvasGroup.alpha = targetAlpha;
        
        // Set final interactable state
        if (targetAlpha <= 0f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}