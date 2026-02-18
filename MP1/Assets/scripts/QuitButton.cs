using UnityEngine;

/// <summary>
/// Simple quit handler for VR. Can be triggered by:
///   - A world-space UI Button (wire OnClick → QuitGame)
///   - An XR Poke Interactable (wire Poke Entered → QuitGame)
///   - Any UnityEvent
///
/// In the Editor it stops Play mode; in a build it exits the application.
/// </summary>
public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("[QuitButton] Quitting application.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
