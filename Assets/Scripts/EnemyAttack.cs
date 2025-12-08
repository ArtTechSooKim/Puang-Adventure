// ...existing code...
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Attack Animation")]
    [Tooltip("공격 애니메이션 재생 여부")]
    [SerializeField] private bool playAttackAnimation = true;

    private float lastHitTime = -99f;
    private Animator anim;
    private EnemyAI enemyAI;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();

        if (playAttackAnimation && anim == null)
            Debug.LogWarning($"⚠ EnemyAttack ({gameObject.name}): Animator를 찾을 수 없습니다. 공격 애니메이션이 재생되지 않습니다.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"🔵 EnemyAttack ({gameObject.name}): OnCollisionEnter2D - 충돌 감지됨! Target: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        TryHit(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🔵 EnemyAttack ({gameObject.name}): OnTriggerEnter2D - 트리거 감지됨! Target: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        TryHit(other.gameObject);
    }

    private void TryHit(GameObject target)
    {
        Debug.Log($"🎯 EnemyAttack ({gameObject.name}): TryHit 호출됨! Target: {target.name}");

        // 쿨다운 체크
        if (Time.time < lastHitTime + hitCooldown)
        {
            Debug.Log($"⏱️ EnemyAttack ({gameObject.name}): 쿨다운 중! (남은 시간: {(lastHitTime + hitCooldown - Time.time):F2}초)");
            return;
        }

        // Player 태그 체크
        if (!target.CompareTag("Player"))
        {
            Debug.Log($"❌ EnemyAttack ({gameObject.name}): Player 태그가 아님! (Tag: {target.tag})");
            return;
        }

        Debug.Log($"✅ EnemyAttack ({gameObject.name}): 공격 조건 통과! 데미지 적용 시작");
        lastHitTime = Time.time;

        // 🎬 공격 애니메이션 재생
        if (playAttackAnimation && anim != null && enemyAI != null)
        {
            anim.SetTrigger("Attack");

            // 공격 방향을 4방향으로 스냅 (자연스러운 전환)
            Vector2 snappedDir = enemyAI.SnapToFourDirection(enemyAI.GetLastMoveDirection());
            anim.SetFloat("MoveX", snappedDir.x);
            anim.SetFloat("MoveY", snappedDir.y);

            Debug.Log($"✅ EnemyAttack ({gameObject.name}): 공격 애니메이션 트리거 발동! 방향: ({snappedDir.x}, {snappedDir.y})");
        }
        else
        {
            Debug.LogWarning($"⚠️ EnemyAttack ({gameObject.name}): 애니메이션 재생 실패! playAttackAnimation: {playAttackAnimation}, anim: {(anim != null)}, enemyAI: {(enemyAI != null)}");
        }

        // 플레이어에게 데미지
        var ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            Debug.Log($"💥 EnemyAttack ({gameObject.name}): Player에게 {damage} 데미지 적용!");
        }
        else
        {
            Debug.LogWarning($"⚠️ EnemyAttack ({gameObject.name}): PlayerHealth component not found on Player object.");
        }
    }
}
// ...existing code...