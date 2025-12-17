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
    [SerializeField] private Button loadButton; // Load button (handled by TitleSceneLoadButton.cs)
    [SerializeField] private Button quitButton;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // 🔥 TitleScene 로드 시 게임 상태 리셋 (Return to Title 버튼으로 왔을 경우)
        CleanupDontDestroyOnLoadObjects();

        // Bind button events
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ TitleSceneManager: Play Button is not assigned!");
        }

        // Load button is handled by TitleSceneLoadButton.cs component on the button itself
        if (loadButton == null && showDebugLogs)
        {
            Debug.Log("ℹ️ TitleSceneManager: Load Button should have TitleSceneLoadButton.cs component");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        if (showDebugLogs)
            Debug.Log("🎮 TitleScene: Ready");
    }

    /// <summary>
    /// TitleScene 로드 시 DontDestroyOnLoad 오브젝트 정리
    /// (Return to Title 버튼으로 돌아온 경우 기존 게임 오브젝트 삭제)
    /// </summary>
    private void CleanupDontDestroyOnLoadObjects()
    {
        // PlayerPersistent 삭제
        if (PlayerPersistent.Instance != null)
        {
            if (showDebugLogs)
                Debug.Log("🗑 TitleScene: 기존 PlayerPersistent 삭제");

            Destroy(PlayerPersistent.Instance.gameObject);
        }

        // QuestManager 삭제
        if (QuestManager.Instance != null)
        {
            if (showDebugLogs)
                Debug.Log("🗑 TitleScene: 기존 QuestManager 삭제");

            Destroy(QuestManager.Instance.gameObject);
        }

        // Inventory 삭제
        if (Inventory.instance != null)
        {
            if (showDebugLogs)
                Debug.Log("🗑 TitleScene: 기존 Inventory 삭제");

            Destroy(Inventory.instance.gameObject);
        }

        // GameManager 삭제
        if (GameManager.I != null)
        {
            if (showDebugLogs)
                Debug.Log("🗑 TitleScene: 기존 GameManager 삭제");

            Destroy(GameManager.I.gameObject);
        }

        if (showDebugLogs)
            Debug.Log("✅ TitleScene: DontDestroyOnLoad 오브젝트 정리 완료");
    }

    /// <summary>
    /// Called when Play button is clicked
    /// Loads the Initial Scene, which will then auto-load the Village Scene
    /// </summary>
    public void OnPlayButtonClicked()
    {
        if (showDebugLogs)
            Debug.Log($"🎬 TitleScene: Play 버튼 클릭 - 게임 상태 리셋 후 '{initialSceneName}' 로드");

        // 🔥 게임 상태 완전 리셋 (PlayerPersistent, QuestManager, Inventory 등)
        ResetGameState();

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
    /// 게임 상태를 완전히 리셋 (새 게임 시작)
    /// </summary>
    private void ResetGameState()
    {
        // 1. PlayerPersistent 삭제 (DontDestroyOnLoad에 남아있는 Player)
        if (PlayerPersistent.Instance != null)
        {
            if (showDebugLogs)
                Debug.Log("🗑 TitleScene: PlayerPersistent 삭제");

            Destroy(PlayerPersistent.Instance.gameObject);
        }

        // 2. QuestManager 리셋
        if (QuestManager.Instance != null)
        {
            if (showDebugLogs)
                Debug.Log("🔄 TitleScene: QuestManager 리셋");

            // QuestManager는 싱글톤이므로 직접 삭제하지 않고 리셋
            // InitialScene에서 새로 생성될 것임
            Destroy(QuestManager.Instance.gameObject);
        }

        // 3. Inventory 리셋 (존재하는 경우)
        if (Inventory.instance != null)
        {
            if (showDebugLogs)
                Debug.Log("🔄 TitleScene: Inventory 리셋");

            Destroy(Inventory.instance.gameObject);
        }

        // 4. GameManager 리셋 (존재하는 경우)
        if (GameManager.I != null)
        {
            if (showDebugLogs)
                Debug.Log("🔄 TitleScene: GameManager 리셋");

            Destroy(GameManager.I.gameObject);
        }

        // 5. AudioManager는 유지 (배경음악 계속 재생)
        // AudioManager는 TitleScene에서도 사용되므로 삭제하지 않음

        if (showDebugLogs)
            Debug.Log("✅ TitleScene: 게임 상태 리셋 완료");
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
