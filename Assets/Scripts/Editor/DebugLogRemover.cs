using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// 빌드 시 자동으로 Debug.Log를 비활성화하는 에디터 스크립트
/// </summary>
public class DebugLogRemover : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("🔇 DebugLogRemover: Disabling Debug.Log for build...");

        // 빌드 타입에 따라 로그 비활성화
        if (report.summary.options.HasFlag(BuildOptions.Development))
        {
            Debug.Log("✅ Development Build - Debug logs will be enabled");
        }
        else
        {
            Debug.Log("✅ Release Build - Debug logs will be disabled");
            DisableAllDebugLogs();
        }
    }

    private void DisableAllDebugLogs()
    {
        // Unity 내장 기능으로 로그 비활성화
        Debug.unityLogger.logEnabled = false;

        Debug.Log("🔇 All Debug logs have been disabled for this build");
    }
}

/// <summary>
/// 에디터 메뉴에 Debug Log 제어 기능 추가
/// </summary>
public class DebugLogControlMenu
{
    [MenuItem("Tools/Debug Logs/Disable All Debug Logs")]
    public static void DisableDebugLogs()
    {
        Debug.unityLogger.logEnabled = false;
        PlayerPrefs.SetInt("DebugLogsEnabled", 0);
        PlayerPrefs.Save();
        Debug.Log("🔇 Debug logs disabled");
    }

    [MenuItem("Tools/Debug Logs/Enable All Debug Logs")]
    public static void EnableDebugLogs()
    {
        Debug.unityLogger.logEnabled = true;
        PlayerPrefs.SetInt("DebugLogsEnabled", 1);
        PlayerPrefs.Save();
        Debug.Log("✅ Debug logs enabled");
    }

    [MenuItem("Tools/Debug Logs/Toggle Debug Logs")]
    public static void ToggleDebugLogs()
    {
        Debug.unityLogger.logEnabled = !Debug.unityLogger.logEnabled;
        PlayerPrefs.SetInt("DebugLogsEnabled", Debug.unityLogger.logEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log(Debug.unityLogger.logEnabled ? "✅ Debug logs enabled" : "🔇 Debug logs disabled");
    }
}
#endif
