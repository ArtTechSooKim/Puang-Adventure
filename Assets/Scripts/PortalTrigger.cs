using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal trigger for scene transitions.
/// When player enters the trigger, loads the target scene and optionally spawns at a specific location.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PortalTrigger : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Name of the scene to load (must be added in Build Settings)")]
    [SerializeField] private string targetSceneName = "TutorialScene";

    [Tooltip("Name of the GameObject in target scene to spawn at (e.g., 'Portal_ToVillage'). Leave empty to use PlayerSpawn tag.")]
    [SerializeField] private string targetSpawnPointName = "";

    [Header("Legacy Spawn Settings (Deprecated)")]
    [Tooltip("Optional: Specific spawn position in target scene. If empty, uses PlayerSpawn tag.")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    [Tooltip("Use specific spawn position instead of PlayerSpawn tag")]
    [SerializeField] private bool useCustomSpawnPosition = false;

    [Header("Quest Stage Requirements")]
    [Tooltip("Require a specific quest stage to use this portal?")]
    [SerializeField] private bool requiresQuestStage = false;

    [Tooltip("Minimum quest stage required to use this portal")]
    [SerializeField] private QuestStage requiredStage = QuestStage.Stage0_VillageTutorial;

    [Tooltip("Message to show when player doesn't meet stage requirement")]
    [TextArea(2, 4)]
    [SerializeField] private string blockedMessage = "You cannot enter this area yet. Complete the required quest first.";

    [Header("Portal Visual (Optional)")]
    [Tooltip("Optional visual effect or sprite renderer to highlight the portal")]
    [SerializeField] private SpriteRenderer portalVisual;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private bool isTransitioning = false;

    private void Awake()
    {
        // Ensure the collider is set to trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"⚠ PortalTrigger: Collider on '{gameObject.name}' was not set to IsTrigger. Auto-fixing.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") && !isTransitioning)
        {
            // Check quest stage requirements
            if (requiresQuestStage && !CheckQuestStageRequirement())
            {
                ShowBlockedMessage();
                return;
            }

            if (showDebugMessages)
                Debug.Log($"🌀 PortalTrigger: Player entered portal '{gameObject.name}' → Loading scene '{targetSceneName}'");

            TriggerSceneTransition(other.gameObject);
        }
    }

    /// <summary>
    /// Trigger the scene transition
    /// </summary>
    private void TriggerSceneTransition(GameObject player)
    {
        isTransitioning = true;

        // Set target spawn point for next scene if specified
        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            PlayerPrefs.SetString("TargetSpawnPoint", targetSpawnPointName);

            if (showDebugMessages)
                Debug.Log($"🎯 PortalTrigger: Set target spawn point to '{targetSpawnPointName}'");
        }
        else
        {
            // Clear any previously set spawn point
            PlayerPrefs.DeleteKey("TargetSpawnPoint");

            if (showDebugMessages)
                Debug.Log($"🎯 PortalTrigger: Using default PlayerSpawn tag");
        }

        // Legacy support: Save player's current position if using custom spawn position
        PlayerPersistent playerPersistent = player.GetComponent<PlayerPersistent>();
        if (playerPersistent != null && useCustomSpawnPosition)
        {
            playerPersistent.SaveCurrentPosition();
        }

        // Load the target scene
        LoadTargetScene();
    }

    /// <summary>
    /// Load the target scene
    /// </summary>
    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"❌ PortalTrigger: Target scene name is empty on '{gameObject.name}'");
            isTransitioning = false;
            return;
        }

        // Check if scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"❌ PortalTrigger: Scene '{targetSceneName}' not found in Build Settings! Add it via File → Build Settings.");
            isTransitioning = false;
        }
    }

    /// <summary>
    /// Get the spawn position for this portal
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return useCustomSpawnPosition ? spawnPosition : Vector3.zero;
    }

    /// <summary>
    /// Check if player meets the quest stage requirement
    /// </summary>
    private bool CheckQuestStageRequirement()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("⚠ PortalTrigger: QuestManager not found - allowing portal access");
            return true;
        }

        bool meetsRequirement = QuestManager.Instance.IsStageReached(requiredStage);

        if (showDebugMessages)
        {
            if (meetsRequirement)
                Debug.Log($"✅ Portal access granted - Stage requirement met ({requiredStage})");
            else
                Debug.Log($"❌ Portal access denied - Stage {requiredStage} required");
        }

        return meetsRequirement;
    }

    /// <summary>
    /// Show blocked message when player doesn't meet requirements
    /// </summary>
    private void ShowBlockedMessage()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(new System.Collections.Generic.List<string> { blockedMessage });

            if (showDebugMessages)
                Debug.Log($"🚫 Portal blocked: {blockedMessage}");
        }
        else
        {
            Debug.LogWarning($"⚠ PortalTrigger: DialogueManager not found - cannot show blocked message");
            Debug.Log($"🚫 Portal Blocked: {blockedMessage}");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw portal area in editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);

            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }

        // Draw arrow pointing up to indicate portal
        Gizmos.color = Color.cyan;
        Vector3 arrowStart = transform.position;
        Vector3 arrowEnd = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(arrowStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowEnd + Vector3.left * 0.15f + Vector3.down * 0.15f);
        Gizmos.DrawLine(arrowEnd, arrowEnd + Vector3.right * 0.15f + Vector3.down * 0.15f);
    }

    private void OnDrawGizmosSelected()
    {
        // Show target spawn position when selected
        if (useCustomSpawnPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPosition, 0.3f);
            Gizmos.DrawLine(transform.position, spawnPosition);

            UnityEditor.Handles.Label(spawnPosition + Vector3.up * 0.5f, $"Spawn Position\n{spawnPosition}");
        }

        // Show scene name label
        UnityEditor.Handles.color = Color.cyan;
        string portalLabel = $"Portal → {targetSceneName}";

        // Add spawn point info
        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            portalLabel += $"\n📍 Spawn at: {targetSpawnPointName}";
        }
        else
        {
            portalLabel += $"\n📍 Spawn at: PlayerSpawn (default)";
        }

        // Add quest requirement info if enabled
        if (requiresQuestStage)
        {
            UnityEditor.Handles.color = Color.yellow;
            portalLabel += $"\n🔒 Requires: {requiredStage}";
        }

        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, portalLabel);
    }
#endif
}
