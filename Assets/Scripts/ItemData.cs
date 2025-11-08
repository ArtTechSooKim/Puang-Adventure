using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Basic Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public bool isStackable;
}
