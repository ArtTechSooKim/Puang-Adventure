using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls NPC interactions with stage-based dialogue and quest item exchanges.
/// Replaces the basic NPCDialogue functionality with advanced quest system integration.
/// </summary>
public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCData npcData;

    [Header("Interaction Settings")]
    [SerializeField] private bool autoOpenOnEnter = false;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject interactionPrompt; // UI element showing "Press E"

    [Header("Quest Marker")]
    [Tooltip("emote21 스프라이트를 할당하세요 (느낌표 말풍선)")]
    [SerializeField] private Sprite questMarkerSprite;
    [Tooltip("NPC 머리 위 마커 오프셋 (기본값: Vector3.up * 1.5f)")]
    [SerializeField] private Vector3 markerOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("마커 크기 (기본값: 0.5f)")]
    [SerializeField] private float markerScale = 0.5f;
    [Tooltip("마커 SortingLayer 이름 (기본값: Default)")]
    [SerializeField] private string markerSortingLayer = "Default";
    [Tooltip("마커 Sorting Order (기본값: 100)")]
    [SerializeField] private int markerSortingOrder = 100;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private bool playerInRange = false; // For E key interaction (BoxCollider2D)
    private bool playerInMarkerRange = false; // For quest marker (CircleCollider2D)
    private QuestStage currentStage;
    private GameObject questMarkerObject;
    private SpriteRenderer questMarkerRenderer;
    private float interactionCooldown = 0f; // Cooldown to prevent rapid re-interaction
    private bool isWaitingForStageAdvance = false; // Prevent multiple stage advances

    private void Reset()
    {
        // Auto-add collider trigger on creation
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D newCol = gameObject.AddComponent<BoxCollider2D>();
            newCol.isTrigger = true;
            newCol.size = new Vector2(2f, 2f); // Default interaction area
        }
    }

    private void Start()
    {
        // Hide interaction prompt initially
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Validate NPC data
        if (npcData == null)
        {
            Debug.LogError($"❌ NPCController on '{gameObject.name}': NPCData is not assigned!");
        }

        // Create quest marker object
        CreateQuestMarker();

        // Initial quest marker update
        UpdateQuestMarker();
    }

    private void Update()
    {
        // Update cooldown timer
        if (interactionCooldown > 0f)
        {
            interactionCooldown -= Time.unscaledDeltaTime; // Use unscaledDeltaTime to work even when Time.timeScale = 0
        }

        // Always update quest marker based on current stage
        if (QuestManager.Instance != null)
        {
            QuestStage newStage = QuestManager.Instance.GetCurrentStage();
            if (newStage != currentStage)
            {
                currentStage = newStage;
                UpdateQuestMarker();
            }
        }

        // Update player range status every frame based on actual collider overlap
        UpdatePlayerRangeStatus();

        if (!playerInRange || npcData == null) return;

        // Block all input if dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            return;
        }

        // Block input during cooldown
        if (interactionCooldown > 0f)
        {
            return;
        }

        // Check for interaction input (New Input System)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartInteraction();
        }

        // Legacy Input System fallback
        if (Input.GetKeyDown(interactionKey))
        {
            StartInteraction();
        }
    }

    /// <summary>
    /// Update player range status based on actual position and THIS NPC's colliders
    /// </summary>
    private void UpdatePlayerRangeStatus()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector2 playerPos = player.transform.position;

        BoxCollider2D myBox = GetComponent<BoxCollider2D>();
        CircleCollider2D myCircle = GetComponent<CircleCollider2D>();

        bool wasInRange = playerInRange;
        bool wasInMarkerRange = playerInMarkerRange;

        if (myBox != null && myCircle != null)
        {
            // Check if player is in THIS NPC's box
            playerInRange = myBox.bounds.Contains(playerPos);

            // Check if player is in THIS NPC's circle
            playerInMarkerRange = myCircle.bounds.Contains(playerPos);
        }
        else if (myBox != null)
        {
            // Only box exists
            playerInRange = myBox.bounds.Contains(playerPos);
            playerInMarkerRange = playerInRange;
        }
        else if (myCircle != null)
        {
            // Only circle exists
            playerInMarkerRange = myCircle.bounds.Contains(playerPos);
            playerInRange = playerInMarkerRange;
        }

        // Update marker if status changed
        if (wasInMarkerRange != playerInMarkerRange)
        {
            UpdateQuestMarker();
        }

        // Auto-open dialogue if enabled and player just entered range
        if (autoOpenOnEnter && !wasInRange && playerInRange)
        {
            // Check if NPC has dialogue for current stage
            if (QuestManager.Instance != null)
            {
                currentStage = QuestManager.Instance.GetCurrentStage();
                bool hasDialogue = npcData != null && npcData.HasDialogueForStage(currentStage);

                if (hasDialogue && DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen())
                {
                    StartInteraction();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // UpdatePlayerRangeStatus() in Update() handles the actual range checking
        // This is just for debug logging
        if (showDebugMessages)
        {
            Debug.Log($"💬 Trigger event: Player entered {npcData.npcName}'s collider area");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // UpdatePlayerRangeStatus() in Update() handles the actual range checking
        // This is just for debug logging
        if (showDebugMessages)
        {
            Debug.Log($"💬 Trigger event: Player left {npcData.npcName}'s collider area");
        }
    }

    /// <summary>
    /// Start the NPC interaction based on current quest stage
    /// </summary>
    private void StartInteraction()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌ NPCController: QuestManager not found!");
            return;
        }

        // Get current quest stage
        currentStage = QuestManager.Instance.GetCurrentStage();

        // Check if this NPC has dialogue for the current stage
        if (!npcData.HasDialogueForStage(currentStage))
        {
            if (showDebugMessages)
                Debug.Log($"⏸ {npcData.npcName} has no dialogue for current stage {currentStage} - interaction blocked");

            // Show a generic message (optional)
            if (DialogueManager.Instance != null)
            {
                ShowDialogue(new List<string> { $"{npcData.npcName}: 지금은 할 말이 없어요..." });
            }
            return;
        }

        // Get dialogue for this stage
        NPCDialogueSet dialogueSet = npcData.GetDialogueForStage(currentStage);

        // Check if player has required items
        if (dialogueSet.HasRequirements())
        {
            if (!CheckRequiredItems(dialogueSet.requiredItems))
            {
                // Show insufficient items message
                string message = !string.IsNullOrEmpty(dialogueSet.insufficientItemsMessage)
                    ? dialogueSet.insufficientItemsMessage
                    : $"You need: {dialogueSet.GetRequiredItemsText()}";

                ShowDialogue(new List<string> { message });
                return;
            }
        }

        // Consume required items before dialogue
        if (dialogueSet.HasRequirements())
        {
            ConsumeItems(dialogueSet.requiredItems);
        }

        // Show dialogue
        ShowDialogue(dialogueSet.dialogueLines);

        // Give reward items
        if (dialogueSet.HasRewards())
        {
            GiveRewardItems(dialogueSet.rewardItems);
        }

        // Store quest advancement flag for after dialogue ends
        if (dialogueSet.advanceStageOnComplete && !isWaitingForStageAdvance)
        {
            StartCoroutine(AdvanceStageAfterDialogue());
        }
    }

    /// <summary>
    /// Wait for dialogue to end, then advance quest stage
    /// </summary>
    private System.Collections.IEnumerator AdvanceStageAfterDialogue()
    {
        isWaitingForStageAdvance = true;

        // Wait until dialogue is closed
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            yield return null;
        }

        // Dialogue finished, now advance stage
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AdvanceStage();

            if (showDebugMessages)
                Debug.Log($"📈 Quest advanced by {npcData.npcName}");
        }

        isWaitingForStageAdvance = false;
    }

    /// <summary>
    /// Check if player has all required items
    /// </summary>
    private bool CheckRequiredItems(List<ItemData> requiredItems)
    {
        if (Inventory.instance == null)
        {
            Debug.LogError("❌ NPCController: Inventory not found!");
            return false;
        }

        foreach (var requiredItem in requiredItems)
        {
            if (requiredItem == null) continue;

            // Check if item exists in inventory
            bool hasItem = false;
            for (int i = 0; i < Inventory.instance.items.Length; i++)
            {
                if (Inventory.instance.items[i] != null &&
                    Inventory.instance.items[i].itemID == requiredItem.itemID)
                {
                    hasItem = true;
                    break;
                }
            }

            if (!hasItem)
            {
                if (showDebugMessages)
                    Debug.Log($"❌ Player missing required item: {requiredItem.itemName}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Consume required items from player inventory
    /// </summary>
    private void ConsumeItems(List<ItemData> itemsToConsume)
    {
        if (Inventory.instance == null) return;

        foreach (var item in itemsToConsume)
        {
            if (item == null) continue;

            // Find and remove the item from inventory
            for (int i = 0; i < Inventory.instance.items.Length; i++)
            {
                if (Inventory.instance.items[i] != null &&
                    Inventory.instance.items[i].itemID == item.itemID)
                {
                    Inventory.instance.RemoveItemAt(i);

                    if (showDebugMessages)
                        Debug.Log($"🗑 Consumed item: {item.itemName}");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Give reward items to player
    /// </summary>
    private void GiveRewardItems(List<ItemData> rewardItems)
    {
        if (Inventory.instance == null) return;

        foreach (var reward in rewardItems)
        {
            if (reward == null) continue;

            bool success = Inventory.instance.AddItem(reward);

            if (success && showDebugMessages)
                Debug.Log($"🎁 Gave player: {reward.itemName}");
            else if (!success)
                Debug.LogWarning($"⚠ Failed to give {reward.itemName} - inventory full?");
        }
    }

    /// <summary>
    /// Show dialogue using DialogueManager
    /// </summary>
    private void ShowDialogue(List<string> dialogueLines)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("❌ NPCController: DialogueManager not found!");
            return;
        }

        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning($"⚠ {npcData.npcName} has no dialogue lines for stage {currentStage}");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueLines);

        // Set cooldown to prevent immediate re-interaction
        interactionCooldown = 0.5f;

        if (showDebugMessages)
            Debug.Log($"💬 Started dialogue with {npcData.npcName} (Stage: {currentStage})");
    }

    /// <summary>
    /// Create quest marker sprite above NPC head
    /// </summary>
    private void CreateQuestMarker()
    {
        if (questMarkerSprite == null)
        {
            if (showDebugMessages)
                Debug.LogWarning($"⚠ {npcData?.npcName ?? gameObject.name}: questMarkerSprite not assigned");
            return;
        }

        // Create child GameObject for marker
        questMarkerObject = new GameObject("QuestMarker");
        questMarkerObject.transform.SetParent(transform);
        questMarkerObject.transform.localPosition = markerOffset;
        questMarkerObject.transform.localScale = Vector3.one * markerScale;

        // Add SpriteRenderer
        questMarkerRenderer = questMarkerObject.AddComponent<SpriteRenderer>();
        questMarkerRenderer.sprite = questMarkerSprite;

        // Set sorting layer and order
        questMarkerRenderer.sortingLayerName = markerSortingLayer;
        questMarkerRenderer.sortingOrder = markerSortingOrder;

        // Initially hidden
        questMarkerObject.SetActive(false);

        if (showDebugMessages)
            Debug.Log($"✅ Created quest marker for {npcData?.npcName ?? gameObject.name} (Layer: {markerSortingLayer}, Order: {markerSortingOrder})");
    }

    /// <summary>
    /// Update quest marker visibility based on current stage and player proximity
    /// </summary>
    private void UpdateQuestMarker()
    {
        if (questMarkerObject == null || npcData == null) return;

        // Get current stage
        if (QuestManager.Instance != null)
        {
            currentStage = QuestManager.Instance.GetCurrentStage();
        }

        // Show marker only if:
        // 1. Player is in marker range (Circle Collider 2D trigger)
        // 2. NPC has dialogue for current stage
        bool hasDialogue = npcData.HasDialogueForStage(currentStage);
        bool shouldShowMarker = playerInMarkerRange && hasDialogue;

        questMarkerObject.SetActive(shouldShowMarker);

        if (showDebugMessages && questMarkerObject.activeSelf != shouldShowMarker)
        {
            if (shouldShowMarker)
                Debug.Log($"📍 Quest marker shown for {npcData.npcName} (Stage: {currentStage}, Player in marker range)");
            else
                Debug.Log($"📍 Quest marker hidden for {npcData.npcName} (Stage: {currentStage}, Player in marker range: {playerInMarkerRange})");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Test Interaction")]
    private void DebugTestInteraction()
    {
        playerInRange = true;
        StartInteraction();
    }

    [ContextMenu("Debug: Print Current Stage Dialogue")]
    private void DebugPrintCurrentDialogue()
    {
        if (npcData == null)
        {
            Debug.LogError("NPCData not assigned!");
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager not found!");
            return;
        }

        QuestStage stage = QuestManager.Instance.GetCurrentStage();
        NPCDialogueSet dialogueSet = npcData.GetDialogueForStage(stage);

        Debug.Log($"=== {npcData.npcName} at Stage {stage} ===");
        Debug.Log($"Dialogue Lines: {dialogueSet.dialogueLines.Count}");
        Debug.Log($"Required Items: {dialogueSet.GetRequiredItemsText()}");
        Debug.Log($"Reward Items: {dialogueSet.GetRewardItemsText()}");
        Debug.Log($"Advances Stage: {dialogueSet.advanceStageOnComplete}");
    }

    private void OnDrawGizmos()
    {
        // Draw interaction range
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col is BoxCollider2D boxCol)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.offset, boxCol.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show NPC name label
        if (npcData != null)
        {
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"NPC: {npcData.npcName}");
        }
    }
#endif
}
