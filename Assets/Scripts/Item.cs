using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData data;
    // time until this world item can be picked up (Time.time). 0 means immediately pickable.
    private float pickableTime = 0f;

    // Called by spawner to prevent immediate re-pickup after dropping
    public void SetPickupDelay(float delaySeconds)
    {
        if (delaySeconds <= 0f) pickableTime = 0f;
        else pickableTime = Time.time + delaySeconds;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ignore pickups until pickableTime
        if (Time.time < pickableTime)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"{data.itemName} 획득!");
            if (Inventory.instance == null)
            {
                Debug.LogWarning("Item.OnTriggerEnter2D: Inventory.instance is null. Cannot add item.");
                return;
            }

            // Only destroy the world item if it was successfully added to inventory (not full).
            bool added = Inventory.instance.AddItem(data);
            if (added)
                Destroy(gameObject);
        }
    }
}
