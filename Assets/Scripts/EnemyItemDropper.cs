using UnityEngine;

/// <summary>
/// Handles item dropping when an enemy is destroyed.
/// Attach this component to enemy prefabs and assign the item prefab to drop.
/// Works with EnemyHealth's Die() method.
/// </summary>
public class EnemyItemDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("The world item prefab to spawn when this enemy dies (e.g., Item_BatBone_World)")]
    [SerializeField] private GameObject itemPrefabToDrop;

    [Tooltip("Vertical offset from enemy position where item spawns (e.g., -0.3)")]
    [SerializeField] private float dropOffsetY = -0.3f;

    [Tooltip("Drop chance (0-1). 1 = always drop, 0.5 = 50% chance")]
    [SerializeField] [Range(0f, 1f)] private float dropChance = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private void OnDestroy()
    {
        // Only drop items when destroyed during gameplay (not when scene unloads)
        if (!gameObject.scene.isLoaded)
            return;

        // Check if this enemy should drop an item
        if (!ShouldDropItem())
        {
            if (showDebugMessages)
                Debug.Log($"[EnemyItemDropper] {gameObject.name} did not drop item (chance check failed)");
            return;
        }

        DropItem();
    }

    /// <summary>
    /// Determine if item should drop based on drop chance
    /// </summary>
    private bool ShouldDropItem()
    {
        if (itemPrefabToDrop == null)
        {
            if (showDebugMessages)
                Debug.LogWarning($"[EnemyItemDropper] {gameObject.name} has no itemPrefabToDrop assigned!");
            return false;
        }

        // Roll for drop chance
        float roll = Random.value;
        return roll <= dropChance;
    }

    /// <summary>
    /// Spawn the item at this enemy's position
    /// </summary>
    private void DropItem()
    {
        if (itemPrefabToDrop == null)
        {
            Debug.LogError($"[EnemyItemDropper] Cannot drop item - itemPrefabToDrop is null on {gameObject.name}");
            return;
        }

        // Calculate drop position (enemy position + offset)
        Vector3 dropPosition = transform.position + new Vector3(0f, dropOffsetY, 0f);

        if (showDebugMessages)
            Debug.Log($"[EnemyItemDropper] {gameObject.name} dropping item '{itemPrefabToDrop.name}' at {dropPosition}");

        // Instantiate the item prefab
        GameObject droppedItem = Instantiate(itemPrefabToDrop, dropPosition, Quaternion.identity);

        // Configure the dropped item
        ConfigureDroppedItem(droppedItem);

        if (showDebugMessages)
            Debug.Log($"[EnemyItemDropper] ✅ Successfully dropped '{itemPrefabToDrop.name}' from {gameObject.name}");
    }

    /// <summary>
    /// Configure the dropped item's components
    /// </summary>
    private void ConfigureDroppedItem(GameObject droppedItem)
    {
        if (droppedItem == null)
            return;

        // Configure Rigidbody2D (disable gravity, zero velocity)
        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Configure SpriteRenderer (sorting layer)
        SpriteRenderer sr = droppedItem.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Player";
            sr.sortingOrder = 5;
        }

        // Configure Item component (pickup delay to prevent immediate pickup)
        Item itemComponent = droppedItem.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.SetPickupDelay(0.5f);

            if (showDebugMessages)
                Debug.Log($"[EnemyItemDropper] Configured dropped item with 0.5s pickup delay");
        }
        else
        {
            Debug.LogWarning($"[EnemyItemDropper] Dropped item '{droppedItem.name}' has no Item component!");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Visualize drop position in Scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 dropPos = transform.position + new Vector3(0f, dropOffsetY, 0f);
        Gizmos.DrawWireSphere(dropPos, 0.2f);

        UnityEditor.Handles.Label(dropPos + Vector3.up * 0.3f, "Item Drop Position");
    }
#endif
}
