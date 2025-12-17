using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Audio;
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int scoreValue = 1; // 죽였을 때 올릴 점수

    [Header("Invincibility Settings")]
    [Tooltip("체크하면 이 적은 절대 죽지 않습니다")]
    [SerializeField] private bool isInvincible = false;

    [Header("Death Animation")]
    [SerializeField] private float deathAnimationDuration = 1.0f; // 사망 애니메이션 길이

    private int currentHealth;
    private bool isDead = false; // 사망 상태 추적
    private Animator anim;
    private EnemyAI enemyAI; // AI 비활성화용

    // Event for tutorial or other systems to listen to
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();

        if (anim == null)
            Debug.LogWarning($"⚠ EnemyHealth ({gameObject.name}): Animator를 찾을 수 없습니다. 사망 애니메이션이 재생되지 않습니다.");
    }

    public void TakeDamage(int amount)
    {
        // 이미 죽었으면 추가 데미지 무시
        if (isDead) return;

        // 무적이면 데미지 무시
        if (isInvincible) return;

        if (amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0) Die();
    }

    /// <summary>
    /// 무적 상태 설정
    /// </summary>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    /// <summary>
    /// 최대 체력 설정
    /// </summary>
    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
    }

    private void Die()
    {
        if (isDead) return; // 중복 호출 방지
        isDead = true;

        Debug.Log($"💀 EnemyHealth: Enemy died: {name}");

        // 🔊 사망 사운드 재생 (Boss인지 일반 Enemy인지 구분)
        bool isBoss = GetComponent<BossWakeUp>() != null || GetComponent<BossAttack>() != null;
        if (isBoss)
        {
            AudioManager.I?.PlayBossDeathSound(transform.position);
        }
        else
        {
            AudioManager.I?.PlayEnemyDeathSound(transform.position);
        }

        // 🎬 사망 애니메이션 재생
        if (anim != null)
        {
            // Animator Controller 확인
            if (anim.runtimeAnimatorController == null)
            {
                Debug.LogError($"❌ EnemyHealth ({gameObject.name}): Animator Controller가 할당되지 않았습니다!");
            }
            else
            {
                // 현재 Animator 상태 출력
                AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"📊 EnemyHealth ({gameObject.name}): Current Animator State: {currentState.fullPathHash}");

                // Dead 트리거 발동
                anim.SetTrigger("Dead");
                Debug.Log($"✅ EnemyHealth ({gameObject.name}): 사망 애니메이션 트리거 'Dead' 발동");

                // 트리거가 실제로 설정되었는지 확인
                foreach (var param in anim.parameters)
                {
                    if (param.name == "Dead")
                    {
                        Debug.Log($"✅ EnemyHealth ({gameObject.name}): 'Dead' 트리거 파라미터 존재 확인");
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"❌ EnemyHealth ({gameObject.name}): Animator 컴포넌트가 없습니다!");
        }

        // 🚫 AI 비활성화 (더 이상 움직이지 않음)
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        // Rigidbody2D 정지 (물리 시뮬레이션 중단)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // 움직임 완전 정지
        }

        // Collider 비활성화 (더 이상 충돌하지 않음)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // OnDeath 이벤트 호출
        if (OnDeath != null)
        {
            Debug.Log($"🔔 EnemyHealth: OnDeath has {OnDeath.GetInvocationList().Length} subscriber(s). Invoking...");
            OnDeath.Invoke();
        }
        else
        {
            Debug.LogWarning($"⚠️ EnemyHealth: OnDeath event has no subscribers for {name}!");
        }

        // GameManager에 점수 추가
        GameManager.I?.OnEnemyKilled(scoreValue);

        // 사망 애니메이션 재생 후 오브젝트 파괴
        StartCoroutine(DestroyAfterAnimation());
    }

    /// <summary>
    /// 사망 애니메이션 재생 후 오브젝트 파괴
    /// </summary>
    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        Destroy(gameObject);
    }
}