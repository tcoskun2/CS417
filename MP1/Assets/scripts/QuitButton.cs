using UnityEngine;
using UnityEngine.InputSystem;

public class QuitButton : MonoBehaviour
{
    [Tooltip("Assign any Input Action (e.g. a controller button or keyboard key) to trigger quit.")]
    public InputActionReference quitAction;

    void OnEnable()
    {
        if (quitAction != null)
        {
            quitAction.action.Enable();
            quitAction.action.performed += OnQuit;
        }
    }

    void OnDisable()
    {
        if (quitAction != null)
            quitAction.action.performed -= OnQuit;
    }

    void OnQuit(InputAction.CallbackContext ctx)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
