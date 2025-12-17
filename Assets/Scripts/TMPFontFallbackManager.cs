using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// TextMeshPro 한글 폰트 Fallback 관리자
/// 게임 시작 시 모든 TMP 컴포넌트에 한글 폰트 Fallback을 자동으로 설정합니다.
/// </summary>
public class TMPFontFallbackManager : MonoBehaviour
{
    [Header("Fallback Fonts")]
    [Tooltip("기본 한글 폰트 (Fallback으로 사용)")]
    [SerializeField] private TMP_FontAsset koreanFallbackFont;

    [Header("Settings")]
    [Tooltip("씬 로드 시 자동으로 설정")]
    [SerializeField] private bool autoSetupOnStart = true;

    [Tooltip("동적으로 생성되는 TMP도 자동 설정")]
    [SerializeField] private bool monitorDynamicTMP = true;

    private static TMPFontFallbackManager instance;
    private HashSet<TextMeshProUGUI> processedTexts = new HashSet<TextMeshProUGUI>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAllTMPComponents();
        }
    }

    private void Update()
    {
        if (monitorDynamicTMP)
        {
            CheckForNewTMPComponents();
        }
    }

    /// <summary>
    /// 씬의 모든 TMP 컴포넌트에 Fallback 설정
    /// </summary>
    public void SetupAllTMPComponents()
    {
        if (koreanFallbackFont == null)
        {
            Debug.LogWarning("[TMPFontFallbackManager] koreanFallbackFont가 설정되지 않았습니다!");
            return;
        }

        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);

        int setupCount = 0;
        foreach (var tmp in allTexts)
        {
            if (SetupFallback(tmp))
            {
                setupCount++;
            }
        }
    }

    /// <summary>
    /// 새로 생성된 TMP 컴포넌트 체크
    /// </summary>
    private void CheckForNewTMPComponents()
    {
        if (koreanFallbackFont == null) return;

        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);

        foreach (var tmp in allTexts)
        {
            if (!processedTexts.Contains(tmp))
            {
                SetupFallback(tmp);
            }
        }
    }

    /// <summary>
    /// 개별 TMP에 Fallback 설정
    /// </summary>
    private bool SetupFallback(TextMeshProUGUI tmp)
    {
        if (tmp == null || koreanFallbackFont == null)
            return false;

        if (processedTexts.Contains(tmp))
            return false;

        if (tmp.font != null)
        {
            if (tmp.font.fallbackFontAssetTable == null)
            {
                tmp.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }

            if (!tmp.font.fallbackFontAssetTable.Contains(koreanFallbackFont))
            {
                tmp.font.fallbackFontAssetTable.Add(koreanFallbackFont);
            }
        }

        processedTexts.Add(tmp);
        return true;
    }

    /// <summary>
    /// 특정 TMP 컴포넌트에 강제로 Fallback 설정
    /// </summary>
    public static void ForceSetupFallback(TextMeshProUGUI tmp)
    {
        if (instance != null && tmp != null)
        {
            instance.SetupFallback(tmp);
        }
    }
}
