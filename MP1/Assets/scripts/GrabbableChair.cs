using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to each chair (child of a desk).
/// Uses XRSimpleInteractable (no built-in movement) so XRI never teleports the chair.
/// While held, the chair slides along the local Z axis based on hand movement.
/// On release it snaps fully to pushed-in or pulled-out.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class GrabbableChair : MonoBehaviour
{
    [Header("Chair Identity")]
    [Tooltip("Unique index used by the puzzle manager to identify this chair.")]
    public int chairIndex;

    [Header("Slide Constraint (local space)")]
    [Tooltip("Local offset along Z when fully pushed in (usually 0).")]
    public float pushedInOffset = 0f;

    [Tooltip("Local offset along Z when fully pulled out (e.g. 0.6).")]
    public float pulledOutOffset = 0.6f;

    [Header("State")]
    [Tooltip("Does this chair start pushed in?")]
    public bool startPushedIn = true;

    [Header("References")]
    public ChairPuzzleManager puzzleManager;

    [HideInInspector] public bool isPushedIn;

    private Vector3 localStartPos;
    private Quaternion localStartRot;
    private float slideMin;
    private float slideMax;

    private XRSimpleInteractable interactable;
    private Rigidbody rb;

    private bool isHeld = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor heldBy;
    private Vector3 handStartWorldPos;
    private float chairOffsetAtGrab;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        EnsureColliderExists();
        ExcludeParentDesksFromRaycast();

        localStartPos = transform.localPosition;
        localStartRot = transform.localRotation;
        slideMin = Mathf.Min(pushedInOffset, pulledOutOffset);
        slideMax = Mathf.Max(pushedInOffset, pulledOutOffset);

        isPushedIn = startPushedIn;
        SnapTo(isPushedIn ? pushedInOffset : pulledOutOffset);

        interactable.selectEntered.AddListener(OnGrabbed);
        interactable.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnGrabbed);
            interactable.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        heldBy = args.interactorObject;

        // Record where the hand is right now and the chair's current offset
        handStartWorldPos = heldBy.transform.position;
        chairOffsetAtGrab = isPushedIn ? pushedInOffset : pulledOutOffset;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
        heldBy = null;

        // Snap to whichever end the chair is closer to
        float currentOffset = Vector3.Dot(transform.localPosition - localStartPos, Vector3.forward);
        float midpoint = (pushedInOffset + pulledOutOffset) * 0.5f;
        isPushedIn = currentOffset < midpoint;

        SnapTo(isPushedIn ? pushedInOffset : pulledOutOffset);

        puzzleManager?.OnChairStateChanged();
    }

    void Update()
    {
        if (!isHeld || heldBy == null) return;

        // Project hand movement onto the world-space Z axis of the chair's parent
        Vector3 worldForward = transform.parent != null
            ? transform.parent.TransformDirection(Vector3.forward)
            : Vector3.forward;

        Vector3 handDelta = heldBy.transform.position - handStartWorldPos;
        float slideAmount = Vector3.Dot(handDelta, worldForward);

        float newOffset = Mathf.Clamp(chairOffsetAtGrab + slideAmount, slideMin, slideMax);
        transform.localPosition = localStartPos + Vector3.forward * newOffset;
        transform.localRotation = localStartRot;
    }

    void SnapTo(float offset)
    {
        transform.localPosition = localStartPos + Vector3.forward * offset;
        transform.localRotation = localStartRot;
    }

    void EnsureColliderExists()
    {
        if (GetComponentInChildren<Collider>() == null)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                gameObject.AddComponent<BoxCollider>();
            }
            else
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.center = transform.InverseTransformPoint(bounds.center);
                box.size = transform.InverseTransformVector(bounds.size);
                box.size = new Vector3(Mathf.Abs(box.size.x), Mathf.Abs(box.size.y), Mathf.Abs(box.size.z));
                Debug.Log($"[GrabbableChair] Auto-added BoxCollider to '{name}'.", this);
            }
        }

        // Register all colliders with the interactable so the ray can detect it
        List<Collider> cols = new List<Collider>(GetComponentsInChildren<Collider>());
        interactable.colliders.Clear();
        interactable.colliders.AddRange(cols);

        interactable.enabled = false;
        interactable.enabled = true;
    }

    void ExcludeParentDesksFromRaycast()
    {
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        Transform current = transform.parent;
        while (current != null)
        {
            foreach (Collider col in current.GetComponents<Collider>())
            {
                if (current.GetComponent<XRBaseInteractable>() == null)
                    col.gameObject.layer = ignoreLayer;
            }
            current = current.parent;
        }
    }
}
