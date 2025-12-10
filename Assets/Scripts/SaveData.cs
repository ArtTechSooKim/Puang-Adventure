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

    // Inventory
    public InventorySaveData inventoryData;

    // Quest progress (optional - can be added later)
    // public int questStage;

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
        inventoryData = new InventorySaveData();
    }

    /// <summary>
    /// Check if this save data is empty (no actual save)
    /// </summary>
    public bool IsEmpty()
    {
        return sceneName == "Empty" || string.IsNullOrEmpty(sceneName);
    }
}
