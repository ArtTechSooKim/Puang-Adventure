using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// BGM (Background Music) 전용 컨트롤러.
/// DontDestroyOnLoad로 씬 전환 시에도 지속됩니다.
/// 크로스페이드(Crossfade)와 페이드 인/아웃 기능 제공.
/// </summary>
public class BGMController : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("메인 BGM AudioSource")]
    [SerializeField] private AudioSource bgmSource1;

    [Tooltip("크로스페이드용 보조 AudioSource")]
    [SerializeField] private AudioSource bgmSource2;

    [Header("Audio Mixer")]
    [Tooltip("BGM용 AudioMixer Group")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Header("Settings")]
    [Tooltip("기본 페이드 시간 (초)")]
    [SerializeField] private float defaultFadeTime = 1.5f;

    [Tooltip("BGM 기본 볼륨 (0~1)")]
    [SerializeField] private float defaultVolume = 0.75f;

    [Tooltip("다이얼로그 중 BGM 감소 비율 (0~1)")]
    [SerializeField] private float duckVolumeMultiplier = 0.3f;

    // 현재 재생 중인 AudioSource
    private AudioSource currentSource;
    private AudioSource nextSource;

    // 볼륨 제어
    private float currentVolume = 1f;
    private float targetVolume = 1f;
    private bool isDucking = false;

    // 코루틴 참조
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        InitializeAudioSources();
        currentSource = bgmSource1;
        nextSource = bgmSource2;
        currentVolume = defaultVolume;
        targetVolume = defaultVolume;
    }

    /// <summary>
    /// AudioSource 초기화
    /// </summary>
    private void InitializeAudioSources()
    {
        if (bgmSource1 == null)
        {
            bgmSource1 = gameObject.AddComponent<AudioSource>();
            bgmSource1.playOnAwake = false;
            bgmSource1.loop = true;
        }

        if (bgmSource2 == null)
        {
            bgmSource2 = gameObject.AddComponent<AudioSource>();
            bgmSource2.playOnAwake = false;
            bgmSource2.loop = true;
        }

        // AudioMixer 연결
        if (bgmMixerGroup != null)
        {
            bgmSource1.outputAudioMixerGroup = bgmMixerGroup;
            bgmSource2.outputAudioMixerGroup = bgmMixerGroup;
        }

        // 초기 볼륨 설정
        bgmSource1.volume = 0f;
        bgmSource2.volume = 0f;
    }

    /// <summary>
    /// AudioMixerGroup 설정 (AudioManager에서 호출)
    /// </summary>
    public void SetMixerGroup(AudioMixerGroup mixerGroup)
    {
        bgmMixerGroup = mixerGroup;

        if (bgmSource1 != null)
        {
            bgmSource1.outputAudioMixerGroup = bgmMixerGroup;
        }

        if (bgmSource2 != null)
        {
            bgmSource2.outputAudioMixerGroup = bgmMixerGroup;
        }

        Debug.Log($"✅ BGMController: MixerGroup set to {mixerGroup?.name ?? "null"}");
    }

    /// <summary>
    /// BGM 재생 (페이드 인)
    /// </summary>
    public void PlayBGM(AudioClip clip, float fadeTime = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ BGMController: AudioClip is null!");
            return;
        }

        // 이미 같은 곡이 재생 중이면 무시
        if (currentSource.clip == clip && currentSource.isPlaying)
        {
            return;
        }

        if (fadeTime < 0f) fadeTime = defaultFadeTime;

        StopAllCoroutines();
        StartCoroutine(CrossfadeBGM(clip, fadeTime));
    }

    /// <summary>
    /// BGM 크로스페이드 (이전 BGM 페이드 아웃 + 새 BGM 페이드 인)
    /// </summary>
    private IEnumerator CrossfadeBGM(AudioClip newClip, float fadeTime)
    {
        // 다음 소스에 새로운 클립 설정
        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime; // UI 일시정지에도 작동
            float progress = elapsed / fadeTime;

            // 이전 BGM 페이드 아웃
            currentSource.volume = Mathf.Lerp(currentVolume, 0f, progress);

            // 새 BGM 페이드 인
            nextSource.volume = Mathf.Lerp(0f, targetVolume, progress);

            yield return null;
        }

        // 최종 볼륨 설정
        currentSource.volume = 0f;
        currentSource.Stop();
        nextSource.volume = targetVolume;

        // 소스 스왑
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        currentVolume = targetVolume;
    }

    /// <summary>
    /// BGM 정지 (페이드 아웃)
    /// </summary>
    public void StopBGM(float fadeTime = -1f)
    {
        if (fadeTime < 0f) fadeTime = defaultFadeTime;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOut(fadeTime));
    }

    /// <summary>
    /// 페이드 아웃 코루틴
    /// </summary>
    private IEnumerator FadeOut(float fadeTime)
    {
        float startVolume = currentSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            currentSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        currentSource.volume = 0f;
        currentSource.Stop();
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        currentSource.Pause();
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        currentSource.UnPause();
    }

    /// <summary>
    /// BGM 볼륨 덕킹 (다이얼로그 중 볼륨 감소)
    /// </summary>
    public void DuckBGM(float duckTime = 0.5f)
    {
        if (isDucking) return;

        isDucking = true;
        targetVolume = defaultVolume * duckVolumeMultiplier;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToVolume(targetVolume, duckTime));
    }

    /// <summary>
    /// BGM 볼륨 복구 (다이얼로그 종료 후)
    /// </summary>
    public void RestoreBGM(float restoreTime = 0.5f)
    {
        if (!isDucking) return;

        isDucking = false;
        targetVolume = defaultVolume;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToVolume(targetVolume, restoreTime));
    }

    /// <summary>
    /// 특정 볼륨으로 페이드
    /// </summary>
    private IEnumerator FadeToVolume(float target, float fadeTime)
    {
        float startVolume = currentSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            currentSource.volume = Mathf.Lerp(startVolume, target, elapsed / fadeTime);
            currentVolume = currentSource.volume;
            yield return null;
        }

        currentSource.volume = target;
        currentVolume = target;
    }

    /// <summary>
    /// 현재 재생 중인 BGM 클립 반환
    /// </summary>
    public AudioClip GetCurrentClip()
    {
        return currentSource.clip;
    }

    /// <summary>
    /// BGM이 재생 중인지 확인
    /// </summary>
    public bool IsPlaying()
    {
        return currentSource.isPlaying;
    }

    /// <summary>
    /// 기본 볼륨 설정 (외부에서 설정 변경 시 호출)
    /// </summary>
    public void SetDefaultVolume(float volume)
    {
        defaultVolume = Mathf.Clamp01(volume);
        if (!isDucking)
        {
            targetVolume = defaultVolume;
        }
    }
}
