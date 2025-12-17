using UnityEngine;

/// <summary>
/// 검기 이펙트 컨트롤러
/// 애니메이션 재생 후 자동으로 SpriteRenderer를 비활성화하여 렉 걸린 것처럼 남아있는 문제 해결
/// </summary>
public class SlashEffectController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("이펙트가 자동으로 숨겨질 때까지 대기 시간 (초) - 애니메이션 길이보다 길게 설정")]
    [SerializeField] private float autoHideDelay = 0.5f;

    [Tooltip("디버그 메시지 출력 여부")]
    [SerializeField] private bool showDebugMessages = false;

    private SpriteRenderer[] spriteRenderers;
    private Animator animator;
    private float hideTimer = -1f;
    private bool isPlaying = false;
    private bool useAnimatorTiming = true; // Animator 기반 타이밍 사용 여부

    private void Awake()
    {
        // 이 GameObject와 직속 자식들의 SpriteRenderer만 가져오기 (Player 제외)
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        // Player의 SpriteRenderer 제외 (태그로 확인)
        System.Collections.Generic.List<SpriteRenderer> validRenderers = new System.Collections.Generic.List<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            // Player 태그를 가진 GameObject의 SpriteRenderer는 제외
            if (sr != null && !sr.CompareTag("Player"))
            {
                validRenderers.Add(sr);
            }
            else if (sr != null && sr.CompareTag("Player"))
            {
                Debug.Log($"[SlashEffectController] Player SpriteRenderer 제외: {sr.gameObject.name}");
            }
        }
        spriteRenderers = validRenderers.ToArray();

        animator = GetComponent<Animator>();

        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning($"⚠ SlashEffectController ({gameObject.name}): SpriteRenderer를 찾을 수 없습니다!");
        }
        else
        {
            Debug.Log($"[SlashEffectController] {spriteRenderers.Length}개의 SpriteRenderer 발견 (Player 제외)");
        }

        // Animator가 있으면 자동 감지 모드, 없으면 타이머 모드
        if (animator != null)
        {
            useAnimatorTiming = true;
            Debug.Log("[SlashEffectController] Animator 발견 - 자동 감지 모드");
        }
        else
        {
            useAnimatorTiming = false;
            Debug.LogWarning("[SlashEffectController] Animator 없음 - 타이머 모드로 전환");
        }

        // 초기 상태: 모든 SpriteRenderer 비활성화
        HideEffect();
    }

    private void OnEnable()
    {
        // Enable될 때마다 타이머 리셋
        hideTimer = -1f;
        isPlaying = false;

        if (showDebugMessages)
            Debug.Log($"[SlashEffect] {gameObject.name} OnEnable");
    }

    private void Update()
    {
        // Animator에서 Attack 트리거가 발동되었는지 확인
        if (animator != null && useAnimatorTiming)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Attack 애니메이션이 재생 중인지 확인
            bool isAttackAnimPlaying = stateInfo.IsName("Attack") ||
                                        stateInfo.IsName("SlashAttack") ||
                                        stateInfo.IsName("Slash");

            if (isAttackAnimPlaying && !isPlaying)
            {
                // 애니메이션 시작
                isPlaying = true;
                ShowEffect();
                hideTimer = -1f; // Animator 기반일 때는 타이머 비활성화

                if (showDebugMessages)
                    Debug.Log($"[SlashEffect] 애니메이션 시작 - {stateInfo.normalizedTime:F2}");
            }
            else if (isPlaying && stateInfo.normalizedTime >= 1.0f)
            {
                // 애니메이션 완전히 끝남 (100% 이상)
                if (showDebugMessages)
                    Debug.Log($"[SlashEffect] 애니메이션 완료 - {stateInfo.normalizedTime:F2}");

                isPlaying = false;
                HideEffect();
            }
            else if (isPlaying && !isAttackAnimPlaying)
            {
                // 애니메이션이 중단되거나 다른 상태로 전환됨
                if (showDebugMessages)
                    Debug.Log($"[SlashEffect] 애니메이션 중단 감지");

                isPlaying = false;
                HideEffect();
            }
        }

        // 타이머 기반 자동 숨김 (PlayEffect()로 직접 호출된 경우)
        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0f)
            {
                HideEffect();
                hideTimer = -1f;
                isPlaying = false;

                if (showDebugMessages)
                    Debug.Log($"[SlashEffect] 타이머 만료 - 이펙트 숨김");
            }
        }
    }

    /// <summary>
    /// 이펙트 표시 (SpriteRenderer 활성화)
    /// </summary>
    private void ShowEffect()
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = true;
            }
        }

        if (showDebugMessages)
            Debug.Log($"[SlashEffect] 이펙트 표시됨");
    }

    /// <summary>
    /// 이펙트 숨김 (SpriteRenderer 비활성화)
    /// </summary>
    private void HideEffect()
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = false;
            }
        }

        if (showDebugMessages)
            Debug.Log($"[SlashEffect] 이펙트 숨겨짐");
    }

    /// <summary>
    /// 외부에서 강제로 이펙트 숨김 (PlayerController 등에서 호출 가능)
    /// </summary>
    public void ForceHide()
    {
        HideEffect();
        hideTimer = -1f;
        isPlaying = false;
    }

    /// <summary>
    /// 외부에서 이펙트 재생 트리거 (타이머 기반)
    /// Animator를 사용하지 않고 직접 제어할 때 사용
    /// </summary>
    public void PlayEffect()
    {
        ShowEffect();
        hideTimer = autoHideDelay;
        isPlaying = true;
        useAnimatorTiming = false; // 외부 호출 시에는 타이머 기반으로 전환

        if (showDebugMessages)
            Debug.Log($"[SlashEffect] PlayEffect() 호출됨 - 타이머 기반 ({autoHideDelay}초)");
    }

    /// <summary>
    /// Animator 기반 자동 제어로 전환 (기본 모드)
    /// </summary>
    public void EnableAnimatorTiming()
    {
        useAnimatorTiming = true;

        if (showDebugMessages)
            Debug.Log($"[SlashEffect] Animator 기반 타이밍 활성화");
    }
}
