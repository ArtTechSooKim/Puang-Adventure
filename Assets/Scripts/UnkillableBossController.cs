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
    [SerializeField] private float fadeInDuration = 2f; // 검은 화면에서 밝아지는 시간
    [SerializeField] private bool showDebugMessages = true;

    [Header("Fade Settings")]
    [SerializeField] private UnityEngine.UI.Image fadeImage; // 검은 화면용 Image (자동 생성)

    private bool playerDied = false;
    private float timer = 0f;
    private int initialPlayerHealth = -1; // 플레이어 최초 체력 저장용

    private void Start()
    {
        if (showDebugMessages)
            Debug.Log("💀 UnkillableBossController: Scene started!");

        // Fade Image 자동 생성 (없을 경우)
        CreateFadeImageIfNeeded();

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

        // Village로 복귀 (사망 메시지는 Village 씬 이동 후 표시)
        StartCoroutine(ReturnToVillage());
    }

    /// <summary>
    /// Village로 복귀 (검은 화면에서 페이드 인 후 메시지 표시)
    /// </summary>
    private IEnumerator ReturnToVillage()
    {
        // Village 씬에서 페이드 인 후 사망 메시지를 표시하도록 PlayerPersistent에 미리 저장
        // PlayerPersistent가 \n으로 구분하여 여러 줄로 표시함
        PlayerPersistent.Instance?.SetPendingDialogue(
            "\"으아... 꿈 속이었지만 거대 버섯은 정말 무시무시했어..\"\n\"중붕이를 찾아가 마지막 사냥을 준비하자!\"",
            withFadeIn: true
        );

        // 검은 화면으로 페이드 아웃
        yield return StartCoroutine(FadeToBlack());

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

            // Player 애니메이션을 Idle 상태로 강제 전환
            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                // Dead 트리거 리셋
                playerAnim.ResetTrigger("Dead");

                // Idle 상태로 전환 (IsWalking = false)
                playerAnim.SetBool("IsWalking", false);

                if (showDebugMessages)
                    Debug.Log("✅ Player animation reset to Idle");
            }
        }

        if (showDebugMessages)
            Debug.Log($"🌀 Returning to Village: {returnSceneName}");

        // 씬 로드
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

    /// <summary>
    /// Fade Image 자동 생성 (없을 경우)
    /// </summary>
    private void CreateFadeImageIfNeeded()
    {
        if (fadeImage != null) return;

        // Canvas 찾기 또는 생성
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 최상위 레이어
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Fade Image 생성
        GameObject fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(canvas.transform, false);

        fadeImage = fadeObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // 투명한 검은색

        // 전체 화면 크기로 설정
        RectTransform rectTransform = fadeObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        if (showDebugMessages)
            Debug.Log("✅ UnkillableBossController: Fade Image 자동 생성 완료");
    }

    /// <summary>
    /// 검은 화면으로 페이드 아웃
    /// </summary>
    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;

        // 시간 정지
        Time.timeScale = 0f;

        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1); // 불투명한 검은색

        while (elapsed < fadeInDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (fadeInDuration / 2f);
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        fadeImage.color = targetColor;

        if (showDebugMessages)
            Debug.Log("🌑 Faded to black");
    }

    /// <summary>
    /// 검은 화면에서 페이드 인
    /// </summary>
    private IEnumerator FadeFromBlack()
    {
        if (fadeImage == null)
        {
            // 씬 전환 후 Fade Image를 다시 찾아야 함
            CreateFadeImageIfNeeded();
        }

        if (fadeImage == null)
        {
            // 그래도 없으면 시간만 복구하고 종료
            Time.timeScale = 1f;
            yield break;
        }

        // 검은 화면으로 시작
        fadeImage.color = new Color(0, 0, 0, 1);

        yield return new WaitForSecondsRealtime(0.5f); // 잠깐 대기

        float elapsed = 0f;
        Color startColor = new Color(0, 0, 0, 1); // 불투명한 검은색
        Color targetColor = new Color(0, 0, 0, 0); // 투명한 검은색

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        fadeImage.color = targetColor;

        // 시간 복구
        Time.timeScale = 1f;

        if (showDebugMessages)
            Debug.Log("☀ Faded from black - player can now move");
    }
}
