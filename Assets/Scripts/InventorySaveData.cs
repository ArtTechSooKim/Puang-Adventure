using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable data structure for saving/loading inventory
/// </summary>
[System.Serializable]
public class InventorySaveData
{
    public List<string> itemIDs = new List<string>();
    public List<int> stackCounts = new List<int>();
    public int selectedHotbarIndex;

    /// <summary>
    /// Create save data from Inventory
    /// </summary>
    public static InventorySaveData FromInventory(Inventory inventory)
    {
        InventorySaveData data = new InventorySaveData();

        if (inventory == null || inventory.items == null)
            return data;

        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null)
            {
                data.itemIDs.Add(inventory.items[i].itemID);
                data.stackCounts.Add(inventory.items[i].stackCount);
            }
            else
            {
                data.itemIDs.Add("");
                data.stackCounts.Add(0);
            }
        }

        // Note: Hotbar selection is not saved in this version
        // Hotbar.cs doesn't have a currentSlot field

        return data;
    }

    /// <summary>
    /// Load inventory data into Inventory
    /// </summary>
    public void LoadIntoInventory(Inventory inventory)
    {
        if (inventory == null || itemIDs == null)
            return;

        // Clear current inventory
        for (int i = 0; i < inventory.items.Length; i++)
        {
            inventory.items[i] = null;
        }

        // Load items
        for (int i = 0; i < itemIDs.Count && i < inventory.capacity; i++)
        {
            if (string.IsNullOrEmpty(itemIDs[i]))
                continue;

            // TODO: Load actual ItemData from Resources
            // For now, this is a placeholder - you need to implement item loading
            // You should create a Resources/Items folder and place ItemData assets there
            // Then uncomment the following code:
            //
            // ItemData loadedItem = Resources.Load<ItemData>("Items/" + itemIDs[i]);
            // if (loadedItem != null)
            // {
            //     ItemData runtimeCopy = loadedItem.CreateRuntimeCopy();
            //     runtimeCopy.stackCount = stackCounts[i];
            //     inventory.items[i] = runtimeCopy;
            // }

            Debug.LogWarning($"⚠ InventorySaveData: Item loading not implemented yet for itemID: {itemIDs[i]}");
        }

        // Refresh UI
        inventory.RefreshUIReferences();
    }
}
