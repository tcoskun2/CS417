using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class HandSpecificGrabPoints : MonoBehaviour
{
    [Header("Attach points")]
    public Transform leftHandAttach;
    public Transform rightHandAttach;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnSelectEntered);
        grab.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Identify which hand grabbed it (direct or ray)
        var controller = args.interactorObject.transform.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>();
        if (controller == null)
            return;

        // controller.xrController is deprecated in some versions; use handedness via name/tag/layer if needed
        // Most projects simply name the hands "LeftHand Controller" / "RightHand Controller"
        string n = controller.name.ToLowerInvariant();

        if (n.Contains("left") && leftHandAttach != null)
            grab.attachTransform = leftHandAttach;
        else if (n.Contains("right") && rightHandAttach != null)
            grab.attachTransform = rightHandAttach;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        // Optional: reset to a default attach after letting go
        // grab.attachTransform = null; // or keep last used
    }
}
