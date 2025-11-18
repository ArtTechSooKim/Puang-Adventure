using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 최종 보스 처치 시 자동으로 Village로 복귀합니다.
/// BossScene (Stage7): 거대 버섯 보스 처치 → Village 자동 복귀
/// </summary>
public class BossDefeatHandler : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossGameObject; // 보스 GameObject (자동 탐색 가능)
    [SerializeField] private string bossTag = "Boss"; // 보스 태그

    [Header("Transition Settings")]
    [SerializeField] private string returnSceneName = "02_VillageScene";
    [SerializeField] private float victoryMessageDuration = 4f; // 승리 메시지 표시 시간
    [SerializeField] private bool showDebugMessages = true;

    private bool bossDefeated = false;

    private void Start()
    {
        if (showDebugMessages)
            Debug.Log("🏆 BossDefeatHandler: Monitoring boss...");

        // Boss GameObject 자동 탐색
        if (bossGameObject == null)
        {
            bossGameObject = GameObject.FindGameObjectWithTag(bossTag);

            if (bossGameObject == null)
            {
                Debug.LogWarning($"⚠ Boss GameObject with tag '{bossTag}' not found!");
            }
            else
            {
                if (showDebugMessages)
                    Debug.Log($"✅ Found boss: {bossGameObject.name}");
            }
        }
    }

    private void Update()
    {
        if (bossDefeated)
            return;

        // 보스가 파괴되었는지 확인
        if (bossGameObject == null || !bossGameObject.activeInHierarchy)
        {
            CheckBossDefeated();
        }
    }

    /// <summary>
    /// 보스 처치 확인
    /// </summary>
    private void CheckBossDefeated()
    {
        if (bossDefeated)
            return;

        // 보스 GameObject가 파괴되었거나 비활성화되었는지 확인
        GameObject boss = GameObject.FindGameObjectWithTag(bossTag);

        // 보스가 없거나 비활성화되었으면 처치된 것으로 간주
        if (boss == null || !boss.activeInHierarchy)
        {
            OnBossDefeated();
        }
    }

    /// <summary>
    /// 보스 처치 시 호출
    /// </summary>
    private void OnBossDefeated()
    {
        if (bossDefeated)
            return;

        bossDefeated = true;

        if (showDebugMessages)
            Debug.Log("🎉 Boss defeated! Returning to Village...");

        // Stage 진행
        if (QuestManager.Instance != null)
        {
            QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

            if (currentStage == QuestStage.Stage7_FinalBoss)
            {
                QuestManager.Instance.AdvanceStage(); // Stage7 → Stage8
                if (showDebugMessages)
                    Debug.Log("📈 Advanced to Stage8_Ending");
            }
        }

        // 승리 메시지 표시 및 복귀
        StartCoroutine(ShowVictoryAndReturn());
    }

    /// <summary>
    /// 승리 메시지 표시 후 Village로 복귀
    /// </summary>
    private IEnumerator ShowVictoryAndReturn()
    {
        // 승리 메시지
        if (DialogueManager.Instance != null)
        {
            string victoryMessage = "\"이제 푸앙이에게 이걸 가져다 주자..!\"";
            DialogueManager.Instance.StartDialogue(new System.Collections.Generic.List<string> { victoryMessage });
        }

        // 대기
        yield return new WaitForSeconds(victoryMessageDuration);

        // Village로 복귀
        if (showDebugMessages)
            Debug.Log($"🌀 Returning to Village: {returnSceneName}");

        SceneManager.LoadScene(returnSceneName);
    }

    /// <summary>
    /// 공개 메서드: 외부에서 보스 처치 트리거
    /// </summary>
    public void TriggerBossDefeat()
    {
        OnBossDefeated();
    }

    /// <summary>
    /// 디버그: 즉시 Village로 복귀
    /// </summary>
    [ContextMenu("Debug: Force Boss Defeat")]
    private void DebugForceBossDefeat()
    {
        OnBossDefeated();
    }
}
