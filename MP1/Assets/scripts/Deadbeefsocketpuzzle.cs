using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DeadBeefSocketPuzzle : MonoBehaviour
{
    [Header("Sockets in order (8): D E A D B E E F")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] sockets = new UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[8];

    [Header("On success")]
    [SerializeField] private UnityEvent onSolved;

    [Tooltip("Optional: enable this when solved")]
    [SerializeField] private Behaviour scriptToEnable;

    [Tooltip("Optional: disable this when solved")]
    [SerializeField] private Behaviour scriptToDisable;

    private readonly PaperLetter.Letter[] target =
    {
        PaperLetter.Letter.D,
        PaperLetter.Letter.E,
        PaperLetter.Letter.A,
        PaperLetter.Letter.D,
        PaperLetter.Letter.B,
        PaperLetter.Letter.E,
        PaperLetter.Letter.E,
        PaperLetter.Letter.F
    };

    private bool solved;

    private void OnEnable()
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            s.selectEntered.AddListener(OnSocketSelectEntered);
            s.selectExited.AddListener(OnSocketSelectExited);
        }
    }

    private void OnDisable()
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            s.selectEntered.RemoveListener(OnSocketSelectEntered);
            s.selectExited.RemoveListener(OnSocketSelectExited);
        }
    }

    private void OnSocketSelectEntered(SelectEnterEventArgs _) => Check();
    private void OnSocketSelectExited(SelectExitEventArgs _) => Check();

    private void Check()
    {
        if (solved) return;

        for (int i = 0; i < sockets.Length; i++)
        {
            var socket = sockets[i];
            if (!socket) return;

            // socket has nothing
            var interactable = socket.firstInteractableSelected;
            if (interactable == null) return;

            // get the paper letter on the selected object (or its parents)
            var go = interactable.transform.gameObject;
            var letterComp = go.GetComponentInParent<PaperLetter>();
            if (letterComp == null) return;

            if (letterComp.Value != target[i])
                return; // wrong letter in this position
        }

        // All 8 matched in order
        solved = true;

        if (scriptToEnable) scriptToEnable.enabled = true;
        if (scriptToDisable) scriptToDisable.enabled = false;

        onSolved?.Invoke();
    }
}
