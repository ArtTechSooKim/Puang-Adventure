using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Audio;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;

    [Header("Detection")]
    [Tooltip("플레이어 인지 거리 (이 거리 안에 들어와야 추적 시작)")]
    public float detectionRange = 5f;

    [Tooltip("추적 해제 거리 (플레이어가 이 거리보다 멀어지면 추적 중지)")]
    public float deactivationRange = 7f;

    [Header("Animation")]
    [Tooltip("Idle/Walk 전환을 위한 최소 이동 거리 (프레임당)")]
    public float walkThreshold = 0.01f;

    [Tooltip("애니메이션 상태 전환 딜레이 (초) - 깜빡임 방지")]
    [SerializeField] private float animationStateDelay = 0.1f;

    [Tooltip("FlipX 방향 반전 (원본 스프라이트가 반대 방향인 경우 체크)")]
    [SerializeField] private bool invertFlipX = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    private Transform player;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector3 lastPosition;
    private Vector3 lastMoveDirection = Vector3.down; // 마지막 이동 방향 (기본값: 아래)
    private bool isChasing = false; // 추적 중인지 여부
    private bool lastIsWalking = false; // 마지막 Walking 상태
    private float lastStateChangeTime = 0f; // 마지막 상태 변경 시간

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;

        if (player == null)
            Debug.LogWarning($"⚠ EnemyAI ({gameObject.name}): Player를 찾을 수 없습니다!");
        if (anim == null)
            Debug.LogWarning($"⚠ EnemyAI ({gameObject.name}): Animator를 찾을 수 없습니다. 애니메이션이 재생되지 않습니다.");
        if (spriteRenderer == null)
            Debug.LogWarning($"⚠ EnemyAI ({gameObject.name}): SpriteRenderer를 찾을 수 없습니다!");
    }

    void Update()
    {
        if (player == null) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 추적 상태 관리 (Hysteresis)
        if (!isChasing && distanceToPlayer <= detectionRange)
        {
            isChasing = true; // 추적 시작
        }
        else if (isChasing && distanceToPlayer > deactivationRange)
        {
            isChasing = false; // 추적 중지
        }

        // 이동 전 위치 저장
        Vector3 previousPosition = transform.position;

        // 추적 중일 때만 Player를 향해 이동
        if (isChasing)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, player.position, speed * Time.deltaTime);
        }

        // 실제 이동 방향 계산
        Vector3 moveDirection = transform.position - previousPosition;
        bool currentIsMoving = moveDirection.magnitude > walkThreshold;

        // 애니메이션 상태 전환 (Walk → Idle만 딜레이, Idle → Walk는 즉시)
        bool shouldUpdateAnimation = false;

        if (currentIsMoving != lastIsWalking)
        {
            // Idle → Walk: 즉시 전환 (반응성 향상)
            if (currentIsMoving && !lastIsWalking)
            {
                lastIsWalking = true;
                lastStateChangeTime = Time.time;
                shouldUpdateAnimation = true;
            }
            // Walk → Idle: 딜레이 후 전환 (깜빡임 방지)
            else if (!currentIsMoving && lastIsWalking)
            {
                if (Time.time - lastStateChangeTime >= animationStateDelay)
                {
                    lastIsWalking = false;
                    lastStateChangeTime = Time.time;
                    shouldUpdateAnimation = true;
                }
            }
        }
        else
        {
            // 상태가 유지되면 타이머 리셋
            lastStateChangeTime = Time.time;
        }

        // 애니메이션 업데이트
        if (anim != null && shouldUpdateAnimation)
        {
            anim.SetBool("IsWalking", lastIsWalking);
        }

        // SpriteRenderer flipX 설정 (좌우 이동)
        if (spriteRenderer != null && currentIsMoving && Mathf.Abs(moveDirection.x) > 0.01f)
        {
            // invertFlipX가 true면 방향 반전
            spriteRenderer.flipX = invertFlipX ? (moveDirection.x > 0) : (moveDirection.x < 0);
        }
    }

    /// <summary>
    /// 방향을 4방향(상/하/좌/우) 중 가장 가까운 방향으로 스냅합니다.
    /// Attack 등 특정 액션에서 사용 가능
    /// </summary>
    public Vector2 SnapToFourDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return Vector2.down; // 기본값

        // 상하좌우 중 가장 큰 성분을 선택
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absY > absX)
        {
            // 상 또는 하
            return direction.y > 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            // 좌 또는 우
            return direction.x > 0 ? Vector2.right : Vector2.left;
        }
    }

    /// <summary>
    /// 현재 이동 방향 반환 (다른 스크립트에서 사용 가능)
    /// </summary>
    public Vector3 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 씬 뷰에서 인지 거리 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // 인지 거리 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 추적 해제 거리 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deactivationRange);

        // 추적 중일 때 플레이어와 연결선 표시
        if (Application.isPlaying && isChasing && player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif
}