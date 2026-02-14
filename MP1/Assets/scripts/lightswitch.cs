using UnityEngine;

public class PokeToggle : MonoBehaviour
{
    [Header("Lights (groups)")]
    public GameObject lightsOn;
    public GameObject lightsOff;

    [Header("Switch visuals (positions)")]
    public GameObject switchOn;
    public GameObject switchOff;

    [Header("Settings")]
    public bool startOn = false;
    public float cooldown = 0.4f;

    bool canFire = true;

    void Start()
    {
        SetState(startOn);
    }

    public void OnPoke()
    {
        if (!canFire) return;
        canFire = false;

        bool currentlyOn = lightsOn && lightsOn.activeSelf;
        SetState(!currentlyOn);

        Invoke(nameof(ResetCooldown), cooldown);
    }

    void ResetCooldown() => canFire = true;

    public void SetState(bool on)
    {
        if (lightsOn) lightsOn.SetActive(on);
        if (lightsOff) lightsOff.SetActive(!on);

        if (switchOn) switchOn.SetActive(on);
        if (switchOff) switchOff.SetActive(!on);
    }
}
