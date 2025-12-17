using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 화면 밝기를 제어하는 컨트롤러
/// Canvas Overlay를 사용하여 검은 이미지의 투명도를 조절하여 밝기를 변경합니다.
/// </summary>
public class BrightnessController : MonoBehaviour
{
    [Header("Brightness Settings")]
    [Tooltip("밝기 조절용 검은 이미지 (Canvas Overlay)")]
    [SerializeField] private Image brightnessOverlay;

    [Tooltip("최소 밝기 (0 = 가장 어두움, 1 = 가장 밝음)")]
    [SerializeField] private float minBrightness = 0.2f;

    [Tooltip("최대 밝기")]
    [SerializeField] private float maxBrightness = 1.0f;

    private static BrightnessController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // BrightnessOverlay가 Canvas의 자식이면 Canvas도 DontDestroyOnLoad 처리
            if (brightnessOverlay != null)
            {
                Canvas parentCanvas = brightnessOverlay.GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    DontDestroyOnLoad(parentCanvas.gameObject);
                }
            }

            InitializeBrightnessOverlay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 밝기 오버레이 초기화
    /// </summary>
    private void InitializeBrightnessOverlay()
    {
        if (brightnessOverlay == null)
        {
            Debug.LogWarning("⚠ BrightnessController: brightnessOverlay가 연결되지 않았습니다! Inspector에서 Image를 연결해주세요.");
        }
        else
        {
            // 초기 밝기를 최대(1.0)로 설정 (화면이 밝은 상태)
            SetBrightness(1.0f);
        }
    }

    /// <summary>
    /// 밝기 설정 (0.0 ~ 1.0)
    /// </summary>
    public void SetBrightness(float brightness)
    {
        brightness = Mathf.Clamp(brightness, minBrightness, maxBrightness);

        if (brightnessOverlay != null)
        {
            float alpha = 1f - brightness;
            Color color = brightnessOverlay.color;
            color.a = alpha;
            brightnessOverlay.color = color;
        }

        RenderSettings.ambientIntensity = Mathf.Lerp(0.5f, 1.5f, brightness);
    }

    /// <summary>
    /// 현재 밝기 가져오기
    /// </summary>
    public float GetBrightness()
    {
        if (brightnessOverlay != null)
        {
            return 1f - brightnessOverlay.color.a;
        }
        return 1f;
    }

    /// <summary>
    /// 정적 메서드: 밝기 설정
    /// </summary>
    public static void SetGlobalBrightness(float brightness)
    {
        if (instance != null)
        {
            instance.SetBrightness(brightness);
        }
    }

    /// <summary>
    /// 정적 메서드: 밝기 가져오기
    /// </summary>
    public static float GetGlobalBrightness()
    {
        if (instance != null)
        {
            return instance.GetBrightness();
        }
        return 1f;
    }
}
