using UnityEngine;

/// <summary>
/// Manages the global quest progression system.
/// Singleton pattern with DontDestroyOnLoad to persist across scenes.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Progress")]
    [SerializeField] private QuestStage currentStage = QuestStage.Stage0_Tutorial;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (showDebugMessages)
            Debug.Log($"✅ QuestManager initialized at Stage: {currentStage}");
    }

    /// <summary>
    /// Get the current quest stage
    /// </summary>
    public QuestStage GetCurrentStage()
    {
        return currentStage;
    }

    /// <summary>
    /// Advance to the next quest stage
    /// </summary>
    public void AdvanceStage()
    {
        QuestStage nextStage = (QuestStage)((int)currentStage + 1);

        // Check if we've reached the end
        if ((int)nextStage >= System.Enum.GetValues(typeof(QuestStage)).Length)
        {
            if (showDebugMessages)
                Debug.LogWarning("⚠ QuestManager: Already at final stage, cannot advance further.");
            return;
        }

        QuestStage previousStage = currentStage;
        currentStage = nextStage;

        if (showDebugMessages)
            Debug.Log($"📈 Quest Advanced: {previousStage} → {currentStage}");

        // Trigger any stage-specific events
        OnStageChanged(previousStage, currentStage);
    }

    /// <summary>
    /// Set the quest stage directly (useful for debugging or loading saves)
    /// </summary>
    public void SetStage(QuestStage stage)
    {
        QuestStage previousStage = currentStage;
        currentStage = stage;

        if (showDebugMessages)
            Debug.Log($"🎯 Quest Stage Set: {previousStage} → {currentStage}");

        OnStageChanged(previousStage, currentStage);
    }

    /// <summary>
    /// Check if a specific stage requirement is met
    /// </summary>
    public bool IsStageReached(QuestStage requiredStage)
    {
        return (int)currentStage >= (int)requiredStage;
    }

    /// <summary>
    /// Check if the current stage matches exactly
    /// </summary>
    public bool IsCurrentStage(QuestStage stage)
    {
        return currentStage == stage;
    }

    /// <summary>
    /// Called whenever the quest stage changes
    /// Override this to add custom behavior on stage transitions
    /// </summary>
    private void OnStageChanged(QuestStage from, QuestStage to)
    {
        // You can add custom logic here for specific stage transitions
        // Example: Play sound effects, show notifications, unlock areas, etc.
    }

    /// <summary>
    /// Reset quest progress to the beginning (for new game)
    /// </summary>
    public void ResetQuest()
    {
        currentStage = QuestStage.Stage0_Tutorial;

        if (showDebugMessages)
            Debug.Log("🔄 Quest Reset to Stage0_Tutorial");
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Advance Stage")]
    private void DebugAdvanceStage()
    {
        AdvanceStage();
    }

    [ContextMenu("Debug: Reset Quest")]
    private void DebugResetQuest()
    {
        ResetQuest();
    }

    [ContextMenu("Debug: Print Current Stage")]
    private void DebugPrintStage()
    {
        Debug.Log($"Current Quest Stage: {currentStage} (Index: {(int)currentStage})");
    }
#endif
}

/// <summary>
/// Defines all quest stages in the game progression
/// </summary>
public enum QuestStage
{
    Stage0_Tutorial = 0,        // 튜토리얼 단계
    Stage1_FirstQuest = 1,      // 첫 번째 퀘스트
    Stage2_WeaponUpgrade = 2,   // 무기 업그레이드 단계
    Stage3_BossPreparation = 3, // 보스 준비 단계
    Stage4_BossDefeated = 4,    // 보스 격파 후
    Stage5_FinalQuest = 5,      // 최종 퀘스트
    Stage6_GameComplete = 6     // 게임 완료
}
