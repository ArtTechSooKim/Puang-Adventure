using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    [HideInInspector]
    public int index;
    private ItemData currentItem;

    // Drag helpers
    private static GameObject dragIcon;
    private static Slot dropTarget;
    private ItemData draggingItem;

    public void AddItem(ItemData newItem)
    {
        if (newItem == null)
        {
            ClearSlot();
            return;
        }

        currentItem = newItem;
        if (icon != null)
        {
            icon.sprite = newItem.sprite;
            icon.enabled = true;
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    public ItemData GetItem() => currentItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        draggingItem = currentItem;

        // create drag icon
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas != null ? canvas.transform : transform.root, false);
        Image img = dragIcon.AddComponent<Image>();
        img.sprite = draggingItem.sprite;
        img.raycastTarget = false;
        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);

        // visually clear this slot while dragging; actual inventory data remains until drop
        ClearSlot();
        dropTarget = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragIcon.transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        dragIcon.transform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }

        if (draggingItem == null) return;

        // If dropped on another slot, swap/move
        if (dropTarget != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.SwapItems(this.index, dropTarget.index);
            }
        }
        else
        {
            // dropped outside any slot -> drop to world at player
            if (Inventory.instance != null)
            {
                Inventory.instance.DropItemToWorld(this.index);
            }
        }

        // refresh UI
        if (Inventory.instance != null && Inventory.instance.inventoryUI != null)
            Inventory.instance.inventoryUI.UpdateUI();

        draggingItem = null;
        dropTarget = null;
    }

    // Called when another object is dropped on this slot
    public void OnDrop(PointerEventData eventData)
    {
        dropTarget = this;
    }
}
