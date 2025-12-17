using UnityEngine;

/// <summary>
/// 게임 시작 시 로그 설정을 제어하는 스크립트
/// 빌드 타입에 따라 자동으로 로그를 활성화/비활성화합니다.
/// </summary>
public class GameLogger : MonoBehaviour
{
    [Header("로그 설정")]
    [Tooltip("에디터에서 로그 활성화")]
    [SerializeField] private bool enableLogsInEditor = true;

    [Tooltip("개발 빌드에서 로그 활성화")]
    [SerializeField] private bool enableLogsInDevelopmentBuild = false;

    [Tooltip("릴리즈 빌드에서 로그 활성화")]
    [SerializeField] private bool enableLogsInReleaseBuild = false;

    private void Awake()
    {
        ConfigureDebugLogs();
    }

    private void ConfigureDebugLogs()
    {
#if UNITY_EDITOR
        // 에디터에서 실행 중
        Debug.unityLogger.logEnabled = enableLogsInEditor;
#elif DEVELOPMENT_BUILD
        // 개발 빌드
        Debug.unityLogger.logEnabled = enableLogsInDevelopmentBuild;
#else
        // 릴리즈 빌드
        Debug.unityLogger.logEnabled = enableLogsInReleaseBuild;
#endif
    }

    /// <summary>
    /// 런타임에서 로그 활성화/비활성화
    /// </summary>
    public static void SetLogsEnabled(bool enabled)
    {
        Debug.unityLogger.logEnabled = enabled;
    }

    /// <summary>
    /// 현재 로그 활성화 상태 확인
    /// </summary>
    public static bool IsLogsEnabled()
    {
        return Debug.unityLogger.logEnabled;
    }
}
