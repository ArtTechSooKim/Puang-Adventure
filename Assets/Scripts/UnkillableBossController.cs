using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 절대 이길 수 없는 보스 Scene 컨트롤러
/// 플레이어가 한 대 맞거나 일정 시간 후 강제 사망 → Village로 복귀
/// </summary>
public class UnkillableBossController : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossGameObject; // 보스 GameObject
    [SerializeField] private float bossInvincibilityHP = 999999f; // 무적 체력

    [Header("Player Death Settings")]
    [SerializeField] private bool instantDeathOnHit = true; // 한 대 맞으면 즉사
    [SerializeField] private float autoDeathTime = 10f; // 자동 사망 시간 (초)

    [Header("Transition Settings")]
    [SerializeField] private string returnSceneName = "02_VillageScene";
    [SerializeField] private float deathMessageDuration = 3f; // 사망 메시지 표시 시간
    [SerializeField] private bool showDebugMessages = true;

    private bool playerDied = false;
    private float timer = 0f;
    private int initialPlayerHealth = -1; // 플레이어 최초 체력 저장용

    private void Start()
    {
        if (showDebugMessages)
            Debug.Log("💀 UnkillableBossController: Scene started!");

        // PlayerHealth의 일반 사망 처리 비활성화
        DisablePlayerDeathProcessing();

        // Boss를 무적으로 설정
        if (bossGameObject != null)
        {
            // Boss의 Health 컴포넌트를 찾아서 무적으로 설정
            var bossHealth = bossGameObject.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.SetInvincible(true);
                if (showDebugMessages)
                    Debug.Log($"💪 Boss set to invincible!");
            }
            else
            {
                Debug.LogWarning("⚠ UnkillableBossController: Boss has no EnemyHealth component!");
            }
        }
        else
        {
            Debug.LogWarning("⚠ UnkillableBossController: Boss GameObject is not assigned!");
        }

        // Stage 확인 및 진행
        if (QuestManager.Instance != null)
        {
            QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

            if (showDebugMessages)
                Debug.Log($"🎯 Current Stage: {currentStage}");

            // Stage5가 아니면 자동으로 Stage5로 설정
            if (currentStage == QuestStage.Stage4_PeuangSadCutscene)
            {
                QuestManager.Instance.AdvanceStage(); // Stage4 → Stage5
                if (showDebugMessages)
                    Debug.Log("📈 Advanced to Stage5_UnkillableBoss");
            }
        }

        // 자동 사망 타이머 시작
        StartCoroutine(AutoDeathTimer());
    }

    /// <summary>
    /// PlayerHealth의 일반 사망 처리를 비활성화
    /// 이 씬에서는 UnkillableBossController가 사망을 처리함
    /// </summary>
    private void DisablePlayerDeathProcessing()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetIgnoreDeathProcessing(true);

                if (showDebugMessages)
                    Debug.Log("✅ UnkillableBossController: Disabled normal death processing");
            }
            else
            {
                Debug.LogWarning("⚠ UnkillableBossController: PlayerHealth not found on Player!");
            }
        }
        else
        {
            Debug.LogWarning("⚠ UnkillableBossController: Player GameObject not found!");
        }
    }

    private void Update()
    {
        // 플레이어 체력 확인 (선택 사항)
        if (!playerDied && instantDeathOnHit)
        {
            CheckPlayerHealth();
        }
    }

    /// <summary>
    /// 플레이어 체력 확인 (한 대 맞으면 강제 사망)
    /// </summary>
    private void CheckPlayerHealth()
    {
        // Player GameObject 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 최초 체력을 저장하지 않았으면 현재 체력을 최초 체력으로 저장
                if (initialPlayerHealth == -1)
                {
                    initialPlayerHealth = playerHealth.GetCurrentHealth();
                }

                // 체력이 최초 체력보다 낮으면 강제 사망
                if (playerHealth.GetCurrentHealth() < initialPlayerHealth && !playerDied)
                {
                    if (showDebugMessages)
                        Debug.Log("💔 Player took damage! Forcing death...");

                    ForcePlayerDeath();
                }
            }
        }
    }

    /// <summary>
    /// 자동 사망 타이머
    /// </summary>
    private IEnumerator AutoDeathTimer()
    {
        yield return new WaitForSeconds(autoDeathTime);

        if (!playerDied)
        {
            if (showDebugMessages)
                Debug.Log($"⏰ Auto death timer expired ({autoDeathTime}s). Forcing death...");

            ForcePlayerDeath();
        }
    }

    /// <summary>
    /// 플레이어 강제 사망 및 Village 복귀
    /// </summary>
    private void ForcePlayerDeath()
    {
        if (playerDied)
            return;

        playerDied = true;

        // 사망 메시지 표시
        if (DialogueManager.Instance != null)
        {
            string deathMessage = "\"으아... 꿈 속이었지만 거대 버섯은 정말 무시무시했어..\"\n\"중붕이를 찾아가 마지막 사냥을 준비하자!\"";
            DialogueManager.Instance.StartDialogue(new System.Collections.Generic.List<string> { deathMessage });
        }

        // Village로 복귀
        StartCoroutine(ReturnToVillage());
    }

    /// <summary>
    /// Village로 복귀
    /// </summary>
    private IEnumerator ReturnToVillage()
    {
        yield return new WaitForSeconds(deathMessageDuration);

        // Quest Stage 진행 (Stage5 → Stage6)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AdvanceStage(); // Stage5 → Stage6
            if (showDebugMessages)
                Debug.Log("📈 Advanced to Stage6_WeaponUpgrade2");
        }

        // 플레이어 체력 회복 및 사망 처리 재활성화
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
                playerHealth.SetIgnoreDeathProcessing(false); // 일반 사망 처리 재활성화

                if (showDebugMessages)
                    Debug.Log("💚 Player health restored and death processing re-enabled");
            }
        }

        if (showDebugMessages)
            Debug.Log($"🌀 Returning to Village: {returnSceneName}");

        SceneManager.LoadScene(returnSceneName);
    }

    /// <summary>
    /// 공개 메서드: 외부에서 강제 사망 트리거
    /// </summary>
    public void TriggerPlayerDeath()
    {
        ForcePlayerDeath();
    }

    /// <summary>
    /// 디버그: 즉시 Village로 복귀
    /// </summary>
    [ContextMenu("Debug: Return to Village")]
    private void DebugReturnToVillage()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(returnSceneName);
    }
}
