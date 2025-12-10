using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TitleScene의 Load 버튼을 SaveDataPanelController와 연결
/// Load 버튼 클릭 시 SaveDataPanel을 Load 모드로 엽니다
/// </summary>
public class TitleSceneLoadButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Load 버튼 (자동으로 찾습니다)")]
    [SerializeField] private Button loadButton;

    [Header("SaveDataPanel Reference")]
    [Tooltip("SaveDataPanelController (자동으로 찾습니다)")]
    [SerializeField] private SaveDataPanelController saveDataPanelController;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Auto-find SaveDataPanelController if not assigned
        if (saveDataPanelController == null)
        {
            saveDataPanelController = FindAnyObjectByType<SaveDataPanelController>(FindObjectsInactive.Include);
            if (saveDataPanelController != null)
            {
                LogDebug($"✅ TitleSceneLoadButton: Found SaveDataPanelController ('{saveDataPanelController.gameObject.name}')");
            }
            else
            {
                Debug.LogWarning("⚠ TitleSceneLoadButton: SaveDataPanelController not found in scene!");
            }
        }

        // Auto-find Load button if not assigned
        if (loadButton == null)
        {
            loadButton = GetComponent<Button>();
            if (loadButton != null)
            {
                LogDebug("✅ TitleSceneLoadButton: Found Button component");
            }
        }

        // Setup button listener
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            LogDebug("✅ TitleSceneLoadButton: Load button listener setup complete");
        }
        else
        {
            Debug.LogWarning("⚠ TitleSceneLoadButton: Load button is not assigned!");
        }
    }

    /// <summary>
    /// Called when Load button is clicked
    /// </summary>
    private void OnLoadButtonClicked()
    {
        LogDebug("📂 TitleSceneLoadButton: Load button clicked");

        if (saveDataPanelController != null)
        {
            saveDataPanelController.OpenLoadMode();
        }
        else
        {
            Debug.LogError("❌ TitleSceneLoadButton: SaveDataPanelController is null!");
        }
    }

    /// <summary>
    /// Cleanup button listener on destroy
    /// </summary>
    private void OnDestroy()
    {
        if (loadButton != null)
        {
            loadButton.onClick.RemoveListener(OnLoadButtonClicked);
        }
    }

    /// <summary>
    /// Helper method to log debug messages
    /// </summary>
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Test Load Button")]
    private void DebugTestLoadButton()
    {
        OnLoadButtonClicked();
    }
#endif
}
