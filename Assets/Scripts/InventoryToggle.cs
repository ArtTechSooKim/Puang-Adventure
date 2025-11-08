using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    private bool isOpen;
    private UIControls controls;

    private void Awake()
    {
        controls = new UIControls();
        controls.UI.InventoryToggle.performed += _ => ToggleInventory();
    }

    private void OnEnable()
    {
        controls.UI.Enable();
    }

    private void OnDisable()
    {
        controls.UI.Disable();
    }

    private void Start()
    {
        inventoryUI.SetActive(false);
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryUI.SetActive(isOpen);
        Time.timeScale = isOpen ? 0 : 1;
        Debug.Log("Inventory toggled (InputSystem)");
    }
}
