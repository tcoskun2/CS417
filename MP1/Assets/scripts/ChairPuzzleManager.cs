using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for the chair-arrangement puzzle.
/// Each chair is a child of its desk and can be pushed in or pulled out.
/// The player must set every chair to the correct state (pushed-in or pulled-out)
/// to solve the puzzle and unlock the code.
///
/// Setup:
///   1. Drag every GrabbableChair into the "chairs" array.
///   2. Set "correctStates" to the matching pushed-in pattern (true = pushed in, false = pulled out).
///   3. Wire onPuzzleSolved to whatever should happen (reveal code, open door, etc.).
/// </summary>
public class ChairPuzzleManager : MonoBehaviour
{
    [Header("All chairs in the puzzle (order must match correctStates)")]
    public GrabbableChair[] chairs;

    [Header("Correct pushed-in states (true = pushed in, false = pulled out)")]
    [Tooltip("One entry per chair. E.g. [true, false, false, true] means chairs 0 and 3 pushed in, 1 and 2 pulled out.")]
    public bool[] correctStates;

    [Header("Events")]
    [Tooltip("Fired once when the player gets every chair into the correct state.")]
    public UnityEvent onPuzzleSolved;

    [Tooltip("Fired when the puzzle is manually reset.")]
    public UnityEvent onPuzzleReset;

    [Header("Optional — Code Display")]
    [Tooltip("If assigned, this GameObject is activated when the puzzle is solved (e.g. a TextMeshPro showing the unlock code).")]
    public GameObject codeDisplay;

    private bool solved = false;

    void Start()
    {
        if (codeDisplay != null)
            codeDisplay.SetActive(false);
    }

    /// <summary>
    /// Called by any GrabbableChair when it is released and snaps to a new state.
    /// </summary>
    public void OnChairStateChanged()
    {
        if (solved) return;

        if (CheckArrangement())
        {
            solved = true;
            Debug.Log("[ChairPuzzle] Puzzle solved! Correct arrangement detected.");

            if (codeDisplay != null)
                codeDisplay.SetActive(true);

            onPuzzleSolved?.Invoke();
        }
    }

    /// <returns>True when every chair matches its expected pushed-in / pulled-out state.</returns>
    bool CheckArrangement()
    {
        if (chairs == null || correctStates == null) return false;
        if (chairs.Length != correctStates.Length) return false;

        for (int i = 0; i < chairs.Length; i++)
        {
            if (chairs[i] == null) return false;
            if (chairs[i].isPushedIn != correctStates[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Call to allow the puzzle to be solved again after a game reset.
    /// </summary>
    public void ResetPuzzle()
    {
        solved = false;

        if (codeDisplay != null)
            codeDisplay.SetActive(false);

        onPuzzleReset?.Invoke();
    }
}
