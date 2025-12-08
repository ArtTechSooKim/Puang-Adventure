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

    [Header("Animation")]
    [Tooltip("Idle/Walk 전환을 위한 최소 이동 거리 (프레임당)")]
    public float walkThreshold = 0.01f;

    private Transform player;
    private Animator anim;
    private Vector3 lastPosition;
    private Vector3 lastMoveDirection = Vector3.down; // 마지막 이동 방향 (기본값: 아래)

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        anim = GetComponent<Animator>();
        lastPosition = transform.position;

        if (player == null)
            Debug.LogWarning($"⚠ EnemyAI ({gameObject.name}): Player를 찾을 수 없습니다!");
        if (anim == null)
            Debug.LogWarning($"⚠ EnemyAI ({gameObject.name}): Animator를 찾을 수 없습니다. 애니메이션이 재생되지 않습니다.");
    }

    void Update()
    {
        if (player == null) return;

        // 이동 전 위치 저장
        Vector3 previousPosition = transform.position;

        // Player를 향해 이동
        transform.position = Vector2.MoveTowards(
            transform.position, player.position, speed * Time.deltaTime);

        // 실제 이동 방향 계산
        Vector3 moveDirection = transform.position - previousPosition;
        bool isMoving = moveDirection.magnitude > walkThreshold;

        // 애니메이션 업데이트
        if (anim != null)
        {
            // IsWalking Bool 설정
            anim.SetBool("IsWalking", isMoving);

            if (isMoving)
            {
                // 움직일 때: 실제 이동 방향 저장 및 전달
                lastMoveDirection = moveDirection.normalized;
                anim.SetFloat("MoveX", lastMoveDirection.x);
                anim.SetFloat("MoveY", lastMoveDirection.y);
            }
            else
            {
                // 멈췄을 때: 마지막 방향 유지
                anim.SetFloat("MoveX", lastMoveDirection.x);
                anim.SetFloat("MoveY", lastMoveDirection.y);
            }
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
}