using System.Collections;
using UnityEngine;

/// <summary>
/// Boss 깨어나기 연출
/// 씬 시작 시 잠들어있다가 일어나는 애니메이션 재생
/// </summary>
public class BossWakeUp : MonoBehaviour
{
    [Header("Wake Up Settings")]
    [Tooltip("깨어나기 애니메이션 재생 시간 (초)")]
    [SerializeField] private float wakeUpDuration = 2.0f;

    [Tooltip("씬 시작 후 깨어나기까지 대기 시간 (초)")]
    [SerializeField] private float delayBeforeWakeUp = 0.5f;

    [Header("References")]
    private Animator anim;
    private EnemyAI enemyAI;
    private bool hasWokenUp = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();

        if (anim == null)
            Debug.LogWarning($"⚠ BossWakeUp ({gameObject.name}): Animator를 찾을 수 없습니다.");
    }

    private void Start()
    {
        Debug.Log($"🌙 BossWakeUp ({gameObject.name}): Start 호출됨 - Boss 잠들어있는 상태");

        // Boss AI 비활성화 (잠들어있는 동안 움직이지 않음)
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            Debug.Log($"✅ BossWakeUp ({gameObject.name}): Boss AI 비활성화 (잠든 상태)");
        }
        else
        {
            Debug.LogWarning($"⚠️ BossWakeUp ({gameObject.name}): EnemyAI를 찾을 수 없습니다!");
        }

        // 깨어나기 시작
        StartCoroutine(WakeUpSequence());
    }

    /// <summary>
    /// 깨어나기 시퀀스
    /// </summary>
    private IEnumerator WakeUpSequence()
    {
        Debug.Log($"⏰ BossWakeUp ({gameObject.name}): WakeUpSequence 시작 - {delayBeforeWakeUp}초 대기 중...");

        // 잠들어있는 대기 시간
        yield return new WaitForSeconds(delayBeforeWakeUp);

        Debug.Log($"💤 BossWakeUp ({gameObject.name}): 대기 완료 - 이제 깨어나기 시작!");

        // 🎬 깨어나기 애니메이션 트리거
        if (anim != null)
        {
            anim.SetTrigger("WakeUp");
            Debug.Log($"✅ BossWakeUp ({gameObject.name}): WakeUp 트리거 발동! (Animator: {anim.name})");
        }
        else
        {
            Debug.LogError($"❌ BossWakeUp ({gameObject.name}): Animator가 null입니다! WakeUp 애니메이션을 재생할 수 없습니다.");
        }

        Debug.Log($"⏳ BossWakeUp ({gameObject.name}): 깨어나기 애니메이션 재생 중... ({wakeUpDuration}초 대기)");

        // 깨어나기 애니메이션이 재생되는 동안 대기
        yield return new WaitForSeconds(wakeUpDuration);

        Debug.Log($"👁️ BossWakeUp ({gameObject.name}): 깨어나기 애니메이션 완료!");

        // Boss AI 활성화 (이제 움직이기 시작)
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
            Debug.Log($"✅ BossWakeUp ({gameObject.name}): Boss AI 활성화 (깨어남) - 이제 Player를 추적합니다!");
        }
        else
        {
            Debug.LogWarning($"⚠️ BossWakeUp ({gameObject.name}): EnemyAI가 null이어서 활성화할 수 없습니다!");
        }

        hasWokenUp = true;
        Debug.Log($"🎉 BossWakeUp ({gameObject.name}): Boss 완전히 깨어남! (hasWokenUp = true)");
    }

    /// <summary>
    /// Boss가 깨어났는지 확인
    /// </summary>
    public bool HasWokenUp()
    {
        return hasWokenUp;
    }

    /// <summary>
    /// 강제로 즉시 깨우기 (디버그용)
    /// </summary>
    [ContextMenu("Debug: Wake Up Now")]
    public void WakeUpNow()
    {
        StopAllCoroutines();

        if (anim != null)
        {
            anim.SetTrigger("WakeUp");
        }

        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }

        hasWokenUp = true;
        Debug.Log($"✅ BossWakeUp ({gameObject.name}): 즉시 깨어남!");
    }
}
