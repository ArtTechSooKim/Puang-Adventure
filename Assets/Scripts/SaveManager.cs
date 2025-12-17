using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SaveManager - Singleton + DontDestroyOnLoad
/// Handles 8-slot save/load system for the entire game
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private int maxSlots = 8;
    [SerializeField] private bool showDebugLogs = true;

    [Header("Scene Settings")]
    [Tooltip("InitialScene name - loads DontDestroyOnLoad objects first")]
    [SerializeField] private string initialSceneName = "01_InitialScene";

    [Tooltip("Title scene name - if loading from this scene, go through InitialScene first")]
    [SerializeField] private string titleSceneName = "00_TitleScene";

    private string saveDirectory => Path.Combine(Application.persistentDataPath, "SaveData");
    private bool isLoadingScene = false;
    private SaveData pendingSaveData = null; // Store save data when loading through InitialScene

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSaveDirectoryExists();
            LogDebug("✅ SaveManager: Initialized and persisting across scenes");
        }
        else
        {
            LogDebug("⚠ SaveManager: Duplicate instance detected - destroying");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Ensure save directory exists
    /// </summary>
    private void EnsureSaveDirectoryExists()
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            LogDebug($"📁 SaveManager: Created save directory at {saveDirectory}");
        }
    }

    /// <summary>
    /// Get the file path for a specific save slot
    /// </summary>
    public string GetSlotPath(int slotIndex)
    {
        return Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");
    }

    /// <summary>
    /// Check if a save file exists for the given slot
    /// </summary>
    public bool SaveFileExists(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > maxSlots)
        {
            Debug.LogWarning($"⚠ SaveManager: Invalid slot index {slotIndex}. Must be between 1 and {maxSlots}");
            return false;
        }

        return File.Exists(GetSlotPath(slotIndex));
    }

    /// <summary>
    /// Preview load save data without applying it (for UI display)
    /// </summary>
    public SaveData PreviewLoad(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > maxSlots)
        {
            Debug.LogWarning($"⚠ SaveManager: Invalid slot index {slotIndex}");
            return new SaveData(); // Return empty data
        }

        string path = GetSlotPath(slotIndex);

        if (!File.Exists(path))
        {
            return new SaveData(); // Return empty data
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            LogDebug($"📂 SaveManager: Preview loaded slot {slotIndex} - Scene: {data.sceneName}, Time: {data.saveTime}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ SaveManager: Failed to preview slot {slotIndex}! {e.Message}");
            return new SaveData();
        }
    }

    /// <summary>
    /// Save game to specified slot
    /// </summary>
    public void Save(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > maxSlots)
        {
            Debug.LogWarning($"⚠ SaveManager: Invalid slot index {slotIndex}. Must be between 1 and {maxSlots}");
            return;
        }

        try
        {
            SaveData saveData = CollectSaveData();
            string json = JsonUtility.ToJson(saveData, true);
            string path = GetSlotPath(slotIndex);

            File.WriteAllText(path, json);

            LogDebug($"💾 SaveManager: Game saved to slot {slotIndex}");
            LogDebug($"   Scene: {saveData.sceneName}");
            LogDebug($"   Position: {saveData.playerPosition}");
            LogDebug($"   HP: {saveData.playerHealth}, Stamina: {saveData.playerStamina:F1}");
            LogDebug($"   Time: {saveData.saveTime}");

            // Show success message (optional - can integrate with UI)
            Debug.Log($"✅ Game saved to Slot {slotIndex}!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ SaveManager: Save to slot {slotIndex} failed! {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Load game from specified slot
    /// </summary>
    public void Load(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > maxSlots)
        {
            Debug.LogWarning($"⚠ SaveManager: Invalid slot index {slotIndex}. Must be between 1 and {maxSlots}");
            return;
        }

        string path = GetSlotPath(slotIndex);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"⚠ SaveManager: No save file found for slot {slotIndex}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            LogDebug($"📂 SaveManager: Loading game from slot {slotIndex}");
            LogDebug($"   Scene: {saveData.sceneName}");
            LogDebug($"   Position: {saveData.playerPosition}");
            LogDebug($"   HP: {saveData.playerHealth}, Stamina: {saveData.playerStamina:F1}");
            LogDebug($"   Time: {saveData.saveTime}");

            string currentScene = SceneManager.GetActiveScene().name;

            // IMPORTANT: If loading from TitleScene, go through InitialScene first
            // to ensure all DontDestroyOnLoad objects (Player, AudioManager, etc.) are loaded
            if (currentScene == titleSceneName)
            {
                LogDebug($"🔄 SaveManager: Loading from TitleScene - going through InitialScene first");
                pendingSaveData = saveData; // Store the save data
                SceneManager.sceneLoaded += OnInitialSceneLoaded; // Listen for InitialScene load
                SceneManager.LoadScene(initialSceneName);
                return;
            }

            // Check if scene transition is needed
            if (saveData.sceneName != currentScene)
            {
                LogDebug($"🔄 SaveManager: Scene change needed: {currentScene} → {saveData.sceneName}");
                StartCoroutine(LoadSceneAndApply(saveData));
            }
            else
            {
                // Same scene - apply with slight delay to ensure all systems are ready
                LogDebug($"🔄 SaveManager: Same scene - applying save data with delay");
                StartCoroutine(ApplySaveDataDelayed(saveData));
            }

            Debug.Log($"✅ Game loaded from Slot {slotIndex}!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ SaveManager: Load from slot {slotIndex} failed! {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Called when InitialScene is loaded (when loading from TitleScene)
    /// </summary>
    private void OnInitialSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == initialSceneName && pendingSaveData != null)
        {
            LogDebug($"✅ SaveManager: InitialScene loaded, now loading target scene: {pendingSaveData.sceneName}");

            // Unsubscribe from event
            SceneManager.sceneLoaded -= OnInitialSceneLoaded;

            // Now load the actual target scene
            StartCoroutine(LoadSceneAndApply(pendingSaveData));

            pendingSaveData = null; // Clear pending data
        }
    }

    /// <summary>
    /// Collect all current game data to save
    /// </summary>
    private SaveData CollectSaveData()
    {
        SaveData data = new SaveData();

        // Save current scene
        data.sceneName = SceneManager.GetActiveScene().name;

        // Save timestamp
        data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Save player data
        if (PlayerPersistent.Instance != null)
        {
            data.playerPosition = PlayerPersistent.Instance.transform.position;

            if (PlayerPersistent.Instance.Health != null)
            {
                data.playerHealth = PlayerPersistent.Instance.Health.GetCurrentHealth();
            }

            if (PlayerPersistent.Instance.Stamina != null)
            {
                data.playerStamina = PlayerPersistent.Instance.Stamina.GetCurrentStamina();
            }

            // Save player abilities (Dash, Ultimate)
            PlayerController playerController = PlayerPersistent.Instance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                data.isDashEnabled = playerController.IsDashEnabled();
                LogDebug($"💾 Dash enabled: {data.isDashEnabled}");
            }

            PlayerUlt playerUlt = PlayerPersistent.Instance.GetComponent<PlayerUlt>();
            if (playerUlt != null)
            {
                data.isUltEnabled = playerUlt.IsUltEnabled();
                LogDebug($"💾 Ult enabled: {data.isUltEnabled}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager: PlayerPersistent.Instance is null! Using default values.");
        }

        // Save inventory data
        if (Inventory.instance != null)
        {
            data.inventoryData = InventorySaveData.FromInventory(Inventory.instance);
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager: Inventory.instance is null! Inventory will not be saved.");
            data.inventoryData = new InventorySaveData();
        }

        // Save quest stage
        if (QuestManager.Instance != null)
        {
            data.questStage = (int)QuestManager.Instance.GetCurrentStage();
            LogDebug($"💾 Quest Stage saved: {QuestManager.Instance.GetCurrentStage()}");
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager: QuestManager.Instance is null! Quest progress will not be saved.");
            data.questStage = 0;
        }

        return data;
    }

    /// <summary>
    /// Apply save data with a slight delay (for same-scene loads)
    /// </summary>
    private IEnumerator ApplySaveDataDelayed(SaveData data)
    {
        // Wait one frame to ensure all managers are initialized
        yield return new WaitForEndOfFrame();

        LogDebug("🔄 SaveManager: Applying save data after delay");
        ApplySaveData(data);
    }

    /// <summary>
    /// Apply loaded save data to the game
    /// </summary>
    private void ApplySaveData(SaveData data)
    {
        if (PlayerPersistent.Instance == null)
        {
            Debug.LogError("❌ SaveManager: Cannot apply save data - PlayerPersistent.Instance is null!");
            return;
        }

        // Restore player position
        PlayerPersistent.Instance.transform.position = data.playerPosition;
        LogDebug($"📍 Position restored to {data.playerPosition}");

        // Restore player health
        if (PlayerPersistent.Instance.Health != null)
        {
            PlayerPersistent.Instance.Health.SetHealth(data.playerHealth);
            LogDebug($"❤️ Health restored to {data.playerHealth}");
        }

        // Restore player stamina
        if (PlayerPersistent.Instance.Stamina != null)
        {
            PlayerPersistent.Instance.Stamina.SetStamina(data.playerStamina);
            LogDebug($"⚡ Stamina restored to {data.playerStamina:F1}");
        }

        // Restore player abilities (Dash, Ultimate)
        PlayerController playerController = PlayerPersistent.Instance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (data.isDashEnabled)
            {
                playerController.EnableDash();
                LogDebug("🏃 Dash enabled");
            }
            else
            {
                playerController.DisableDash();
                LogDebug("🚫 Dash disabled");
            }
        }

        PlayerUlt playerUlt = PlayerPersistent.Instance.GetComponent<PlayerUlt>();
        if (playerUlt != null)
        {
            if (data.isUltEnabled)
            {
                playerUlt.EnableUlt();
                LogDebug("⚡ Ultimate enabled");
            }
            else
            {
                playerUlt.DisableUlt();
                LogDebug("🚫 Ultimate disabled");
            }
        }

        // Restore inventory data
        if (Inventory.instance != null && data.inventoryData != null)
        {
            data.inventoryData.LoadIntoInventory(Inventory.instance);
            LogDebug("📦 Inventory restored");

            // 강제로 UI 갱신 (Hotbar + InventoryUI)
            Inventory.instance.RefreshUIReferences();
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager: Cannot restore inventory - Inventory.instance or inventoryData is null");
        }

        // Restore quest stage
        LogDebug($"🔍 Attempting to restore quest stage (saved value: {data.questStage})");

        if (QuestManager.Instance != null)
        {
            QuestStage currentStageBeforeLoad = QuestManager.Instance.GetCurrentStage();
            QuestStage targetStage = (QuestStage)data.questStage;

            LogDebug($"📜 Quest Stage before load: {currentStageBeforeLoad}");
            LogDebug($"📜 Quest Stage from save: {targetStage}");

            QuestManager.Instance.SetStage(targetStage);

            QuestStage currentStageAfterLoad = QuestManager.Instance.GetCurrentStage();
            LogDebug($"📜 Quest Stage after SetStage: {currentStageAfterLoad}");

            Debug.Log($"✅ Quest Stage restored: {currentStageBeforeLoad} → {targetStage}");
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager: Cannot restore quest progress - QuestManager.Instance is null");
        }

        LogDebug("✅ SaveManager: All save data applied successfully");
    }

    /// <summary>
    /// Load scene and then apply save data
    /// </summary>
    private IEnumerator LoadSceneAndApply(SaveData data)
    {
        if (isLoadingScene)
        {
            Debug.LogWarning("⚠ SaveManager: Scene load already in progress, ignoring request");
            yield break;
        }

        isLoadingScene = true;

        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.sceneName);

        // Wait until scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        LogDebug($"✅ SaveManager: Scene '{data.sceneName}' loaded");

        // Wait one frame to ensure all Start() methods have run
        yield return new WaitForEndOfFrame();

        // Apply save data
        ApplySaveData(data);

        isLoadingScene = false;
    }

    /// <summary>
    /// Delete a save file (optional feature)
    /// </summary>
    public void DeleteSave(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > maxSlots)
        {
            Debug.LogWarning($"⚠ SaveManager: Invalid slot index {slotIndex}");
            return;
        }

        string path = GetSlotPath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            LogDebug($"🗑 SaveManager: Deleted save file for slot {slotIndex}");
        }
        else
        {
            Debug.LogWarning($"⚠ SaveManager: No save file to delete for slot {slotIndex}");
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
    [ContextMenu("Debug: Show Save Directory")]
    private void DebugShowSaveDirectory()
    {
        Debug.Log($"Save directory: {saveDirectory}");
    }

    [ContextMenu("Debug: List All Saves")]
    private void DebugListAllSaves()
    {
        Debug.Log("=== All Save Files ===");
        for (int i = 1; i <= maxSlots; i++)
        {
            if (SaveFileExists(i))
            {
                SaveData data = PreviewLoad(i);
                Debug.Log($"Slot {i}: {data.sceneName} | {data.saveTime}");
            }
            else
            {
                Debug.Log($"Slot {i}: Empty");
            }
        }
        Debug.Log("=====================");
    }

    [ContextMenu("Debug: Delete All Saves")]
    private void DebugDeleteAllSaves()
    {
        for (int i = 1; i <= maxSlots; i++)
        {
            DeleteSave(i);
        }
        Debug.Log("🗑 All save files deleted");
    }
#endif
}
