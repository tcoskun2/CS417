using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to the water bottle in the scene.
/// The bottle is locked (not grabbable) until the chair puzzle is solved.
/// When Unlock() is called, the bottle glows, shows floating text, and becomes freely grabbable.
/// 
/// Setup:
///   1. Add a Point Light as a child of the water bottle — disable it in the scene.
///   2. Add a 3D TextMeshPro (TextMeshPro - Text) as a child — disable it in the scene.
///   3. Drag both into this component's fields.
///   4. In ChairPuzzleManager, wire onPuzzleSolved → WaterBottleUnlock.Unlock().
/// </summary>
public class WaterBottleUnlock : MonoBehaviour
{
    [Header("Glow Light")]
    [Tooltip("A Point Light that is a child of the water bottle. Starts disabled.")]
    public Light glowLight;

    [Tooltip("The colour of the glow.")]
    public Color glowColor = new Color(0.4f, 0.8f, 1f, 1f);

    [Tooltip("How bright the light gets at full intensity.")]
    public float glowIntensity = 3f;

    [Header("Floating Text")]
    [Tooltip("A TextMeshPro (3D) object that is a child of the water bottle. Starts disabled.")]
    public TextMeshPro unlockText;

    [Tooltip("The message to display.")]
    public string message = "the curse is lifted";

    [Header("Animation")]
    [Tooltip("How many seconds the glow and text take to fade in.")]
    public float fadeInDuration = 1.5f;

    [Tooltip("How fast (degrees/sec) the text gently rotates to face different angles. 0 = no spin.")]
    public float textSpinSpeed = 15f;

    [Tooltip("How far above the bottle the text floats (local Y offset).")]
    public float textFloatHeight = 0.3f;

    private bool unlocked = false;
    private float fadeTimer = 0f;
    private Vector3 textStartLocal;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Lock the bottle until the puzzle is solved
        if (grabInteractable != null)
            grabInteractable.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (glowLight != null)
        {
            glowLight.color = glowColor;
            glowLight.intensity = 0f;
            glowLight.gameObject.SetActive(false);
        }

        if (unlockText != null)
        {
            unlockText.text = message;
            unlockText.alpha = 0f;
            textStartLocal = unlockText.transform.localPosition;
            unlockText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by ChairPuzzleManager.onPuzzleSolved.
    /// Enables grabbing and triggers the visual effect.
    /// </summary>
    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        fadeTimer = 0f;

        // Make the bottle grabbable and subject to physics
        if (grabInteractable != null)
            grabInteractable.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (glowLight != null)
            glowLight.gameObject.SetActive(true);

        if (unlockText != null)
            unlockText.gameObject.SetActive(true);

        Debug.Log("[WaterBottleUnlock] The curse is lifted! Bottle is now grabbable.");
    }

    void Update()
    {
        if (!unlocked) return;

        // Fade in over time
        fadeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fadeTimer / fadeInDuration);

        if (glowLight != null)
            glowLight.intensity = Mathf.Lerp(0f, glowIntensity, t);

        if (unlockText != null)
        {
            unlockText.alpha = t;

            // Gently float upward during fade-in
            Vector3 pos = textStartLocal;
            pos.y += textFloatHeight * t;
            unlockText.transform.localPosition = pos;

            // Slow spin so the text is visible from different angles
            if (textSpinSpeed > 0f)
                unlockText.transform.Rotate(Vector3.up, textSpinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
