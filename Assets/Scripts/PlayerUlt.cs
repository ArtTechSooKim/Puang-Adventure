using System.Collections;
using UnityEngine;

/// <summary>
/// Player 궁극기 "Blade Dance (난무)" 시스템
/// R 키로 발동, 8회 연속 슬래시 FX + 전 범위 타격
/// </summary>
public class PlayerUlt : MonoBehaviour
{
    [Header("궁극기 설정")]
    [Tooltip("궁극기 쿨타임 (초)")]
    [SerializeField] private float ultCooldown = 15f;

    [Tooltip("난무 타격 횟수")]
    [SerializeField] private int slashCount = 8;

    [Tooltip("각 슬래시 간격 (초)")]
    [SerializeField] private float slashInterval = 0.15f;

    [Tooltip("궁극기 1회 타격 데미지")]
    [SerializeField] private int ultDamage = 10;

    [Tooltip("1회 타격당 생성할 FX 개수")]
    [SerializeField] private int fxPerSlash = 3;

    [Header("오브젝트 참조")]
    [Tooltip("궁극기 공격 범위 (CircleCollider2D 포함)")]
    [SerializeField] private GameObject ultArea;

    [Tooltip("FX 생성 위치 (Transform)")]
    [SerializeField] private Transform ultEffect;

    [Tooltip("Player SpriteRenderer (난무 시 숨김)")]
    [SerializeField] private SpriteRenderer playerSprite;

    [Tooltip("Player Collider (난무 시 비활성화)")]
    [SerializeField] private Collider2D playerCollider;

    [Header("SlashFx Prefabs")]
    [Tooltip("슬래시 FX 프리팹 배열 (랜덤 선택)")]
    [SerializeField] private GameObject[] slashFxPrefabs;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    // 내부 상태
    private float lastUltTime = -999f; // 마지막 궁극기 사용 시간
    private bool isUltActive = false; // 궁극기 진행 중 여부
    private bool isUltEnabled = false; // 궁극기 기능 활성화 여부 (대화로 해금)
    private CircleCollider2D ultAreaCollider; // UltArea의 CircleCollider2D
    private PlayerController playerController; // PlayerController 참조

    private void Start()
    {
        // UltArea의 CircleCollider2D 가져오기
        if (ultArea != null)
        {
            ultAreaCollider = ultArea.GetComponent<CircleCollider2D>();
            if (ultAreaCollider == null)
            {
                Debug.LogError("[PlayerUlt] UltArea에 CircleCollider2D가 없습니다!");
            }
            else
            {
                // 초기에는 UltArea 비활성화
                ultArea.SetActive(false);
                DebugLog($"UltArea 초기화 완료 (Radius: {ultAreaCollider.radius})");
            }
        }

        // PlayerController 가져오기
        playerController = GetComponent<PlayerController>();

        // 자동 참조 설정 (할당되지 않은 경우)
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
            DebugLog("playerSprite 자동 할당");
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
            DebugLog("playerCollider 자동 할당");
        }

        if (ultEffect == null)
        {
            // UltEffect Transform 찾기
            Transform found = transform.Find("UltEffect");
            if (found != null)
            {
                ultEffect = found;
                DebugLog("ultEffect 자동 할당");
            }
        }

        DebugLog("PlayerUlt 초기화 완료");
    }

    private void Update()
    {
        // 궁극기 진행 중이면 입력 차단
        if (isUltActive) return;

        // R 키 입력 감지
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryActivateUlt();
        }
    }

    /// <summary>
    /// 궁극기 기능을 활성화합니다. (외부 호출용 - DialogueManager)
    /// </summary>
    public void EnableUlt()
    {
        Debug.Log($"[PlayerUlt] EnableUlt() 호출됨 - 현재 상태: {isUltEnabled}");
        isUltEnabled = true;
        Debug.Log($"[PlayerUlt] ✅ 궁극기 기능이 활성화되었습니다! 새 상태: {isUltEnabled}");
        Debug.Log($"[PlayerUlt] 이제 R 키를 눌러 궁극기 'Blade Dance'를 사용할 수 있습니다.");
    }

    /// <summary>
    /// 궁극기 기능을 비활성화합니다.
    /// </summary>
    public void DisableUlt()
    {
        isUltEnabled = false;
        DebugLog("궁극기 기능이 비활성화되었습니다.");
    }

    /// <summary>
    /// 현재 궁극기 활성화 상태를 반환합니다.
    /// </summary>
    public bool IsUltEnabled()
    {
        return isUltEnabled;
    }

    /// <summary>
    /// 궁극기 발동 시도
    /// </summary>
    private void TryActivateUlt()
    {
        Debug.Log($"[PlayerUlt] TryActivateUlt() 호출됨");
        Debug.Log($"[PlayerUlt] 현재 isUltEnabled 상태: {isUltEnabled}");

        // 궁극기 활성화 여부 체크
        if (!isUltEnabled)
        {
            Debug.Log($"[PlayerUlt] ❌ 궁극기 입력 차단: 궁극기 기능 비활성화 상태 (isUltEnabled={isUltEnabled})");
            return;
        }

        Debug.Log($"[PlayerUlt] ✅ 궁극기 활성화 체크 통과!");

        // 쿨타임 체크
        if (Time.time < lastUltTime + ultCooldown)
        {
            float remainingCooldown = (lastUltTime + ultCooldown) - Time.time;
            Debug.Log($"[PlayerUlt] ❌ 궁극기 쿨타임 중! 남은 시간: {remainingCooldown:F1}초");
            return;
        }

        // 궁극기 발동
        Debug.Log("[PlayerUlt] 🔥 궁극기 'Blade Dance' 발동!");
        lastUltTime = Time.time;
        StartCoroutine(BladeDanceRoutine());
    }

    /// <summary>
    /// 궁극기 "Blade Dance" 메인 루틴
    /// </summary>
    private IEnumerator BladeDanceRoutine()
    {
        isUltActive = true;

        // 1. Player 숨기기 & 무적 & 이동/공격 불가
        ActivatePlayerInvincibility();

        // 2. UltArea 활성화
        if (ultArea != null)
        {
            ultArea.SetActive(true);
            DebugLog("UltArea 활성화");
        }

        // 궁극기 슬래시 소리 재생 (첫 타격 시작 시 1회만)
        if (AudioManager.I != null)
        {
            AudioManager.I.PlayUltimateSlashSound();
        }

        // 3. 8회 슬래시 FX + 타격
        for (int i = 0; i < slashCount; i++)
        {
            DebugLog($"⚔ 난무 {i + 1}/{slashCount} 타격!");

            // FX 생성 (1회당 여러 개)
            for (int j = 0; j < fxPerSlash; j++)
            {
                SpawnSlashFx();
            }

            // 타격 판정
            PerformUltAttack();

            // 다음 슬래시까지 대기
            yield return new WaitForSeconds(slashInterval);
        }

        // 4. UltArea 비활성화
        if (ultArea != null)
        {
            ultArea.SetActive(false);
            DebugLog("UltArea 비활성화");
        }

        // 5. Player 복귀
        DeactivatePlayerInvincibility();

        isUltActive = false;
        DebugLog("✅ 궁극기 종료! 쿨타임 시작");
    }

    /// <summary>
    /// Player 무적/은폐 활성화
    /// </summary>
    private void ActivatePlayerInvincibility()
    {
        // SpriteRenderer 숨기기
        if (playerSprite != null)
        {
            playerSprite.enabled = false;
            DebugLog("Player 외형 숨김");
        }

        // Collider 비활성화 (적과 충돌 방지)
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
            DebugLog("Player Collider 비활성화");
        }

        // PlayerController 이동/공격 차단
        if (playerController != null)
        {
            playerController.SetUltActive(true);
        }
    }

    /// <summary>
    /// Player 무적/은폐 해제
    /// </summary>
    private void DeactivatePlayerInvincibility()
    {
        // SpriteRenderer 다시 표시
        if (playerSprite != null)
        {
            playerSprite.enabled = true;
            DebugLog("Player 외형 복구");
        }

        // Collider 다시 활성화
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
            DebugLog("Player Collider 활성화");
        }

        // PlayerController 이동/공격 복구
        if (playerController != null)
        {
            playerController.SetUltActive(false);
        }
    }

    /// <summary>
    /// 슬래시 FX 생성 (랜덤 회전 + UltArea 범위 내 랜덤 위치)
    /// </summary>
    private void SpawnSlashFx()
    {
        if (slashFxPrefabs == null || slashFxPrefabs.Length == 0)
        {
            Debug.LogWarning("[PlayerUlt] slashFxPrefabs가 비어있습니다!");
            return;
        }

        if (ultEffect == null)
        {
            Debug.LogWarning("[PlayerUlt] ultEffect가 할당되지 않았습니다!");
            return;
        }

        if (ultAreaCollider == null)
        {
            Debug.LogWarning("[PlayerUlt] ultAreaCollider가 할당되지 않았습니다!");
            return;
        }

        // 랜덤 프리팹 선택
        GameObject randomPrefab = slashFxPrefabs[Random.Range(0, slashFxPrefabs.Length)];

        // UltArea 범위 내 랜덤 위치 생성
        Vector2 randomOffset = Random.insideUnitCircle * ultAreaCollider.radius;
        Vector3 spawnPosition = ultEffect.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // FX 생성 (랜덤 360도 회전)
        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        GameObject fxInstance = Instantiate(randomPrefab, spawnPosition, randomRotation);

        DebugLog($"SlashFx 생성: {randomPrefab.name} at {spawnPosition} (offset: {randomOffset})");
    }

    /// <summary>
    /// 궁극기 타격 판정 (UltArea 범위 내 모든 적)
    /// </summary>
    private void PerformUltAttack()
    {
        if (ultAreaCollider == null)
        {
            Debug.LogWarning("[PlayerUlt] ultAreaCollider가 없습니다!");
            return;
        }

        // UltArea 범위 내 모든 적 검색
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            ultAreaCollider.transform.position,
            ultAreaCollider.radius,
            LayerMask.GetMask("Enemy") // Enemy 레이어 사용
        );

        DebugLog($"UltArea 범위 내 적 발견: {hits.Length}명");

        // 각 적에게 데미지 적용
        foreach (var hit in hits)
        {
            // EnemyHealth 스크립트가 있는 경우
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(ultDamage);
                DebugLog($"  → {hit.name}에게 {ultDamage} 데미지!");
            }
            else
            {
                DebugLog($"  → {hit.name}: EnemyHealth 컴포넌트 없음");
            }
        }
    }

    /// <summary>
    /// 디버그 로그 출력
    /// </summary>
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PlayerUlt] {message}");
        }
    }

    /// <summary>
    /// 현재 궁극기 쿨타임 상태 확인 (외부 호출용)
    /// </summary>
    public bool IsOnCooldown()
    {
        return Time.time < lastUltTime + ultCooldown;
    }

    /// <summary>
    /// 남은 쿨타임 시간 반환 (외부 호출용)
    /// </summary>
    public float GetRemainingCooldown()
    {
        if (IsOnCooldown())
        {
            return (lastUltTime + ultCooldown) - Time.time;
        }
        return 0f;
    }

    // Gizmos로 UltArea 범위 시각화 (Scene 뷰에서만 보임)
    private void OnDrawGizmosSelected()
    {
        if (ultArea != null && ultAreaCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ultAreaCollider.transform.position, ultAreaCollider.radius);
        }
    }
}
