using UnityEngine;
using System.Diagnostics;

/// <summary>
/// 전역 디버그 로깅 컨트롤러
/// 빌드 시 모든 로그를 자동으로 제거합니다.
/// </summary>
public static class DebugLogger
{
    // 에디터에서만 로그 활성화, 빌드에서는 비활성화
#if UNITY_EDITOR
    private static bool enableLogs = true;
#else
    private static bool enableLogs = false;
#endif

    /// <summary>
    /// 로그 활성화/비활성화 설정
    /// </summary>
    public static void SetLogsEnabled(bool enabled)
    {
#if UNITY_EDITOR
        enableLogs = enabled;
#endif
    }

    /// <summary>
    /// 일반 로그
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void Log(object message)
    {
        if (enableLogs)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    /// <summary>
    /// 일반 로그 (컨텍스트 포함)
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void Log(object message, Object context)
    {
        if (enableLogs)
        {
            UnityEngine.Debug.Log(message, context);
        }
    }

    /// <summary>
    /// 경고 로그
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message)
    {
        if (enableLogs)
        {
            UnityEngine.Debug.LogWarning(message);
        }
    }

    /// <summary>
    /// 경고 로그 (컨텍스트 포함)
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message, Object context)
    {
        if (enableLogs)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }
    }

    /// <summary>
    /// 에러 로그 (항상 표시)
    /// </summary>
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }

    /// <summary>
    /// 에러 로그 (컨텍스트 포함, 항상 표시)
    /// </summary>
    public static void LogError(object message, Object context)
    {
        UnityEngine.Debug.LogError(message, context);
    }
}
