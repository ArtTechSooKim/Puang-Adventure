using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Basic Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;           // Unique identifier for saving/loading
    public string itemName;
    public Sprite sprite;

    [Header("Stacking")]
    public bool isStackable = true;
    public int maxStackSize = 99;

    [Header("World Object")]
    public GameObject worldPrefab;  // Prefab to spawn when dropped into world

    [Header("Weapon Properties")]
    public bool isWeapon = false;
    public int weaponTier = 0;      // 0 = non-weapon, 1-3 = weapon tiers
    public bool hasUltimate = false; // Ultimate skill available for this weapon

    [Header("Quest Properties")]
    public bool isQuestItem = false; // Important items for quest progression

    [HideInInspector]
    public int stackCount = 1;      // Runtime stack count (not serialized in ScriptableObject)

    /// <summary>
    /// Create a runtime copy of this ItemData
    /// </summary>
    public ItemData CreateRuntimeCopy()
    {
        ItemData copy = CreateInstance<ItemData>();
        copy.itemID = this.itemID;
        copy.itemName = this.itemName;
        copy.sprite = this.sprite;
        copy.isStackable = this.isStackable;
        copy.maxStackSize = this.maxStackSize;
        copy.worldPrefab = this.worldPrefab;
        copy.isWeapon = this.isWeapon;
        copy.weaponTier = this.weaponTier;
        copy.hasUltimate = this.hasUltimate;
        copy.isQuestItem = this.isQuestItem;
        copy.stackCount = 1;
        return copy;
    }

    /// <summary>
    /// Copy properties from another ItemData
    /// </summary>
    public void CopyFrom(ItemData other)
    {
        this.itemID = other.itemID;
        this.itemName = other.itemName;
        this.sprite = other.sprite;
        this.isStackable = other.isStackable;
        this.maxStackSize = other.maxStackSize;
        this.worldPrefab = other.worldPrefab;
        this.isWeapon = other.isWeapon;
        this.weaponTier = other.weaponTier;
        this.hasUltimate = other.hasUltimate;
        this.isQuestItem = other.isQuestItem;
    }
}
