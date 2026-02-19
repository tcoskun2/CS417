using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject disableWhenToggled;

    public void Toggle()
    {
        if (!target) return;

        // Toggle the target
        bool newState = !target.activeSelf;
        target.SetActive(newState);

        if (disableWhenToggled)
            disableWhenToggled.SetActive(false);
    }
}
