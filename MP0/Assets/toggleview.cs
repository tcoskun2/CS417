using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleViewpoint : MonoBehaviour
{
    public Transform xrOrigin;

    public Transform roomPoint;
    public Transform externalPoint;

    public InputActionReference toggleAction;

    private bool atExternal = false;

    void OnEnable()
    {
        if (toggleAction != null) toggleAction.action.Enable();
    }

    void OnDisable()
    {
        if (toggleAction != null) toggleAction.action.Disable();
    }

    void Update()
    {
        if (toggleAction.action != null && toggleAction.action.WasPressedThisFrame())
        {
            atExternal = !atExternal;
            TeleportTo(atExternal ? externalPoint : roomPoint);
        }
    }

    void TeleportTo(Transform target)
    {
        if (xrOrigin == null || target == null) return;

        xrOrigin.position = target.position;
    }
}
