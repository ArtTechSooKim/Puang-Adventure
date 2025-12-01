using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 시네마틱 영상을 전체화면으로 재생하고, 재생 완료 후 다음 씬으로 자동 전환합니다.
/// 05_PeuangSadScene에서 사용
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class CinematicVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("재생할 비디오 클립")]
    [SerializeField] private VideoClip videoClip;

    [Header("Rendering")]
    [Tooltip("렌더링 방식 선택")]
    [SerializeField] private RenderMode renderMode = RenderMode.CameraNearPlane;

    [Tooltip("RenderTexture 방식 사용 시: RawImage 컴포넌트 (자동 생성 가능)")]
    [SerializeField] private RawImage targetRawImage;

    [Tooltip("RenderTexture 방식 사용 시: Sorting Layer 이름")]
    [SerializeField] private string sortingLayerName = "Default";

    [Tooltip("RenderTexture 방식 사용 시: Sorting Order")]
    [SerializeField] private int sortingOrder = 100;

    [Header("Scene Transition")]
    [Tooltip("영상 재생 후 이동할 씬 이름")]
    [SerializeField] private string nextSceneName = "06_UnkillableBossScene";

    [Tooltip("영상 종료 후 씬 전환까지의 대기 시간 (초)")]
    [SerializeField] private float transitionDelay = 1f;

    [Header("Skip Settings")]
    [Tooltip("스페이스바로 영상 스킵 허용")]
    [SerializeField] private bool allowSkip = true;

    [Tooltip("영상 스킵 가능 시작 시간 (초) - 처음 N초는 스킵 불가")]
    [SerializeField] private float skipAvailableAfter = 2f;

    [Header("Audio Settings")]
    [Tooltip("비디오 오디오 볼륨 (0-1)")]
    [SerializeField] private float volume = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    public enum RenderMode
    {
        CameraNearPlane,    // 카메라에 직접 렌더링 (Sorting Layer 불가)
        RenderTexture       // RenderTexture + Canvas (Sorting Layer 가능)
    }

    private VideoPlayer videoPlayer;
    private bool isPlaying = false;
    private bool hasEnded = false;
    private float playbackStartTime = 0f;
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
            Debug.Log("🎬 CinematicVideoPlayer: Starting cinematic video...");

        PlayVideo();
    }

    private void SetupRenderMode()
    {
        if (renderMode == RenderMode.RenderTexture)
        {
            // RenderTexture 생성
            renderTexture = new RenderTexture(1920, 1080, 0);
            renderTexture.name = "CinematicVideoRenderTexture";

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

        // GraphicRaycaster 추가 (선택사항)
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
            Debug.Log("✅ CinematicVideoPlayer: Auto-created Canvas and RawImage for video");
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
            Debug.Log($"✅ CinematicVideoPlayer: Canvas setup - Layer: {sortingLayerName}, Order: {sortingOrder}");
    }

    private void SetupVideoPlayer()
    {
        // VideoPlayer 설정
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

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
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        if (videoPlayer.audioOutputMode == VideoAudioOutputMode.Direct)
        {
            videoPlayer.SetDirectAudioVolume(0, volume);
        }

        // 비디오 클립 설정
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        else
        {
            Debug.LogError("❌ CinematicVideoPlayer: Video clip is not assigned!");
        }

        // 이벤트 등록
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void PlayVideo()
    {
        if (videoClip == null)
        {
            Debug.LogError("❌ CinematicVideoPlayer: Cannot play - no video clip assigned!");
            LoadNextScene();
            return;
        }

        videoPlayer.Play();
        isPlaying = true;
        playbackStartTime = Time.time;

        if (showDebugMessages)
            Debug.Log($"▶ Playing video: {videoClip.name}");
    }

    private void Update()
    {
        // 스킵 처리
        if (allowSkip && isPlaying && !hasEnded)
        {
            float elapsedTime = Time.time - playbackStartTime;

            if (elapsedTime >= skipAvailableAfter && Input.GetKeyDown(KeyCode.Space))
            {
                if (showDebugMessages)
                    Debug.Log("⏭ Video skipped by user");

                SkipVideo();
            }
        }
    }

    private void SkipVideo()
    {
        if (hasEnded) return;

        hasEnded = true;
        isPlaying = false;
        videoPlayer.Stop();

        LoadNextScene();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (hasEnded) return;

        hasEnded = true;
        isPlaying = false;

        if (showDebugMessages)
            Debug.Log("✅ CinematicVideoPlayer: Video playback completed");

        StartCoroutine(TransitionToNextScene());
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"❌ CinematicVideoPlayer: Video error - {message}");
        LoadNextScene(); // 에러 발생 시에도 다음 씬으로 진행
    }

    private IEnumerator TransitionToNextScene()
    {
        if (transitionDelay > 0)
        {
            yield return new WaitForSeconds(transitionDelay);
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("❌ CinematicVideoPlayer: Next scene name is not set!");
            return;
        }

        if (showDebugMessages)
            Debug.Log($"🌀 CinematicVideoPlayer: Loading next scene - {nextSceneName}");

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.errorReceived -= OnVideoError;
        }

        // RenderTexture 정리
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    // 디버그 메서드
    [ContextMenu("Debug: Skip to Next Scene")]
    private void DebugSkipToNextScene()
    {
        SkipVideo();
    }
}
