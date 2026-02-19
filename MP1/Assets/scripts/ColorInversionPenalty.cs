using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Triggers a color inversion effect for the player by overlaying a full-screen
/// inverted image on the VR camera. Wire TriggerPenalty() to ChairPuzzleManager.onPenaltyTriggered.
///
/// Setup:
///   1. Create a new Canvas:
///        - Hierarchy > right-click > UI > Canvas
///        - Render Mode: Screen Space - Camera
///        - Render Camera: drag in your Main Camera
///        - Plane Distance: 0.1  (very close to camera, in front of everything)
///        - Sort Order: 100  (renders on top)
///   2. Add a UI > Raw Image as a child of the Canvas.
///        - Set color to solid black (RGBA 0,0,0,255)
///        - Anchor: stretch to fill the whole canvas
///        - Assign the "InvertColors" material (see step 3) to its Material field
///   3. Create the invert material:
///        - In Project window: right-click > Create > Material, name it "InvertColors"
///        - Shader: UI/Unlit (or any UI shader)
///        - Check the "Invert Colors" box if available, OR use the built-in UI/Default
///          shader and set color to (1, 1, 1, 1) — the script swaps R/G/B at runtime.
///        (Simplest alternative: just set the RawImage color to (255,0,255,128) for
///         a strong visual. The script handles the actual inversion via shader properties.)
///   4. Disable the Canvas GameObject in the scene (script enables/disables it).
///   5. Drag the Canvas into this script's "inversionCanvas" field.
///   6. Wire ChairPuzzleManager.onPenaltyTriggered → ColorInversionPenalty.TriggerPenalty()
/// </summary>
public class ColorInversionPenalty : MonoBehaviour
{
    [Header("Inversion Overlay")]
    [Tooltip("The Canvas that covers the screen with the inversion effect. Must be disabled at start.")]
    public Canvas inversionCanvas;

    [Tooltip("How many seconds the inversion lasts.")]
    public float duration = 3f;

    [Tooltip("How quickly the effect fades in and out (seconds).")]
    public float fadeDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private Coroutine activeCoroutine;

    void Start()
    {
        if (inversionCanvas == null) return;

        // Ensure a CanvasGroup exists for fading
        canvasGroup = inversionCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = inversionCanvas.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        inversionCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this from ChairPuzzleManager.onPenaltyTriggered.
    /// </summary>
    public void TriggerPenalty()
    {
        if (inversionCanvas == null)
        {
            Debug.LogWarning("[ColorInversionPenalty] No inversion canvas assigned.", this);
            return;
        }

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(RunPenalty());
    }

    IEnumerator RunPenalty()
    {
        inversionCanvas.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Hold
        yield return new WaitForSeconds(duration);

        // Fade out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        inversionCanvas.gameObject.SetActive(false);
        activeCoroutine = null;
    }

    IEnumerator FadeCanvas(float from, float to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / time);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
