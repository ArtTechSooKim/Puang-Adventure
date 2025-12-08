using System.Collections;
using UnityEngine;

/// <summary>
/// Boss 전용 공격 시스템
/// Player와 충돌 시 공격 모션 재생 및 데미지
/// </summary>
public class BossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackCooldown = 2.0f; // Boss 공격 쿨다운

    [Header("Attack Animation")]
    [SerializeField] private float attackDuration = 0.5f; // 공격 애니메이션 지속 시간

    private float lastAttackTime = -999f;
    private Animator anim;
    private EnemyAI enemyAI;
    private BossWakeUp bossWakeUp; // Boss 깨어남 상태 확인용
    private bool isAttacking = false; // 공격 중인지 여부

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        bossWakeUp = GetComponent<BossWakeUp>(); // BossWakeUp 스크립트 가져오기 (없으면 null)

        if (anim == null)
            Debug.LogWarning($"⚠ BossAttack ({gameObject.name}): Animator를 찾을 수 없습니다.");
        if (enemyAI == null)
            Debug.LogWarning($"⚠ BossAttack ({gameObject.name}): EnemyAI를 찾을 수 없습니다.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"🔵 BossAttack ({gameObject.name}): OnCollisionEnter2D - 충돌 감지됨! Target: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        TryAttack(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🔵 BossAttack ({gameObject.name}): OnTriggerEnter2D - 트리거 감지됨! Target: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        TryAttack(other.gameObject);
    }

    private void TryAttack(GameObject target)
    {
        Debug.Log($"🎯 BossAttack ({gameObject.name}): TryAttack 호출됨! Target: {target.name}");

        // Boss가 아직 깨어나지 않았으면 공격하지 않음
        if (bossWakeUp != null && !bossWakeUp.HasWokenUp())
        {
            Debug.Log($"😴 BossAttack ({gameObject.name}): Boss가 아직 깨어나지 않아서 공격하지 않음");
            return;
        }

        // 쿨다운 체크
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log($"⏱️ BossAttack ({gameObject.name}): 쿨다운 중! (남은 시간: {(lastAttackTime + attackCooldown - Time.time):F2}초)");
            return;
        }

        // 공격 중이면 패스
        if (isAttacking)
        {
            Debug.Log($"⚔️ BossAttack ({gameObject.name}): 이미 공격 중!");
            return;
        }

        // Player 태그 체크
        if (!target.CompareTag("Player"))
        {
            Debug.Log($"❌ BossAttack ({gameObject.name}): Player 태그가 아님! (Tag: {target.tag})");
            return;
        }

        Debug.Log($"✅ BossAttack ({gameObject.name}): 공격 조건 통과! 공격 시작");
        StartCoroutine(PerformAttack(target));
    }

    private IEnumerator PerformAttack(GameObject target)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Debug.Log($"⚔️ BossAttack ({gameObject.name}): 공격 시작!");

        // 🎬 공격 애니메이션 재생
        if (anim != null && enemyAI != null)
        {
            anim.SetTrigger("Attack");

            // 공격 방향을 4방향으로 스냅 (자연스러운 전환)
            Vector2 snappedDir = enemyAI.SnapToFourDirection(enemyAI.GetLastMoveDirection());
            anim.SetFloat("MoveX", snappedDir.x);
            anim.SetFloat("MoveY", snappedDir.y);

            Debug.Log($"✅ BossAttack ({gameObject.name}): 공격 애니메이션 트리거 발동! 방향: ({snappedDir.x}, {snappedDir.y})");
        }
        else
        {
            Debug.LogWarning($"⚠️ BossAttack ({gameObject.name}): 애니메이션 재생 실패! anim: {(anim != null)}, enemyAI: {(enemyAI != null)}");
        }

        // 💥 Player에게 데미지 적용
        var ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            Debug.Log($"💥 BossAttack ({gameObject.name}): Player에게 {damage} 데미지 적용!");
        }
        else
        {
            Debug.LogWarning($"⚠️ BossAttack ({gameObject.name}): PlayerHealth를 찾을 수 없습니다.");
        }

        // 공격 지속 시간 대기 (애니메이션 재생 시간)
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        Debug.Log($"✅ BossAttack ({gameObject.name}): 공격 종료");
    }
}
