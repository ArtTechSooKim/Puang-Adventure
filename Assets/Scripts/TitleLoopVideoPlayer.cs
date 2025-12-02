using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면에서 배경으로 무한 반복 재생되는 비디오 플레이어입니다.
/// 00_TitleScene에서 사용
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class TitleLoopVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("루프 재생할 비디오 클립")]
    [SerializeField] private VideoClip videoClip;

    [Tooltip("비디오 재생 속도 (1.0 = 정상 속도)")]
    [SerializeField] private float playbackSpeed = 1f;

    [Header("Rendering")]
    [Tooltip("렌더링 방식 선택")]
    [SerializeField] private RenderMode renderMode = RenderMode.CameraNearPlane;

    [Tooltip("RenderTexture 방식 사용 시: RawImage 컴포넌트 (자동 생성 가능)")]
    [SerializeField] private RawImage targetRawImage;

    [Tooltip("RenderTexture 방식 사용 시: Sorting Layer 이름")]
    [SerializeField] private string sortingLayerName = "Default";

    [Tooltip("RenderTexture 방식 사용 시: Sorting Order")]
    [SerializeField] private int sortingOrder = -10;

    [Header("Audio Settings")]
    [Tooltip("비디오 오디오 활성화")]
    [SerializeField] private bool enableAudio = true;

    [Tooltip("비디오 오디오 볼륨 (0-1)")]
    [SerializeField] private float volume = 0.3f;

    [Header("Advanced")]
    [Tooltip("비디오 준비 완료 전까지 대기")]
    [SerializeField] private bool waitUntilReady = true;

    [Tooltip("비디오 페이드 인 시간 (초)")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Header("UI Fade In")]
    [Tooltip("비디오 시작 후 페이드인할 UI CanvasGroup (Play, Load, Save 버튼 등)")]
    [SerializeField] private CanvasGroup titleUICanvasGroup;

    [Tooltip("비디오 시작 후 UI 페이드인까지 대기 시간 (초)")]
    [SerializeField] private float uiFadeInDelay = 2f;

    [Tooltip("UI 페이드인 지속 시간 (초)")]
    [SerializeField] private float uiFadeInDuration = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    public enum RenderMode
    {
        CameraNearPlane,    // 카메라에 직접 렌더링 (Sorting Layer 불가)
        RenderTexture       // RenderTexture + Canvas (Sorting Layer 가능)
    }

    private VideoPlayer videoPlayer;
    private bool isReady = false;
    private CanvasGroup canvasGroup;
    private RenderTexture renderTexture;
    private Canvas videoCanvas;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        SetupRenderMode();
        SetupVideoPlayer();
    }

    private void Start()
    {
        if (showDebugMessages)
            Debug.Log("🎬 TitleLoopVideoPlayer: Initializing looping background video...");

        // 페이드 인 효과를 위한 CanvasGroup 확인
        canvasGroup = GetComponent<CanvasGroup>();

        // UI CanvasGroup 초기화 (투명하게 시작)
        if (titleUICanvasGroup != null)
        {
            titleUICanvasGroup.alpha = 0f;
            if (showDebugMessages)
                Debug.Log("✅ TitleLoopVideoPlayer: Title UI set to invisible (will fade in after video)");
        }

        PrepareAndPlayVideo();
    }

    private void SetupRenderMode()
    {
        if (renderMode == RenderMode.RenderTexture)
        {
            // RenderTexture 생성
            renderTexture = new RenderTexture(1920, 1080, 0);
            renderTexture.name = "TitleVideoRenderTexture";

            // Canvas 자동 생성 (없으면)
            if (targetRawImage == null)
            {
                CreateVideoCanvas();
            }
            else
            {
                // 기존 RawImage 사용 시 Canvas 설정
                videoCanvas = targetRawImage.GetComponentInParent<Canvas>();
                if (videoCanvas != null)
                {
                    SetupCanvas(videoCanvas);
                }
            }

            // RawImage에 RenderTexture 할당
            if (targetRawImage != null)
            {
                targetRawImage.texture = renderTexture;
            }
        }
    }

    private void CreateVideoCanvas()
    {
        // Canvas GameObject 생성
        GameObject canvasObj = new GameObject("VideoCanvas");
        canvasObj.transform.SetParent(transform);

        videoCanvas = canvasObj.AddComponent<Canvas>();
        SetupCanvas(videoCanvas);

        // CanvasScaler 추가
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // GraphicRaycaster 추가
        canvasObj.AddComponent<GraphicRaycaster>();

        // RawImage GameObject 생성
        GameObject rawImageObj = new GameObject("VideoRawImage");
        rawImageObj.transform.SetParent(canvasObj.transform, false);

        targetRawImage = rawImageObj.AddComponent<RawImage>();
        targetRawImage.texture = renderTexture;

        // RectTransform 설정 - 전체화면
        RectTransform rectTransform = rawImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        if (showDebugMessages)
            Debug.Log("✅ TitleLoopVideoPlayer: Auto-created Canvas and RawImage for video");
    }

    private void SetupCanvas(Canvas canvas)
    {
        canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;

        // Main Camera 찾기
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = 1f;
        }

        canvas.sortingLayerName = sortingLayerName;
        canvas.sortingOrder = sortingOrder;

        if (showDebugMessages)
            Debug.Log($"✅ TitleLoopVideoPlayer: Canvas setup - Layer: {sortingLayerName}, Order: {sortingOrder}");
    }

    private void SetupVideoPlayer()
    {
        // VideoPlayer 기본 설정
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true; // 루프 재생
        videoPlayer.playbackSpeed = playbackSpeed;

        // 렌더링 모드 설정
        if (renderMode == RenderMode.RenderTexture && renderTexture != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
        }
        else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.aspectRatio = VideoAspectRatio.FitVertically;
        }

        // 오디오 설정
        if (enableAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            if (videoPlayer.audioOutputMode == VideoAudioOutputMode.Direct)
            {
                videoPlayer.SetDirectAudioVolume(0, volume);
            }
        }
        else
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        // 비디오 클립 설정
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        else
        {
            Debug.LogError("❌ TitleLoopVideoPlayer: Video clip is not assigned!");
        }

        // 이벤트 등록
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started += OnVideoStarted;
    }

    private void PrepareAndPlayVideo()
    {
        if (videoClip == null)
        {
            Debug.LogError("❌ TitleLoopVideoPlayer: Cannot play - no video clip assigned!");
            return;
        }

        if (waitUntilReady)
        {
            // 비디오 준비 후 재생
            videoPlayer.Prepare();

            if (showDebugMessages)
                Debug.Log("⏳ Preparing video...");
        }
        else
        {
            // 즉시 재생 시작
            videoPlayer.Play();

            if (showDebugMessages)
                Debug.Log($"▶ Playing video immediately: {videoClip.name}");
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        isReady = true;

        if (showDebugMessages)
            Debug.Log($"✅ Video prepared: {videoClip.name}");

        // 준비 완료 후 재생 시작
        vp.Play();

        // 페이드 인 효과
        if (fadeInDuration > 0 && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void OnVideoStarted(VideoPlayer vp)
    {
        if (showDebugMessages)
            Debug.Log($"▶ Video started playing in loop mode");

        // 비디오 시작 후 UI 페이드인 시작
        if (titleUICanvasGroup != null)
        {
            StartCoroutine(FadeInTitleUI());
        }
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"❌ TitleLoopVideoPlayer: Video error - {message}");
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeInTitleUI()
    {
        if (titleUICanvasGroup == null) yield break;

        // 지정된 시간만큼 대기
        if (showDebugMessages)
            Debug.Log($"⏳ Waiting {uiFadeInDelay}s before fading in title UI...");

        yield return new WaitForSeconds(uiFadeInDelay);

        if (showDebugMessages)
            Debug.Log($"✨ Fading in title UI over {uiFadeInDuration}s...");

        // UI 페이드인
        titleUICanvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < uiFadeInDuration)
        {
            elapsed += Time.deltaTime;
            titleUICanvasGroup.alpha = Mathf.Clamp01(elapsed / uiFadeInDuration);
            yield return null;
        }

        titleUICanvasGroup.alpha = 1f;

        if (showDebugMessages)
            Debug.Log("✅ Title UI fade in completed!");
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.started -= OnVideoStarted;
        }

        // RenderTexture 정리
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    // 공개 메서드 - 외부에서 제어 가능
    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    public void ResumeVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (videoPlayer != null && enableAudio)
        {
            videoPlayer.SetDirectAudioVolume(0, volume);
        }
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Max(0.1f, speed);
        if (videoPlayer != null)
        {
            videoPlayer.playbackSpeed = playbackSpeed;
        }
    }

    // 디버그 메서드
    [ContextMenu("Debug: Restart Video")]
    private void DebugRestartVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
        }
    }

    [ContextMenu("Debug: Toggle Pause")]
    private void DebugTogglePause()
    {
        if (videoPlayer != null)
        {
            if (videoPlayer.isPlaying)
                PauseVideo();
            else
                ResumeVideo();
        }
    }
}
