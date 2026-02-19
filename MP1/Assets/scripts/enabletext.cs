using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    [SerializeField] private GameObject target;

    public void Toggle()
    {
        if (!target) return;
        target.SetActive(!target.activeSelf);
    }
}
