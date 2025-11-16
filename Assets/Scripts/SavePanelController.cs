using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Controller for the Save Panel.
/// Handles saving and loading game data to/from JSON files.
/// Saves player state, inventory, and game progress.
/// </summary>
public class SavePanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Save button")]
    [SerializeField] private Button buttonSave;

    [Tooltip("Load button")]
    [SerializeField] private Button buttonLoad;

    [Tooltip("Optional: Text to display save/load status")]
    [SerializeField] private Text statusText;

    [Header("Save Settings")]
    [Tooltip("Save file name (without extension)")]
    [SerializeField] private string saveFileName = "save_slot_1";

    [Tooltip("Show debug logs")]
    [SerializeField] private bool showDebugLogs = true;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName + ".json");

    private void Start()
    {
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

        UpdateStatusText("Ready to save or load game data.");
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
    /// </summary>
    private void OnSaveButtonClicked()
    {
        LogDebug("💾 SavePanelController: Save button clicked");
        SaveGame();
    }

    /// <summary>
    /// Called when Load button is clicked
    /// </summary>
    private void OnLoadButtonClicked()
    {
        LogDebug("📂 SavePanelController: Load button clicked");
        LoadGame();
    }

    /// <summary>
    /// Save game data to JSON file
    /// </summary>
    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = CollectSaveData();
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, json);

            UpdateStatusText($"✅ Game saved to: {saveFileName}.json");
            LogDebug($"✅ SavePanelController: Game saved successfully to {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            UpdateStatusText($"❌ Save failed: {e.Message}");
            Debug.LogError($"❌ SavePanelController: Save failed! {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Load game data from JSON file
    /// </summary>
    public void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            UpdateStatusText($"⚠ No save file found: {saveFileName}.json");
            Debug.LogWarning($"⚠ SavePanelController: Save file not found at {SaveFilePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            ApplySaveData(saveData);

            UpdateStatusText($"✅ Game loaded from: {saveFileName}.json");
            LogDebug($"✅ SavePanelController: Game loaded successfully from {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            UpdateStatusText($"❌ Load failed: {e.Message}");
            Debug.LogError($"❌ SavePanelController: Load failed! {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Collect all game data to save
    /// </summary>
    private GameSaveData CollectSaveData()
    {
        GameSaveData data = new GameSaveData();

        // Save player position
        if (PlayerPersistent.Instance != null)
        {
            data.playerPosition = PlayerPersistent.Instance.transform.position;
        }

        // Save player health
        if (PlayerPersistent.Instance != null && PlayerPersistent.Instance.Health != null)
        {
            data.playerHealth = PlayerPersistent.Instance.Health.GetCurrentHealth();
        }

        // Save player stamina
        if (PlayerPersistent.Instance != null && PlayerPersistent.Instance.Stamina != null)
        {
            data.playerStamina = PlayerPersistent.Instance.Stamina.GetCurrentStamina();
        }

        // Save inventory data
        if (Inventory.instance != null)
        {
            data.inventoryData = InventorySaveData.FromInventory(Inventory.instance);
        }

        // Save current scene name
        data.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Save timestamp
        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        LogDebug($"💾 SavePanelController: Collected save data - Scene: {data.currentSceneName}, HP: {data.playerHealth}, Stamina: {data.playerStamina:F1}");
        return data;
    }

    /// <summary>
    /// Apply loaded save data to game
    /// </summary>
    private void ApplySaveData(GameSaveData data)
    {
        // Restore player position
        if (PlayerPersistent.Instance != null)
        {
            PlayerPersistent.Instance.transform.position = data.playerPosition;
        }

        // Restore player health
        if (PlayerPersistent.Instance != null && PlayerPersistent.Instance.Health != null)
        {
            PlayerPersistent.Instance.Health.ResetHealth();
            // TODO: Set health to saved value
        }

        // Restore player stamina
        if (PlayerPersistent.Instance != null && PlayerPersistent.Instance.Stamina != null)
        {
            PlayerPersistent.Instance.Stamina.ResetStamina();
            // TODO: Set stamina to saved value
        }

        // Restore inventory data
        if (Inventory.instance != null && data.inventoryData != null)
        {
            data.inventoryData.LoadIntoInventory(Inventory.instance);
        }

        // TODO: Load scene if different from current scene
        // if (data.currentSceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        // {
        //     UnityEngine.SceneManagement.SceneManager.LoadScene(data.currentSceneName);
        // }

        LogDebug($"📂 SavePanelController: Applied save data - Scene: {data.currentSceneName}, HP: {data.playerHealth}, Stamina: {data.playerStamina:F1}");
    }

    /// <summary>
    /// Update status text display
    /// </summary>
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
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
    [ContextMenu("Debug: Save Game")]
    private void DebugSaveGame()
    {
        SaveGame();
    }

    [ContextMenu("Debug: Load Game")]
    private void DebugLoadGame()
    {
        LoadGame();
    }

    [ContextMenu("Debug: Show Save File Path")]
    private void DebugShowSaveFilePath()
    {
        Debug.Log($"Save file path: {SaveFilePath}");
    }
#endif
}

/// <summary>
/// Serializable data structure for game save data
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public string saveTimestamp;
    public string currentSceneName;
    public Vector3 playerPosition;
    public int playerHealth;
    public float playerStamina;
    public InventorySaveData inventoryData;
}
