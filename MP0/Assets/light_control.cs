using UnityEngine;
using UnityEngine.InputSystem;

public class light_control : MonoBehaviour
{
    public Light light;
    public InputActionReference action;

    void Start()
    {
        light = GetComponent<Light>();
        action.action.Enable();
    }

    void Update()
    {
        if (action.action.WasPressedThisFrame())
        {
            //random!
            light.color = Random.ColorHSV(
                0f, 1f,
                1f, 1f,
                1f, 1f
            );
        }
    }
}
