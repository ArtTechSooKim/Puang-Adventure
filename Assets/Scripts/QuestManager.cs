using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages the global quest progression system.
/// Singleton pattern with DontDestroyOnLoad to persist across scenes.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Progress")]
    [SerializeField] private QuestStage currentStage = QuestStage.Stage0_VillageTutorial;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"⚠ QuestManager: Duplicate instance detected in scene! Destroying this ({gameObject.name}). Existing instance at stage: {Instance.GetCurrentStage()}");
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
        // Stage8 도달 시 EndingScene으로 자동 이동
        if (to == QuestStage.Stage8_Ending)
        {
            if (showDebugMessages)
                Debug.Log("🎬 Stage8 reached! Loading EndingScene...");

            StartCoroutine(LoadEndingScene());
        }
    }

    /// <summary>
    /// EndingScene 로드 (짧은 딜레이 후)
    /// </summary>
    private IEnumerator LoadEndingScene()
    {
        // 짧은 딜레이 (대화 종료 등을 위해)
        yield return new WaitForSeconds(0.5f);

        if (showDebugMessages)
            Debug.Log("🌀 Loading EndingScene...");

        SceneManager.LoadScene("08_EndingScene");
    }

    /// <summary>
    /// Reset quest progress to the beginning (for new game)
    /// </summary>
    public void ResetQuest()
    {
        currentStage = QuestStage.Stage0_VillageTutorial;

        if (showDebugMessages)
            Debug.Log("🔄 Quest Reset to Stage0_VillageTutorial");
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
    Stage0_VillageTutorial = 0,      // VillageScene - 칼자루 획득 (프롤로그)
    Stage1_ForestHunt = 1,            // ForestScene - 슬라임2 + 박쥐2 처치
    Stage2_WeaponUpgrade1 = 2,        // VillageScene - 무기 1차 강화 (숲의 검)
    Stage3_CaveExploration = 3,       // CaveScene - 박쥐5 + 해골5 처치
    Stage4_PeuangSadCutscene = 4,     // PeuangSadScene - 퓨앙이 컷씬
    Stage5_UnkillableBoss = 5,        // UnkillableBossScene - 필패 보스전
    Stage6_WeaponUpgrade2 = 6,        // VillageScene - 무기 2차 강화 (중붕이의 검)
    Stage7_FinalBoss = 7,             // BossScene - 거대 버섯 보스 처치
    Stage8_Ending = 8                 // EndingScene - 엔딩
}
