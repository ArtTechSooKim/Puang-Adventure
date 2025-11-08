using UnityEngine;

public class CloseInventory : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    public void Close()
    {
        inventoryUI.SetActive(false);
        Time.timeScale = 1;
        Debug.Log("Inventory closed via X button");
    }
}
