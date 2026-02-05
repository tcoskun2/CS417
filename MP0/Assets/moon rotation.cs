using UnityEngine;

public class moonrotation : MonoBehaviour
{
    // Degrees per second
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
