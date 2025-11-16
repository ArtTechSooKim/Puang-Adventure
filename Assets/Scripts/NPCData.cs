using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject containing all NPC data including stage-based dialogues and quest items
/// </summary>
[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("NPC Info")]
    public string npcName = "NPC";
    public Sprite npcSprite;

    [Header("Stage-Based Dialogues")]
    [Tooltip("List of dialogue sets for different quest stages")]
    public List<NPCDialogueSet> dialogueSets = new List<NPCDialogueSet>();

    [Header("Quest Interactions")]
    [Tooltip("Does this NPC give items during quest progression?")]
    public bool givesQuestItems = false;

    [Tooltip("Does this NPC require items to progress?")]
    public bool requiresQuestItems = false;

    /// <summary>
    /// Get the dialogue set for a specific quest stage
    /// </summary>
    public NPCDialogueSet GetDialogueForStage(QuestStage stage)
    {
        foreach (var dialogueSet in dialogueSets)
        {
            if (dialogueSet.questStage == stage)
                return dialogueSet;
        }

        // Return default/fallback dialogue if no specific stage dialogue found
        if (dialogueSets.Count > 0)
        {
            Debug.LogWarning($"⚠ No dialogue found for {npcName} at stage {stage}. Using first available dialogue.");
            return dialogueSets[0];
        }

        // Return empty dialogue set if none exists
        Debug.LogError($"❌ {npcName} has no dialogue sets configured!");
        return new NPCDialogueSet();
    }

    /// <summary>
    /// Check if this NPC has dialogue for a specific stage
    /// </summary>
    public bool HasDialogueForStage(QuestStage stage)
    {
        foreach (var dialogueSet in dialogueSets)
        {
            if (dialogueSet.questStage == stage)
                return true;
        }
        return false;
    }
}

/// <summary>
/// Defines a set of dialogues and quest interactions for a specific quest stage
/// </summary>
[System.Serializable]
public struct NPCDialogueSet
{
    [Header("Stage Configuration")]
    [Tooltip("Which quest stage this dialogue belongs to")]
    public QuestStage questStage;

    [Header("Dialogue Lines")]
    [Tooltip("The dialogue lines to display")]
    [TextArea(2, 6)]
    public List<string> dialogueLines;

    [Header("Quest Item Requirements (Optional)")]
    [Tooltip("Items required from player to progress (consumed on interaction)")]
    public List<ItemData> requiredItems;

    [Header("Quest Item Rewards (Optional)")]
    [Tooltip("Items given to player after dialogue/interaction")]
    public List<ItemData> rewardItems;

    [Header("Stage Progression")]
    [Tooltip("Advance quest stage after this interaction?")]
    public bool advanceStageOnComplete;

    [Tooltip("Custom message if player doesn't have required items")]
    public string insufficientItemsMessage;

    /// <summary>
    /// Check if this dialogue set has item requirements
    /// </summary>
    public bool HasRequirements()
    {
        return requiredItems != null && requiredItems.Count > 0;
    }

    /// <summary>
    /// Check if this dialogue set gives rewards
    /// </summary>
    public bool HasRewards()
    {
        return rewardItems != null && rewardItems.Count > 0;
    }

    /// <summary>
    /// Get a formatted string of required items
    /// </summary>
    public string GetRequiredItemsText()
    {
        if (!HasRequirements())
            return "None";

        List<string> itemNames = new List<string>();
        foreach (var item in requiredItems)
        {
            if (item != null)
                itemNames.Add(item.itemName);
        }

        return string.Join(", ", itemNames);
    }

    /// <summary>
    /// Get a formatted string of reward items
    /// </summary>
    public string GetRewardItemsText()
    {
        if (!HasRewards())
            return "None";

        List<string> itemNames = new List<string>();
        foreach (var item in rewardItems)
        {
            if (item != null)
                itemNames.Add(item.itemName);
        }

        return string.Join(", ", itemNames);
    }
}
