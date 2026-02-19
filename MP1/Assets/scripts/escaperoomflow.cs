using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneFlowMenuManager : MonoBehaviour
{
    [Header("Scene names (must match exactly)")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Menu UI (assign in MenuScene only)")]
    [SerializeField] private GameObject menuRoot;      // optional: whole menu panel
    [SerializeField] private TextMeshProUGUI announcementText;    // optional: UI Text to show result
    // If you use TextMeshPro, replace Text with TMPro.TextMeshProUGUI

    private void Start()
    {
        // Only the MenuScene should show announcements
        if (SceneManager.GetActiveScene().name == menuSceneName)
        {
            ApplyAnnouncement();
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        ResultStore.Clear();
        SceneManager.LoadScene(gameSceneName);
    }

    // Call these from the GameScene when the player wins/loses
    public void Win()
    {
        Time.timeScale = 1f;
        ResultStore.Set(ResultStore.Result.Win);
        SceneManager.LoadScene(menuSceneName);
    }

    public void Lose()
    {
        Time.timeScale = 1f;
        ResultStore.Set(ResultStore.Result.Lose);
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ApplyAnnouncement()
    {
        if (menuRoot != null) menuRoot.SetActive(true);

        if (announcementText == null) return;

        var r = ResultStore.Get();
        if (r == ResultStore.Result.None)
        {
            announcementText.text = "";
            return;
        }

        if (r == ResultStore.Result.Win) announcementText.text = "You escaped. You win.";
        if (r == ResultStore.Result.Lose) announcementText.text = "You failed to escape. You lose.";

        // Clear after showing once
        ResultStore.Clear();
    }

    // Small static store that survives scene changes (no DontDestroyOnLoad needed)
    private static class ResultStore
    {
        public enum Result { None, Win, Lose }
        private static Result _result = Result.None;

        public static void Set(Result r) { _result = r; }
        public static Result Get() { return _result; }
        public static void Clear() { _result = Result.None; }
    }
}
