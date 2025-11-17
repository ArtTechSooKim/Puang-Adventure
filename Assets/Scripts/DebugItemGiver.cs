using UnityEngine;

/// <summary>
/// Debug tool to quickly give items to the player for testing
/// </summary>
public class DebugItemGiver : MonoBehaviour
{
    [Header("Test Items")]
    [SerializeField] private ItemData slimeDebris;
    [SerializeField] private ItemData batBone;
    [SerializeField] private ItemData forestSword;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode giveQuestItemsKey = KeyCode.F1;
    [SerializeField] private KeyCode clearInventoryKey = KeyCode.F2;

    private void Update()
    {
        if (Input.GetKeyDown(giveQuestItemsKey))
        {
            GiveQuestItems();
        }

        if (Input.GetKeyDown(clearInventoryKey))
        {
            ClearInventory();
        }
    }

    [ContextMenu("Give Quest Items")]
    private void GiveQuestItems()
    {
        if (Inventory.instance == null)
        {
            Debug.LogError("❌ Inventory not found!");
            return;
        }

        if (slimeDebris != null)
        {
            Inventory.instance.AddItem(slimeDebris);
            Debug.Log("✅ Gave: 슬라임 잔해");
        }

        if (batBone != null)
        {
            Inventory.instance.AddItem(batBone);
            Debug.Log("✅ Gave: 박쥐 뼈");
        }
    }

    [ContextMenu("Clear Inventory")]
    private void ClearInventory()
    {
        if (Inventory.instance == null)
        {
            Debug.LogError("❌ Inventory not found!");
            return;
        }

        for (int i = 0; i < Inventory.instance.items.Length; i++)
        {
            Inventory.instance.RemoveItemAt(i);
        }

        Debug.Log("🗑 Inventory cleared");
    }
}
