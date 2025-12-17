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

            // Load actual ItemData from Resources
            ItemData loadedItem = Resources.Load<ItemData>("Items/" + itemIDs[i]);

            // Fallback: Try alternative naming conventions
            if (loadedItem == null)
            {
                // Try capitalizing first letter: weapon_tier1 → Weapon_tier1
                string altID1 = char.ToUpper(itemIDs[i][0]) + itemIDs[i].Substring(1);
                loadedItem = Resources.Load<ItemData>("Items/" + altID1);

                if (loadedItem == null)
                {
                    // Try converting to PascalCase: weapon_tier1 → Item_WeaponTier1
                    string altID2 = ConvertToPascalCase(itemIDs[i]);
                    loadedItem = Resources.Load<ItemData>("Items/" + altID2);
                }
            }

            if (loadedItem != null)
            {
                ItemData runtimeCopy = loadedItem.CreateRuntimeCopy();
                runtimeCopy.stackCount = stackCounts[i];
                inventory.items[i] = runtimeCopy;
                Debug.Log($"✅ InventorySaveData: Loaded item '{loadedItem.itemName}' (ID: {itemIDs[i]}) at slot {i}");
            }
            else
            {
                Debug.LogWarning($"⚠ InventorySaveData: Item not found in Resources/Items/{itemIDs[i]}");
            }
        }

        // Refresh UI
        inventory.RefreshUIReferences();
    }

    /// <summary>
    /// Convert snake_case or lowercase to PascalCase with "Item_" prefix
    /// Example: weapon_tier1 → Item_WeaponTier1
    /// </summary>
    private static string ConvertToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Split by underscore
        string[] parts = input.Split('_');
        System.Text.StringBuilder result = new System.Text.StringBuilder("Item_");

        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            // Capitalize first letter of each part
            result.Append(char.ToUpper(part[0]));
            if (part.Length > 1)
            {
                result.Append(part.Substring(1));
            }
        }

        return result.ToString();
    }
}
