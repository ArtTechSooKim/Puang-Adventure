using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SaveDataPanelController - Manages the 8-slot save/load UI panel
/// Works in both in-game (SavePanel) and TitleScene (LoadButton)
/// </summary>
public class SaveDataPanelController : MonoBehaviour
{
    public enum SaveMode
    {
        Save,
        Load
    }

    [Header("Panel Settings")]
    [SerializeField] private GameObject saveDataPanel;
    [SerializeField] private SaveMode currentMode = SaveMode.Load;

    [Header("Slot Buttons (Auto-find if not assigned)")]
    [SerializeField] private Button[] slotButtons = new Button[8];

    [Header("UI References")]
    [SerializeField] private Button closeButton;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Auto-find panel if not assigned
        if (saveDataPanel == null)
        {
            // SaveDataPanelController가 SaveDataPanel에 직접 붙어있는 경우
            saveDataPanel = gameObject;
            LogDebug($"✅ SaveDataPanelController: Using self as SaveDataPanel ('{gameObject.name}')");
        }

        // Auto-find slot buttons if not assigned
        AutoFindSlotButtons();

        // Setup button listeners
        SetupSlotButtons();

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        // Start with panel closed
        if (saveDataPanel != null)
        {
            saveDataPanel.SetActive(false);
        }

        // 씬 로드 시 저장 데이터 UI 최신화 (패널이 닫혀있어도 실행)
        RefreshSlotsUIIfReady();

        LogDebug("✅ SaveDataPanelController: Initialization complete");
    }

    private void OnEnable()
    {
        // 씬 전환 이벤트 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 전환 이벤트 구독 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 로드될 때마다 저장 데이터 UI 최신화
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        LogDebug($"🔄 SaveDataPanelController: Scene '{scene.name}' loaded - refreshing slots UI");

        // 약간의 딜레이 후 최신화 (SaveManager가 완전히 초기화될 때까지 대기)
        StartCoroutine(DelayedRefresh());
    }

    /// <summary>
    /// 딜레이 후 슬롯 UI 최신화
    /// </summary>
    private System.Collections.IEnumerator DelayedRefresh()
    {
        yield return new WaitForSeconds(0.2f);
        RefreshSlotsUIIfReady();
    }

    /// <summary>
    /// SaveManager가 준비되었을 때만 슬롯 UI 최신화
    /// </summary>
    private void RefreshSlotsUIIfReady()
    {
        if (SaveManager.Instance != null && slotButtons != null && slotButtons.Length > 0)
        {
            RefreshSlotsUI();
        }
        else
        {
            LogDebug("⚠ SaveDataPanelController: SaveManager or slot buttons not ready yet");
        }
    }

    /// <summary>
    /// Auto-find slot buttons from SaveDataPanel
    /// </summary>
    private void AutoFindSlotButtons()
    {
        if (saveDataPanel == null) return;

        int foundCount = 0;

        // Try to find buttons named SlotButton(1), SlotButton(2), etc.
        // Use GetComponentsInChildren to search recursively through all children
        Transform[] allChildren = saveDataPanel.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < 8; i++)
        {
            if (slotButtons[i] == null)
            {
                string targetName = $"SlotButton({i + 1})";

                // Search through all children
                foreach (Transform child in allChildren)
                {
                    if (child.name == targetName)
                    {
                        slotButtons[i] = child.GetComponent<Button>();
                        if (slotButtons[i] != null)
                        {
                            foundCount++;
                            LogDebug($"✅ Found {targetName}");
                        }
                        break;
                    }
                }

                if (slotButtons[i] == null)
                {
                    Debug.LogWarning($"⚠ SaveDataPanelController: {targetName} not found!");
                }
            }
            else
            {
                foundCount++;
            }
        }

        LogDebug($"✅ SaveDataPanelController: Found {foundCount}/8 slot buttons");
    }

    /// <summary>
    /// Setup click listeners for all slot buttons
    /// </summary>
    private void SetupSlotButtons()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                int slotIndex = i + 1; // Slot index starts from 1
                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(slotIndex));
            }
        }

        LogDebug("✅ SaveDataPanelController: Slot button listeners setup complete");
    }

    /// <summary>
    /// Set mode to Save and open panel
    /// </summary>
    public void OpenSaveMode()
    {
        SetMode(SaveMode.Save);
        OpenPanel();
    }

    /// <summary>
    /// Set mode to Load and open panel
    /// </summary>
    public void OpenLoadMode()
    {
        SetMode(SaveMode.Load);
        OpenPanel();
    }

    /// <summary>
    /// Set the current mode (Save or Load)
    /// </summary>
    public void SetMode(SaveMode mode)
    {
        currentMode = mode;
        string modeText = mode == SaveMode.Save ? "저장" : "불러오기";
        LogDebug($"🔧 SaveDataPanelController: Mode set to {modeText} mode");
    }

    /// <summary>
    /// Open the panel and refresh UI
    /// Can be called directly or via OpenSaveMode/OpenLoadMode
    /// </summary>
    public void OpenPanel()
    {
        if (saveDataPanel != null)
        {
            saveDataPanel.SetActive(true);
            RefreshSlotsUI();
        }
    }

    /// <summary>
    /// Close the panel
    /// </summary>
    public void ClosePanel()
    {
        if (saveDataPanel != null)
        {
            saveDataPanel.SetActive(false);
            LogDebug("❌ SaveDataPanelController: Panel closed");
        }
    }

    /// <summary>
    /// Refresh all slot UIs with current save data
    /// </summary>
    public void RefreshSlotsUI()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("⚠ SaveDataPanelController: SaveManager.Instance is null!");
            return;
        }

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                int slotIndex = i + 1;
                UpdateSlotUI(slotButtons[i], slotIndex);
            }
        }

        LogDebug("🔄 SaveDataPanelController: All slots UI refreshed");
    }

    /// <summary>
    /// Update a single slot button's UI
    /// </summary>
    private void UpdateSlotUI(Button slotButton, int slotIndex)
    {
        // Find child TextMeshPro components
        TextMeshProUGUI currentSceneText = slotButton.transform.Find("CurrentScene")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI saveTimeText = slotButton.transform.Find("SaveTime")?.GetComponent<TextMeshProUGUI>();

        if (currentSceneText == null || saveTimeText == null)
        {
            Debug.LogWarning($"⚠ SaveDataPanelController: Text components not found for SlotButton({slotIndex})");
            return;
        }

        // Check if save file exists
        if (SaveManager.Instance.SaveFileExists(slotIndex))
        {
            SaveData data = SaveManager.Instance.PreviewLoad(slotIndex);
            currentSceneText.text = data.sceneName;
            saveTimeText.text = data.saveTime;
        }
        else
        {
            // Empty slot
            currentSceneText.text = "Empty";
            saveTimeText.text = "";
        }
    }

    /// <summary>
    /// Called when a slot button is clicked
    /// </summary>
    private void OnSlotClicked(int slotIndex)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("❌ SaveDataPanelController: SaveManager.Instance is null!");
            return;
        }

        LogDebug($"🖱 SaveDataPanelController: Slot {slotIndex} clicked (Mode: {currentMode})");

        if (currentMode == SaveMode.Save)
        {
            // Save to this slot
            SaveManager.Instance.Save(slotIndex);
            RefreshSlotsUI(); // Update UI after saving
            LogDebug($"💾 Saved to slot {slotIndex}");
        }
        else if (currentMode == SaveMode.Load)
        {
            // Load from this slot
            if (SaveManager.Instance.SaveFileExists(slotIndex))
            {
                SaveManager.Instance.Load(slotIndex);
                ClosePanel(); // Close panel after loading
                LogDebug($"📂 Loaded from slot {slotIndex}");
            }
            else
            {
                Debug.LogWarning($"⚠ SaveDataPanelController: Cannot load - Slot {slotIndex} is empty!");
            }
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
    [ContextMenu("Debug: Refresh Slots UI")]
    private void DebugRefreshSlotsUI()
    {
        RefreshSlotsUI();
    }

    [ContextMenu("Debug: Open Save Mode")]
    private void DebugOpenSaveMode()
    {
        OpenSaveMode();
    }

    [ContextMenu("Debug: Open Load Mode")]
    private void DebugOpenLoadMode()
    {
        OpenLoadMode();
    }
#endif
}
