using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Master UI Controller for the new tab-based UI system.
/// Handles:
/// - Tab key: Toggle entire Master UI (open/close)
/// - ESC key: Close Master UI if open
/// - I key: Open Master UI with Inventory tab active
/// - TimeScale control (0 when UI open, 1 when closed)
/// - Panel switching (Map, Inventory, Settings, Save)
///
/// IMPORTANT: CloseButton (X) is REMOVED. Use Tab/ESC keys only.
/// </summary>
public class UI_MasterController : MonoBehaviour
{
    [Header("=== Master Panel Reference ===")]
    [Tooltip("Main Master Panel GameObject - will be toggled on/off")]
    [SerializeField] private GameObject masterPanel;

    [Header("=== Panel References ===")]
    [Tooltip("Map Panel")]
    [SerializeField] private GameObject mapPanel;

    [Tooltip("Inventory Panel")]
    [SerializeField] private GameObject inventoryPanel;

    [Tooltip("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Tooltip("Save Panel")]
    [SerializeField] private GameObject savePanel;

    [Header("=== Top Buttons ===")]
    [Tooltip("Button to switch to Map Panel")]
    [SerializeField] private Button buttonMap;

    [Tooltip("Button to switch to Inventory Panel")]
    [SerializeField] private Button buttonInventory;

    [Tooltip("Button to switch to Settings Panel")]
    [SerializeField] private Button buttonSettings;

    [Tooltip("Button to switch to Save Panel")]
    [SerializeField] private Button buttonSave;

    [Header("=== SavePanel Buttons ===")]
    [Tooltip("Button to return to title scene")]
    [SerializeField] private Button buttonReturnToTitle;

    [Tooltip("Button to quit game")]
    [SerializeField] private Button buttonQuitGame;

    [Header("=== SaveDataPanel Reference ===")]
    [Tooltip("Reference to SaveDataPanelController to close it when Master UI closes")]
    [SerializeField] private SaveDataPanelController saveDataPanelController;

    [Header("=== HUD Reference ===")]
    [Tooltip("HUD Canvas - will disable interaction when Master UI is open")]
    [SerializeField] private Canvas hudCanvas;

    [Header("=== Settings ===")]
    [Tooltip("Default panel to show when opening Master UI with Tab key")]
    [SerializeField] private PanelType defaultPanel = PanelType.Inventory;

    [Tooltip("Show debug logs")]
    [SerializeField] private bool showDebugLogs = true;

    [Header("=== Auto-Reference Settings ===")]
    [Tooltip("Automatically find UIReferenceManager and use its references")]
    [SerializeField] private bool useUIReferenceManager = true;

    // State
    private bool isMasterUIOpen = false;
    private PanelType currentActivePanel = PanelType.Inventory;

    // Input
    private UIControls controls;

    // Panel enum
    public enum PanelType
    {
        Map,
        Inventory,
        Settings,
        Save
    }

    private void Awake()
    {
        // Setup input controls
        controls = new UIControls();

        // Bind Tab key (toggle Master UI)
        controls.UI.ToggleMasterUI.performed += _ => ToggleMasterUI();

        // Bind ESC key (close Master UI)
        controls.UI.CloseMasterUI.performed += _ => CloseMasterUI();

        // Bind I key (open Inventory directly)
        controls.UI.InventoryToggle.performed += _ => OpenInventoryDirect();

        // Auto-find references from UIReferenceManager
        if (useUIReferenceManager && UIReferenceManager.Instance != null)
        {
            AutoFindReferencesFromManager();
        }
    }

    private void Start()
    {
        // Setup button listeners
        SetupButtonListeners();

        // Ensure Master UI starts closed
        CloseMasterUI();
    }

    private void OnEnable()
    {
        controls?.UI.Enable();
    }

    private void OnDisable()
    {
        controls?.UI.Disable();
    }

    /// <summary>
    /// Auto-find references from UIReferenceManager (if available)
    /// </summary>
    private void AutoFindReferencesFromManager()
    {
        var manager = UIReferenceManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("⚠ UI_MasterController: UIReferenceManager.Instance is null. Cannot auto-find references.");
            return;
        }

        if (masterPanel == null)
            masterPanel = manager.GetMasterPanel();

        if (inventoryPanel == null)
            inventoryPanel = manager.inventoryPanel;

        if (mapPanel == null)
            mapPanel = manager.mapPanel;

        if (settingsPanel == null)
            settingsPanel = manager.settingsPanel;

        if (savePanel == null)
            savePanel = manager.savePanel;

        if (hudCanvas == null)
            hudCanvas = manager.hudCanvas;

        if (buttonMap == null)
            buttonMap = manager.buttonMap;

        if (buttonInventory == null)
            buttonInventory = manager.buttonInventory;

        if (buttonSettings == null)
            buttonSettings = manager.buttonSettings;

        if (buttonSave == null)
            buttonSave = manager.buttonSave;

        // Note: buttonReturnToTitle and buttonQuitGame are in SavePanel and won't be in UIReferenceManager

        // Auto-find SaveDataPanelController
        if (saveDataPanelController == null)
        {
            saveDataPanelController = FindAnyObjectByType<SaveDataPanelController>(FindObjectsInactive.Include);
            if (saveDataPanelController != null)
            {
                LogDebug("✅ UI_MasterController: Found SaveDataPanelController");
            }
        }

        LogDebug("✅ UI_MasterController: Auto-found references from UIReferenceManager");
    }

    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (buttonMap != null)
            buttonMap.onClick.AddListener(() => { AudioManager.I?.PlayUIClickSound(); SwitchPanel(PanelType.Map); });
        else
            Debug.LogWarning("⚠ UI_MasterController: buttonMap is not assigned!");

        if (buttonInventory != null)
            buttonInventory.onClick.AddListener(() => { AudioManager.I?.PlayUIClickSound(); SwitchPanel(PanelType.Inventory); });
        else
            Debug.LogWarning("⚠ UI_MasterController: buttonInventory is not assigned!");

        if (buttonSettings != null)
            buttonSettings.onClick.AddListener(() => { AudioManager.I?.PlayUIClickSound(); SwitchPanel(PanelType.Settings); });
        else
            Debug.LogWarning("⚠ UI_MasterController: buttonSettings is not assigned!");

        if (buttonSave != null)
            buttonSave.onClick.AddListener(() => { AudioManager.I?.PlayUIClickSound(); SwitchPanel(PanelType.Save); });
        else
            Debug.LogWarning("⚠ UI_MasterController: buttonSave is not assigned!");

        // Setup SavePanel buttons
        if (buttonReturnToTitle != null)
            buttonReturnToTitle.onClick.AddListener(() => { AudioManager.I?.PlayUIClickSound(); OnReturnToTitleClicked(); });
        else
            Debug.LogWarning("⚠ UI_MasterController: buttonReturnToTitle is not assigned!");

        if (buttonQuitGame != null)
            buttonQuitGame.onClick.AddListener(() => { AudioManager.I?.PlayUIClickSound(); OnQuitGameClicked(); });
        else
            Debug.LogWarning("⚠ UI_MasterController: buttonQuitGame is not assigned!");

        LogDebug("✅ UI_MasterController: Button listeners setup complete");
    }

    /// <summary>
    /// Toggle Master UI on/off (Tab key)
    /// </summary>
    private void ToggleMasterUI()
    {
        // Block UI opening if in GameOver state
        if (GameManager.I != null && GameManager.I.IsGameOver())
        {
            LogDebug("⏸ UI_MasterController: Cannot open UI during GameOver - user must return to TitleScene");
            return;
        }

        if (isMasterUIOpen)
        {
            CloseMasterUI();
        }
        else
        {
            OpenMasterUI(defaultPanel);
        }
    }

    /// <summary>
    /// Open Master UI with a specific panel active
    /// </summary>
    public void OpenMasterUI(PanelType panelType)
    {
        if (masterPanel == null)
        {
            Debug.LogError("❌ UI_MasterController.OpenMasterUI: masterPanel is not assigned!");
            return;
        }

        // Block UI opening if dialogue is active
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            LogDebug("⏸ UI_MasterController: Cannot open UI while dialogue is active");
            return;
        }

        isMasterUIOpen = true;
        masterPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game

        // 🔊 패널 열림 사운드 재생
        AudioManager.I?.PlayUIPanelOpenSound();

        // Disable HUD interaction (but keep visible)
        SetHUDInteractable(false);

        // Switch to requested panel
        SwitchPanel(panelType);

        LogDebug($"📂 UI_MasterController: Master UI opened with {panelType} panel");
    }

    /// <summary>
    /// Close Master UI (Tab/ESC key)
    /// </summary>
    public void CloseMasterUI()
    {
        if (masterPanel == null)
        {
            Debug.LogError("❌ UI_MasterController.CloseMasterUI: masterPanel is not assigned!");
            return;
        }

        // SaveDataPanel도 함께 닫기
        if (saveDataPanelController != null)
        {
            saveDataPanelController.ClosePanel();
            LogDebug("📂 UI_MasterController: SaveDataPanel 닫음");
        }

        isMasterUIOpen = false;
        masterPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game

        // 🔊 패널 닫힘 사운드 재생
        AudioManager.I?.PlayUIPanelCloseSound();

        // Re-enable HUD interaction
        SetHUDInteractable(true);

        LogDebug("📂 UI_MasterController: Master UI closed");
    }

    /// <summary>
    /// Open Inventory directly (I key shortcut)
    /// </summary>
    private void OpenInventoryDirect()
    {
        // Block UI opening if in GameOver state
        if (GameManager.I != null && GameManager.I.IsGameOver())
        {
            LogDebug("⏸ UI_MasterController: Cannot open Inventory during GameOver - user must return to TitleScene");
            return;
        }

        if (isMasterUIOpen)
        {
            // If already open, switch to Inventory panel
            SwitchPanel(PanelType.Inventory);
        }
        else
        {
            // Open Master UI with Inventory active
            OpenMasterUI(PanelType.Inventory);
        }
    }

    /// <summary>
    /// Switch active panel (called by top buttons)
    /// </summary>
    public void SwitchPanel(PanelType panelType)
    {
        currentActivePanel = panelType;

        // Disable all panels
        if (mapPanel != null) mapPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (savePanel != null) savePanel.SetActive(false);

        // Enable requested panel
        switch (panelType)
        {
            case PanelType.Map:
                if (mapPanel != null)
                {
                    mapPanel.SetActive(true);
                    LogDebug("🗺 UI_MasterController: Switched to Map panel");
                }
                else
                {
                    Debug.LogWarning("⚠ UI_MasterController: mapPanel is null!");
                }
                break;

            case PanelType.Inventory:
                if (inventoryPanel != null)
                {
                    inventoryPanel.SetActive(true);
                    LogDebug("📦 UI_MasterController: Switched to Inventory panel");

                    // Refresh Inventory UI
                    if (Inventory.instance != null && Inventory.instance.inventoryUI != null)
                    {
                        Inventory.instance.inventoryUI.UpdateUI();
                    }
                }
                else
                {
                    Debug.LogWarning("⚠ UI_MasterController: inventoryPanel is null!");
                }
                break;

            case PanelType.Settings:
                if (settingsPanel != null)
                {
                    settingsPanel.SetActive(true);
                    LogDebug("⚙ UI_MasterController: Switched to Settings panel");
                }
                else
                {
                    Debug.LogWarning("⚠ UI_MasterController: settingsPanel is null!");
                }
                break;

            case PanelType.Save:
                if (savePanel != null)
                {
                    savePanel.SetActive(true);
                    LogDebug("💾 UI_MasterController: Switched to Save panel");
                }
                else
                {
                    Debug.LogWarning("⚠ UI_MasterController: savePanel is null!");
                }
                break;
        }
    }

    /// <summary>
    /// Enable/disable HUD interaction (but keep visible)
    /// IMPORTANT: HUD (especially Hotbar) should ALWAYS be interactable even when Master UI is open
    /// This allows drag & drop between Hotbar and Inventory
    /// </summary>
    private void SetHUDInteractable(bool interactable)
    {
        // ✅ HUD는 항상 상호작용 가능해야 함!
        // 인벤토리가 열려있을 때도 핫바와 드래그 앤 드롭이 작동해야 함
        // 따라서 이 메서드는 아무 것도 하지 않음

        // REMOVED: GraphicRaycaster disable/enable
        // REMOVED: CanvasGroup interactable/blocksRaycasts control

        // HUD는 항상 활성화 상태 유지
        return;
    }

    /// <summary>
    /// Public getter for Master UI state
    /// </summary>
    public bool IsMasterUIOpen()
    {
        return isMasterUIOpen;
    }

    /// <summary>
    /// Public getter for current active panel
    /// </summary>
    public PanelType GetCurrentActivePanel()
    {
        return currentActivePanel;
    }

    /// <summary>
    /// Called when Return to Title button is clicked
    /// </summary>
    private void OnReturnToTitleClicked()
    {
        LogDebug("🏠 UI_MasterController: Return to Title button clicked");

        // Resume time scale before scene transition
        Time.timeScale = 1f;

        // 🔥 게임 상태 리셋 (PlayerPersistent는 TitleScene에서 자동 삭제되므로 여기서는 하지 않음)
        // TitleScene의 Awake에서 PlayerPersistent.Instance를 삭제함
        // 다른 매니저들도 TitleScene에서 리셋될 것임

        // Load title scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("00_TitleScene");
    }

    /// <summary>
    /// Called when Quit Game button is clicked
    /// </summary>
    private void OnQuitGameClicked()
    {
        LogDebug("👋 UI_MasterController: Quit Game button clicked");

        // Resume time scale before quitting
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
    [ContextMenu("Debug: Open Master UI (Inventory)")]
    private void DebugOpenInventory()
    {
        OpenMasterUI(PanelType.Inventory);
    }

    [ContextMenu("Debug: Close Master UI")]
    private void DebugCloseMasterUI()
    {
        CloseMasterUI();
    }

    [ContextMenu("Debug: Toggle Master UI")]
    private void DebugToggleMasterUI()
    {
        ToggleMasterUI();
    }
#endif
}
