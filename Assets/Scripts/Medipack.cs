using UnityEngine;

/// <summary>
/// Medipack - 체력 회복 아이템
/// Player가 트리거에 닿으면 체력을 회복하고 사라집니다.
/// </summary>
public class Medipack : MonoBehaviour
{
    [Header("Heal Settings")]
    [Tooltip("회복할 체력 양")]
    [SerializeField] private int healAmount = 20;

    [Header("Effects (Optional)")]
    [Tooltip("회복 효과음 (AudioManager 사용 권장)")]
    [SerializeField] private bool playHealSound = true;

    [Tooltip("회복 시 파티클 효과 (선택사항)")]
    [SerializeField] private GameObject healEffectPrefab;

    [Header("Visual Settings")]
    [Tooltip("획득 시 애니메이션 재생 (선택사항)")]
    [SerializeField] private Animator animator;

    [Tooltip("획득 후 사라지기까지 대기 시간 (애니메이션용)")]
    [SerializeField] private float destroyDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = false;

    private bool isUsed = false; // 중복 사용 방지

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player 태그 확인
        if (!other.CompareTag("Player"))
            return;

        // 이미 사용되었으면 무시
        if (isUsed)
            return;

        // Player의 PlayerHealth 컴포넌트 찾기
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning($"⚠ Medipack: Player에 PlayerHealth 컴포넌트가 없습니다!");
            return;
        }

        // 체력이 이미 최대치면 회복 안 함
        if (playerHealth.IsFullHealth())
        {
            if (showDebugMessages)
                Debug.Log($"💊 Medipack: 체력이 이미 최대입니다. 회복하지 않음.");
            return;
        }

        // 사용 처리
        isUsed = true;

        // 체력 회복
        playerHealth.Heal(healAmount);

        if (showDebugMessages)
            Debug.Log($"💊 Medipack: Player 체력 {healAmount} 회복!");

        // 🔊 회복 사운드 재생
        if (playHealSound && AudioManager.I != null)
        {
            AudioManager.I.PlayPlayerHealSound();
        }

        // 🎨 파티클 효과 생성 (선택사항)
        if (healEffectPrefab != null)
        {
            GameObject effect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // 2초 후 파티클 제거
        }

        // 🎬 획득 애니메이션 재생 (선택사항)
        if (animator != null)
        {
            animator.SetTrigger("Pickup");
        }

        // 오브젝트 제거 (약간의 딜레이 후)
        Destroy(gameObject, destroyDelay);
    }

    private void Reset()
    {
        // Inspector에서 생성 시 자동 설정
        // Collider2D가 없으면 자동 추가
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.isTrigger = true;
            circleCol.radius = 0.5f;
            Debug.Log("✅ Medipack: CircleCollider2D (IsTrigger) 자동 추가됨");
        }
        else
        {
            col.isTrigger = true;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Scene 뷰에서 회복 범위 표시
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 초록색 반투명

            if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
            else if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 선택되었을 때 힐량 표시
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, $"💊 Heal: {healAmount}");
    }
#endif
}
