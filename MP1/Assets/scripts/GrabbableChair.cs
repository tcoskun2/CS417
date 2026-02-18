using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach to each chair (child of a desk).
/// Slides back/forth along a local axis. Snaps to pushed-in or pulled-out on release.
/// XRI is only used for grab detection — all movement is handled by this script
/// so the chair never warps to the hand.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class GrabbableChair : MonoBehaviour
{
    [Header("Chair Identity")]
    [Tooltip("Unique index used by the puzzle manager to identify this chair.")]
    public int chairIndex;

    [Header("Slide Constraint (local space)")]
    [Tooltip("Local-space axis the chair slides along (e.g. 0,0,1 for local Z).")]
    public Vector3 slideAxis = Vector3.forward;

    [Tooltip("Local offset from the starting position when the chair is fully pushed in.")]
    public float pushedInOffset = 0f;

    [Tooltip("Local offset from the starting position when the chair is fully pulled out.")]
    public float pulledOutOffset = 0.5f;

    [Header("State")]
    [Tooltip("Does this chair start pushed in?")]
    public bool startPushedIn = true;

    [Header("References")]
    [Tooltip("Drag the ChairPuzzleManager here so the chair can notify it on release.")]
    public ChairPuzzleManager puzzleManager;

    [HideInInspector]
    public bool isPushedIn;

    private Vector3 localStartPos;
    private Quaternion localStartRot;
    private Vector3 slideDir;
    private float slideMin;
    private float slideMax;
    private XRGrabInteractable grab;
    private Rigidbody rb;

    // Grab tracking — we move the chair ourselves based on hand delta
    private Vector3 grabStartInteractorWorldPos;
    private Vector3 grabStartChairLocalPos;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        // Prevent XRI from moving the chair — we handle all movement ourselves.
        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.throwOnDetach = false;
    }

    void Start()
    {
        EnsureColliderExists();
        ExcludeParentDesksFromRaycast();

        localStartPos = transform.localPosition;
        localStartRot = transform.localRotation;
        slideDir = slideAxis.normalized;

        slideMin = Mathf.Min(pushedInOffset, pulledOutOffset);
        slideMax = Mathf.Max(pushedInOffset, pulledOutOffset);

        isPushedIn = startPushedIn;
        float offset = isPushedIn ? pushedInOffset : pulledOutOffset;
        transform.localPosition = localStartPos + slideDir * offset;

        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void EnsureColliderExists()
    {
        Collider existing = GetComponentInChildren<Collider>();

        if (existing == null)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[GrabbableChair] '{name}' has no Collider and no Renderer. Adding a default BoxCollider.", this);
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

                Debug.Log($"[GrabbableChair] Auto-added BoxCollider to '{name}' (size: {box.size}).", this);
            }
        }

        List<Collider> freshColliders = new List<Collider>(GetComponentsInChildren<Collider>());
        grab.colliders.Clear();
        grab.colliders.AddRange(freshColliders);

        grab.enabled = false;
        grab.enabled = true;

        Debug.Log($"[GrabbableChair] '{name}' registered {freshColliders.Count} collider(s) with XRGrabInteractable.", this);
    }

    void ExcludeParentDesksFromRaycast()
    {
        Transform current = transform.parent;
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");

        while (current != null)
        {
            Collider[] parentColliders = current.GetComponents<Collider>();
            foreach (Collider col in parentColliders)
            {
                if (current.GetComponent<XRBaseInteractable>() == null)
                {
                    col.gameObject.layer = ignoreLayer;
                    Debug.Log($"[GrabbableChair] Set parent '{current.name}' to Ignore Raycast layer.", this);
                }
            }
            current = current.parent;
        }
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabbed);
            grab.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        grabStartInteractorWorldPos = args.interactorObject.transform.position;

        // Use the known-good snapped position, not whatever XRI may have
        // already moved the chair to during its internal grab processing.
        float currentOffset = isPushedIn ? pushedInOffset : pulledOutOffset;
        grabStartChairLocalPos = localStartPos + slideDir * currentOffset;

        // Force the chair back to its correct position immediately.
        transform.localPosition = grabStartChairLocalPos;
        transform.localRotation = localStartRot;
    }

    void LateUpdate()
    {
        if (!grab.isSelected) return;

        // How far has the hand moved in world space since grab started?
        Vector3 interactorWorldPos = grab.interactorsSelecting[0].transform.position;
        Vector3 worldDelta = interactorWorldPos - grabStartInteractorWorldPos;

        // Convert the slide direction from local to world space
        Vector3 worldSlideDir = transform.parent != null
            ? transform.parent.TransformDirection(slideDir)
            : slideDir;

        // Project the hand movement onto the slide axis
        float slideAmount = Vector3.Dot(worldDelta, worldSlideDir);

        // Apply to the chair's local position (relative to where it was when grabbed)
        Vector3 targetLocal = grabStartChairLocalPos + slideDir * slideAmount;
        float projected = Vector3.Dot(targetLocal - localStartPos, slideDir);
        projected = Mathf.Clamp(projected, slideMin, slideMax);

        transform.localPosition = localStartPos + slideDir * projected;
        transform.localRotation = localStartRot;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        Vector3 localOffset = transform.localPosition - localStartPos;
        float projected = Vector3.Dot(localOffset, slideDir);

        float midpoint = (pushedInOffset + pulledOutOffset) * 0.5f;
        isPushedIn = projected < midpoint;

        float snapOffset = isPushedIn ? pushedInOffset : pulledOutOffset;
        transform.localPosition = localStartPos + slideDir * snapOffset;
        transform.localRotation = localStartRot;

        if (puzzleManager != null)
            puzzleManager.OnChairStateChanged();
    }
}
