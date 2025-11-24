using UnityEngine;

/// <summary>
/// Defines a spawn point with a local offset for portal arrivals.
/// Attach this to portal GameObjects to specify where players should spawn relative to the portal.
/// </summary>
public class PortalSpawnPoint : MonoBehaviour
{
    [Header("Spawn Offset")]
    [Tooltip("Offset from this GameObject's position in LOCAL space (e.g., Vector3.down spawns below, Vector3.forward spawns in front)")]
    [SerializeField] private Vector3 localOffset = Vector3.down * 1f;

    [Header("Spawn Direction")]
    [Tooltip("Should the player face the same direction as this GameObject when spawning?")]
    [SerializeField] private bool matchRotation = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    /// <summary>
    /// Get the world position where the player should spawn
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        // Convert local offset to world space
        return transform.position + transform.TransformDirection(localOffset);
    }

    /// <summary>
    /// Get the rotation the player should have when spawning
    /// </summary>
    public Quaternion GetSpawnRotation()
    {
        return matchRotation ? transform.rotation : Quaternion.identity;
    }

    /// <summary>
    /// Get the local offset value
    /// </summary>
    public Vector3 GetLocalOffset()
    {
        return localOffset;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        // Draw the spawn point position
        Vector3 spawnPos = GetSpawnPosition();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPos, 0.3f);

        // Draw line from portal to spawn position
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, spawnPos);

        // Draw direction arrow if matching rotation
        if (matchRotation)
        {
            Gizmos.color = Color.blue;
            Vector3 forward = transform.forward * 0.5f;
            Gizmos.DrawRay(spawnPos, forward);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detailed info when selected
        Vector3 spawnPos = GetSpawnPosition();

        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(spawnPos + Vector3.up * 0.5f, $"Player Spawn\nOffset: {localOffset}");

        // Draw coordinate axes at spawn point
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawLine(spawnPos, spawnPos + transform.right * 0.3f);
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawLine(spawnPos, spawnPos + transform.up * 0.3f);
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawLine(spawnPos, spawnPos + transform.forward * 0.3f);
    }
#endif
}
