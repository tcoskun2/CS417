using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for the chair-arrangement puzzle.
/// Each chair is a child of its desk and can be pushed in or pulled out.
/// The player must set every chair to the correct state to solve the puzzle.
/// A specific wrong configuration triggers a color inversion penalty.
/// </summary>
public class ChairPuzzleManager : MonoBehaviour
{
    [Header("All chairs in the puzzle (order must match the state arrays)")]
    public GrabbableChair[] chairs;

    [Header("Correct pushed-in states (true = pushed in, false = pulled out)")]
    [Tooltip("The winning configuration. Length must match chairs array.")]
    public bool[] correctStates;

    [Header("Penalty Configuration")]
    [Tooltip("A specific wrong configuration that triggers the color inversion penalty. Length must match chairs array.")]
    public bool[] penaltyStates;

    [Tooltip("Seconds the player must wait before the penalty can trigger again.")]
    public float penaltyCooldown = 5f;

    [Header("Events")]
    [Tooltip("Fired once when every chair is in the correct state.")]
    public UnityEvent onPuzzleSolved;

    [Tooltip("Fired when the penalty configuration is matched.")]
    public UnityEvent onPenaltyTriggered;

    [Tooltip("Fired when the puzzle is manually reset.")]
    public UnityEvent onPuzzleReset;

    [Header("Optional — Code Display")]
    [Tooltip("If assigned, activated when the puzzle is solved.")]
    public GameObject codeDisplay;

    private bool solved = false;
    private bool penaltyOnCooldown = false;

    void Start()
    {
        if (codeDisplay != null)
            codeDisplay.SetActive(false);
    }

    /// <summary>
    /// Called by any GrabbableChair when it snaps to a new state.
    /// </summary>
    public void OnChairStateChanged()
    {
        if (solved) return;

        if (CheckArrangement(correctStates))
        {
            solved = true;
            Debug.Log("[ChairPuzzle] Puzzle solved!");

            if (codeDisplay != null)
                codeDisplay.SetActive(true);

            onPuzzleSolved?.Invoke();
        }
        else if (!penaltyOnCooldown && penaltyStates != null && CheckArrangement(penaltyStates))
        {
            Debug.Log("[ChairPuzzle] Penalty configuration detected!");
            penaltyOnCooldown = true;
            onPenaltyTriggered?.Invoke();
            Invoke(nameof(ResetPenaltyCooldown), penaltyCooldown);
        }
    }

    void ResetPenaltyCooldown() => penaltyOnCooldown = false;

    bool CheckArrangement(bool[] states)
    {
        if (chairs == null || states == null) return false;
        if (chairs.Length != states.Length) return false;

        for (int i = 0; i < chairs.Length; i++)
        {
            if (chairs[i] == null) return false;
            if (chairs[i].isPushedIn != states[i]) return false;
        }

        return true;
    }

    public void ResetPuzzle()
    {
        solved = false;

        if (codeDisplay != null)
            codeDisplay.SetActive(false);

        onPuzzleReset?.Invoke();
    }
}
