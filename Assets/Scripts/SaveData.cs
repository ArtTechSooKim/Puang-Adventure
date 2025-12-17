using UnityEngine;

/// <summary>
/// Complete save data structure for the game
/// Includes player state, inventory, scene, and metadata
/// </summary>
[System.Serializable]
public class SaveData
{
    // Metadata
    public string saveTime;
    public string sceneName;

    // Player state
    public Vector3 playerPosition;
    public int playerHealth;
    public float playerStamina;

    // Player abilities
    public bool isDashEnabled;
    public bool isUltEnabled;

    // Inventory
    public InventorySaveData inventoryData;

    // Quest progress
    public int questStage; // QuestStage enum을 int로 저장

    /// <summary>
    /// Create default empty save data
    /// </summary>
    public SaveData()
    {
        saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        sceneName = "Empty";
        playerPosition = Vector3.zero;
        playerHealth = 100;
        playerStamina = 100f;
        isDashEnabled = false;
        isUltEnabled = false;
        inventoryData = new InventorySaveData();
        questStage = 0; // QuestStage.Stage0_VillageTutorial
    }

    /// <summary>
    /// Check if this save data is empty (no actual save)
    /// </summary>
    public bool IsEmpty()
    {
        return sceneName == "Empty" || string.IsNullOrEmpty(sceneName);
    }
}
