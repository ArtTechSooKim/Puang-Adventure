using UnityEngine;

/// <summary>
/// 동물들이 자유롭게 돌아다니는 AI
/// Idle과 Walk 상태를 랜덤하게 전환하며, 좌우로만 이동
/// </summary>
public class AnimalWanderer : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("이동 속도")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Tooltip("이동 방향을 바꾸는 최소 시간 (초)")]
    [SerializeField] private float minChangeDirectionTime = 2f;

    [Tooltip("이동 방향을 바꾸는 최대 시간 (초)")]
    [SerializeField] private float maxChangeDirectionTime = 5f;

    [Header("Idle Settings")]
    [Tooltip("Idle 상태 최소 시간 (초)")]
    [SerializeField] private float minIdleTime = 1f;

    [Tooltip("Idle 상태 최대 시간 (초)")]
    [SerializeField] private float maxIdleTime = 3f;

    [Tooltip("Idle 상태로 전환될 확률 (0~1)")]
    [SerializeField] private float idleChance = 0.3f;

    [Header("Animation")]
    [Tooltip("Animator 컴포넌트 (자동으로 찾음)")]
    [SerializeField] private Animator animator;

    [Tooltip("SpriteRenderer 컴포넌트 (자동으로 찾음)")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("FlipX 방향 반전 (원본 스프라이트가 반대 방향인 경우 체크)")]
    [SerializeField] private bool invertFlipX = false;

    [Header("Boundary (Optional)")]
    [Tooltip("이동 범위 제한 사용 여부")]
    [SerializeField] private bool useBoundary = false;

    [Tooltip("왼쪽 경계")]
    [SerializeField] private float leftBoundary = -10f;

    [Tooltip("오른쪽 경계")]
    [SerializeField] private float rightBoundary = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    // State
    private enum AnimalState
    {
        Idle,
        Walking
    }

    private AnimalState currentState = AnimalState.Idle;
    private float stateTimer = 0f;
    private float nextStateChangeTime = 0f;

    // Movement
    private int moveDirection = 1; // 1 = 오른쪽, -1 = 왼쪽
    private Vector3 startPosition;

    private void Awake()
    {
        // 자동으로 컴포넌트 찾기
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"⚠ AnimalWanderer ({gameObject.name}): Animator를 찾을 수 없습니다!");
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"⚠ AnimalWanderer ({gameObject.name}): SpriteRenderer를 찾을 수 없습니다!");
            }
        }

        startPosition = transform.position;

        // 랜덤한 초기 방향 설정
        moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        // 랜덤한 초기 상태 시간 설정
        ChangeState(Random.Range(0f, 1f) < 0.5f ? AnimalState.Idle : AnimalState.Walking);
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        // 상태 전환 체크
        if (stateTimer >= nextStateChangeTime)
        {
            DecideNextState();
        }

        // 현재 상태에 따라 행동
        switch (currentState)
        {
            case AnimalState.Idle:
                // Idle 상태에서는 아무것도 하지 않음
                break;

            case AnimalState.Walking:
                Walk();
                break;
        }

        // 경계 체크 (범위를 벗어나면 방향 전환)
        if (useBoundary)
        {
            CheckBoundaries();
        }
    }

    /// <summary>
    /// 다음 상태 결정
    /// </summary>
    private void DecideNextState()
    {
        // 랜덤하게 Idle 또는 Walk 선택
        if (Random.Range(0f, 1f) < idleChance)
        {
            ChangeState(AnimalState.Idle);
        }
        else
        {
            ChangeState(AnimalState.Walking);

            // Walking 상태로 전환 시 랜덤하게 방향도 바꿀 수 있음
            if (Random.Range(0f, 1f) < 0.5f)
            {
                ChangeDirection();
            }
        }
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    private void ChangeState(AnimalState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case AnimalState.Idle:
                nextStateChangeTime = Random.Range(minIdleTime, maxIdleTime);
                SetAnimationState(false); // Idle 애니메이션
                break;

            case AnimalState.Walking:
                nextStateChangeTime = Random.Range(minChangeDirectionTime, maxChangeDirectionTime);
                SetAnimationState(true); // Walk 애니메이션
                break;
        }
    }

    /// <summary>
    /// 걷기 동작
    /// </summary>
    private void Walk()
    {
        // 좌우로만 이동
        Vector3 movement = new Vector3(moveDirection * moveSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(movement, Space.World);

        // SpriteRenderer flipX 설정 (왼쪽으로 가면 flip)
        if (spriteRenderer != null)
        {
            // invertFlipX가 true면 방향 반전
            spriteRenderer.flipX = invertFlipX ? (moveDirection > 0) : (moveDirection < 0);
        }
    }

    /// <summary>
    /// 방향 전환
    /// </summary>
    private void ChangeDirection()
    {
        moveDirection *= -1;

        // SpriteRenderer flipX 업데이트
        if (spriteRenderer != null)
        {
            // invertFlipX가 true면 방향 반전
            spriteRenderer.flipX = invertFlipX ? (moveDirection > 0) : (moveDirection < 0);
        }
    }

    /// <summary>
    /// 경계 체크 (범위를 벗어나면 방향 전환)
    /// </summary>
    private void CheckBoundaries()
    {
        if (transform.position.x <= leftBoundary && moveDirection < 0)
        {
            // 왼쪽 경계를 넘어가면 오른쪽으로 전환
            moveDirection = 1;
            transform.position = new Vector3(leftBoundary, transform.position.y, transform.position.z);
        }
        else if (transform.position.x >= rightBoundary && moveDirection > 0)
        {
            // 오른쪽 경계를 넘어가면 왼쪽으로 전환
            moveDirection = -1;
            transform.position = new Vector3(rightBoundary, transform.position.y, transform.position.z);
        }
    }

    /// <summary>
    /// 애니메이션 상태 설정
    /// </summary>
    private void SetAnimationState(bool isWalking)
    {
        if (animator != null)
        {
            // "IsWalking" 파라미터가 있다고 가정
            animator.SetBool("IsWalking", isWalking);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 경계 범위 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !useBoundary) return;

        // 경계선 그리기
        Gizmos.color = Color.yellow;

        Vector3 center = transform.position;
        float height = 2f;

        // 왼쪽 경계
        Gizmos.DrawLine(new Vector3(leftBoundary, center.y - height, center.z),
                       new Vector3(leftBoundary, center.y + height, center.z));

        // 오른쪽 경계
        Gizmos.DrawLine(new Vector3(rightBoundary, center.y - height, center.z),
                       new Vector3(rightBoundary, center.y + height, center.z));

        // 이동 방향 표시
        if (Application.isPlaying && currentState == AnimalState.Walking)
        {
            Gizmos.color = Color.green;
            Vector3 arrowStart = transform.position;
            Vector3 arrowEnd = arrowStart + new Vector3(moveDirection * 0.5f, 0f, 0f);
            Gizmos.DrawLine(arrowStart, arrowEnd);

            // 화살표 끝
            Gizmos.DrawSphere(arrowEnd, 0.1f);
        }
    }
#endif
}
