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
        {
            Debug.LogWarning("⚠ InventorySaveData: Inventory or items array is null!");
            return data;
        }

        Debug.Log($"💾 InventorySaveData: Saving {inventory.items.Length} slots");

        int savedItemCount = 0;
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null)
            {
                data.itemIDs.Add(inventory.items[i].itemID);
                data.stackCounts.Add(inventory.items[i].stackCount);
                savedItemCount++;
                Debug.Log($"   Slot {i}: {inventory.items[i].itemName} (ID: {inventory.items[i].itemID}, Stack: {inventory.items[i].stackCount})");
            }
            else
            {
                data.itemIDs.Add("");
                data.stackCounts.Add(0);
            }
        }

        Debug.Log($"✅ InventorySaveData: Saved {savedItemCount} items out of {inventory.items.Length} slots");

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
        {
            Debug.LogWarning("⚠ InventorySaveData: Inventory or itemIDs is null!");
            return;
        }

        Debug.Log($"📂 InventorySaveData: Loading {itemIDs.Count} slots into inventory");

        // Clear current inventory
        for (int i = 0; i < inventory.items.Length; i++)
        {
            inventory.items[i] = null;
        }

        int loadedItemCount = 0;

        // Load items
        for (int i = 0; i < itemIDs.Count && i < inventory.capacity; i++)
        {
            if (string.IsNullOrEmpty(itemIDs[i]))
                continue;

            // Convert saved itemID to correct Resources path
            string resourcePath = GetCorrectResourcePath(itemIDs[i]);

            // Load actual ItemData from Resources
            ItemData loadedItem = Resources.Load<ItemData>("Items/" + resourcePath);

            if (loadedItem == null)
            {
                Debug.LogWarning($"⚠ InventorySaveData: Item not found in Resources/Items/{resourcePath} (original ID: {itemIDs[i]})");
            }

            if (loadedItem != null)
            {
                ItemData runtimeCopy = loadedItem.CreateRuntimeCopy();
                runtimeCopy.stackCount = stackCounts[i];
                inventory.items[i] = runtimeCopy;
                loadedItemCount++;
                Debug.Log($"   Slot {i}: {loadedItem.itemName} (ID: {itemIDs[i]}, Stack: {stackCounts[i]})");
            }
            else
            {
                Debug.LogWarning($"⚠ InventorySaveData: Item not found in Resources/Items/{itemIDs[i]}");
            }
        }

        Debug.Log($"✅ InventorySaveData: Loaded {loadedItemCount} items into inventory");

        // Refresh UI
        inventory.RefreshUIReferences();
    }

    /// <summary>
    /// Map saved itemID to correct Resources path
    /// Hardcoded mapping for 7 items
    /// </summary>
    private static string GetCorrectResourcePath(string savedItemID)
    {
        if (string.IsNullOrEmpty(savedItemID))
            return savedItemID;

        // Direct mapping for all 7 items
        switch (savedItemID.ToLower())
        {
            case "batbone":
            case "bat_bone":
                return "BatBone";

            case "bossmeat":
            case "boss_meat":
                return "BossMeat";

            case "item_weapontier0":
            case "weapon_tier0":
                return "Item_WeaponTier0";

            case "item_weapontier1":
            case "weapon_tier1":
                return "Item_WeaponTier1";

            case "item_weapontier2":
            case "weapon_tier2":
                return "Item_WeaponTier2";

            case "skeletonbone":
            case "skeleton_bone":
                return "SkeletonBone";

            case "slimeresidue":
            case "slime_residue":
                return "SlimeResidue";

            default:
                // Return original if no mapping found
                return savedItemID;
        }
    }
}
