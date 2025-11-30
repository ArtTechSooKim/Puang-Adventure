using UnityEngine;

/// <summary>
/// FX 프리팹에 부착하여 일정 시간 후 자동으로 오브젝트를 파괴합니다.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Tooltip("오브젝트가 생성된 후 파괴되기까지의 시간 (초)")]
    [SerializeField] private float destroyDelay = 0.5f;

    private void Start()
    {
        // 지정된 시간 후 자동 파괴
        Destroy(gameObject, destroyDelay);
    }
}
