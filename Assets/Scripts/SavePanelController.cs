using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the Save Panel in UI_MasterPanel
/// Integrates with SaveDataPanelController to open Save/Load UI
/// </summary>
public class SavePanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Save button - opens SaveDataPanel in Save mode")]
    [SerializeField] private Button buttonSave;

    [Tooltip("Load button - opens SaveDataPanel in Load mode")]
    [SerializeField] private Button buttonLoad;

    [Header("SaveDataPanel Reference")]
    [Tooltip("Reference to SaveDataPanelController")]
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
                LogDebug($"✅ SavePanelController: Found SaveDataPanelController ('{saveDataPanelController.gameObject.name}')");
            }
            else
            {
                Debug.LogWarning("⚠ SavePanelController: SaveDataPanelController not found in scene!");
            }
        }

        // Setup button listeners
        if (buttonSave != null)
        {
            buttonSave.onClick.AddListener(OnSaveButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠ SavePanelController: buttonSave is not assigned!");
        }

        if (buttonLoad != null)
        {
            buttonLoad.onClick.AddListener(OnLoadButtonClicked);
        }
        else
        {
            Debug.LogWarning("⚠ SavePanelController: buttonLoad is not assigned!");
        }

        // Auto-find buttons from UIReferenceManager if not assigned
        if (buttonSave == null || buttonLoad == null)
        {
            AutoFindButtons();
        }

        LogDebug("✅ SavePanelController: Initialized");
    }

    /// <summary>
    /// Auto-find buttons from UIReferenceManager
    /// </summary>
    private void AutoFindButtons()
    {
        if (UIReferenceManager.Instance == null) return;

        if (buttonSave == null)
        {
            buttonSave = UIReferenceManager.Instance.buttonSaveGame;
        }

        if (buttonLoad == null)
        {
            buttonLoad = UIReferenceManager.Instance.buttonLoadGame;
        }
    }

    /// <summary>
    /// Called when Save button is clicked
    /// Sets mode to Save and opens the panel
    /// </summary>
    private void OnSaveButtonClicked()
    {
        LogDebug("💾 SavePanelController: Save button clicked");

        if (saveDataPanelController != null)
        {
            // Option 1: 기존 방식 유지 (OpenSaveMode가 모드 설정 + 패널 열기)
            saveDataPanelController.OpenSaveMode();

            // Option 2: 버튼에서 직접 모드 설정
            // saveDataPanelController.SetMode(SaveDataPanelController.SaveMode.Save);
        }
        else
        {
            Debug.LogError("❌ SavePanelController: SaveDataPanelController is null!");
        }
    }

    /// <summary>
    /// Called when Load button is clicked
    /// Sets mode to Load and opens the panel
    /// </summary>
    private void OnLoadButtonClicked()
    {
        LogDebug("📂 SavePanelController: Load button clicked");

        if (saveDataPanelController != null)
        {
            // Option 1: 기존 방식 유지 (OpenLoadMode가 모드 설정 + 패널 열기)
            saveDataPanelController.OpenLoadMode();

            // Option 2: 버튼에서 직접 모드 설정
            // saveDataPanelController.SetMode(SaveDataPanelController.SaveMode.Load);
        }
        else
        {
            Debug.LogError("❌ SavePanelController: SaveDataPanelController is null!");
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
}
