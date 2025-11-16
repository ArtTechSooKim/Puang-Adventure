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

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private bool playerInRange = false;
    private QuestStage currentStage;

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
    }

    private void Update()
    {
        if (!playerInRange || npcData == null) return;

        // Block all input if dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            return;
        }

        // Update current quest stage
        if (QuestManager.Instance != null)
        {
            currentStage = QuestManager.Instance.GetCurrentStage();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Check if NPC has dialogue for current stage
        bool hasDialogue = false;
        if (QuestManager.Instance != null)
        {
            currentStage = QuestManager.Instance.GetCurrentStage();
            hasDialogue = npcData.HasDialogueForStage(currentStage);
        }

        // Show interaction prompt only if NPC has dialogue for current stage
        if (interactionPrompt != null)
            interactionPrompt.SetActive(hasDialogue);

        if (showDebugMessages)
        {
            if (hasDialogue)
                Debug.Log($"💬 Player entered {npcData.npcName}'s interaction range");
            else
                Debug.Log($"⏸ Player entered {npcData.npcName}'s range (no dialogue for stage {currentStage})");
        }

        // Auto-open dialogue if enabled (only if has dialogue)
        if (autoOpenOnEnter && hasDialogue)
        {
            StartInteraction();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // Hide interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (showDebugMessages)
            Debug.Log($"💬 Player left {npcData.npcName}'s interaction range");
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

        // Advance quest stage if configured
        if (dialogueSet.advanceStageOnComplete)
        {
            QuestManager.Instance.AdvanceStage();

            if (showDebugMessages)
                Debug.Log($"📈 Quest advanced by {npcData.npcName}");
        }
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

        if (showDebugMessages)
            Debug.Log($"💬 Started dialogue with {npcData.npcName} (Stage: {currentStage})");
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
