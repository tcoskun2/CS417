using UnityEngine;


public class PuzzleStageUnlock : MonoBehaviour
{
    [Header("Sockets to enable (next 8)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] socketsToEnable = new UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[8];

    [Header("Lights to enable")]
    [SerializeField] private Light[] lightsToEnable = new Light[2];

    [Header("Projectors")]
    [SerializeField] private GameObject newProjectorObject;
    [SerializeField] private GameObject oldProjectorObject;

    [Header("Options")]
    [Tooltip("If true, disables this component after activation so it only runs once.")]
    [SerializeField] private bool oneShot = true;

    private bool _activated;

    /// <summary>
    /// Call this from an XR event (e.g., Socket Select Entered) or a button.
    /// </summary>
    public void Activate()
    {
        if (_activated) return;
        _activated = true;

        // Enable sockets
        if (socketsToEnable != null)
        {
            for (int i = 0; i < socketsToEnable.Length; i++)
            {
                var socket = socketsToEnable[i];
                if (socket == null) continue;

                socket.gameObject.SetActive(true);
                socket.enabled = true;
                socket.socketActive = true;


                // If the socket relies on a trigger collider, make sure it's enabled too
                var col = socket.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }
        }

        // Turn on lights
        if (lightsToEnable != null)
        {
            for (int i = 0; i < lightsToEnable.Length; i++)
            {
                var lightComp = lightsToEnable[i];
                if (lightComp == null) continue;

                lightComp.enabled = true;
                // Optional: if the light GameObject is disabled, enable it too
                lightComp.gameObject.SetActive(true);
            }
        }

        // Turn on new projector
        if (newProjectorObject != null)
            newProjectorObject.SetActive(true);

        // Disable old projector
        if (oldProjectorObject != null)
            oldProjectorObject.SetActive(false);

        if (oneShot)
            enabled = false;
    }

    // Optional convenience for testing in Play Mode: right-click component header
    [ContextMenu("Activate (Test)")]
    private void ActivateTest() => Activate();
}
