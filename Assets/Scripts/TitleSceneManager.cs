using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the Title Scene UI and scene transitions
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the Initial Scene (loads DontDestroyOnLoad objects)")]
    [SerializeField] private string initialSceneName = "01_InitialScene";

    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Bind button events
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ TitleSceneManager: Play Button is not assigned!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        if (showDebugLogs)
            Debug.Log("🎮 TitleScene: Ready");
    }

    /// <summary>
    /// Called when Play button is clicked
    /// Loads the Initial Scene, which will then auto-load the Village Scene
    /// </summary>
    public void OnPlayButtonClicked()
    {
        if (showDebugLogs)
            Debug.Log($"🎬 TitleScene: Loading '{initialSceneName}'...");

        if (Application.CanStreamedLevelBeLoaded(initialSceneName))
        {
            SceneManager.LoadScene(initialSceneName);
        }
        else
        {
            Debug.LogError($"❌ TitleSceneManager: Scene '{initialSceneName}' not found in Build Settings!");
        }
    }

    /// <summary>
    /// Called when Quit button is clicked
    /// </summary>
    public void OnQuitButtonClicked()
    {
        if (showDebugLogs)
            Debug.Log("👋 TitleScene: Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Unbind button events
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }

#if UNITY_EDITOR
    [ContextMenu("Test: Load Initial Scene")]
    private void DebugLoadInitialScene()
    {
        OnPlayButtonClicked();
    }
#endif
}
