using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// SFX (Sound Effects) 전용 컨트롤러.
/// AudioSource 풀링을 사용하여 효율적으로 효과음을 재생합니다.
/// 2D/3D 오디오 모두 지원.
/// </summary>
public class SFXController : MonoBehaviour
{
    [Header("Audio Mixer")]
    [Tooltip("SFX용 AudioMixer Group")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Pooling Settings")]
    [Tooltip("AudioSource 풀 초기 크기")]
    [SerializeField] private int poolSize = 10;

    [Tooltip("풀이 가득 찰 때 자동으로 확장할지 여부")]
    [SerializeField] private bool autoExpand = true;

    [Header("3D Audio Settings")]
    [Tooltip("3D 사운드 최소 거리")]
    [SerializeField] private float minDistance = 1f;

    [Tooltip("3D 사운드 최대 거리")]
    [SerializeField] private float maxDistance = 50f;

    [Tooltip("3D 사운드 감쇠 곡선")]
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    // AudioSource 풀
    private List<AudioSource> sfxPool = new List<AudioSource>();

    // 디버그 카운터
    private int totalSoundsPlayed = 0;

    private void Awake()
    {
        InitializePool();
    }

    /// <summary>
    /// AudioSource 풀 초기화
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateAudioSource();
        }

        Debug.Log($"✅ SFXController: Initialized pool with {poolSize} AudioSources");
    }

    /// <summary>
    /// 새로운 AudioSource 생성
    /// </summary>
    private AudioSource CreateAudioSource()
    {
        GameObject audioObj = new GameObject($"SFX_AudioSource_{sfxPool.Count}");
        audioObj.transform.SetParent(transform);

        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // AudioMixer 연결
        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        // 3D 오디오 설정
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;

        sfxPool.Add(audioSource);
        return audioSource;
    }

    /// <summary>
    /// AudioMixerGroup 설정 (AudioManager에서 호출)
    /// </summary>
    public void SetMixerGroup(AudioMixerGroup mixerGroup)
    {
        sfxMixerGroup = mixerGroup;

        // 이미 생성된 모든 AudioSource에 MixerGroup 적용
        foreach (AudioSource source in sfxPool)
        {
            if (source != null)
            {
                source.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        Debug.Log($"✅ SFXController: MixerGroup set to {mixerGroup?.name ?? "null"} (applied to {sfxPool.Count} sources)");
    }

    /// <summary>
    /// 사용 가능한 AudioSource 가져오기
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        // 재생 중이지 않은 AudioSource 찾기
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // 모든 AudioSource가 사용 중일 때
        if (autoExpand)
        {
            Debug.LogWarning($"⚠️ SFXController: Pool exhausted, creating new AudioSource (Total: {sfxPool.Count + 1})");
            return CreateAudioSource();
        }
        else
        {
            // 가장 오래된 사운드를 강제 정지하고 재사용
            AudioSource oldest = sfxPool[0];
            oldest.Stop();
            return oldest;
        }
    }

    /// <summary>
    /// 2D 효과음 재생 (UI, 플레이어 액션 등)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ SFXController: AudioClip is null!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        source.transform.position = transform.position; // 부모 위치로 리셋
        source.spatialBlend = 0f; // 2D
        source.volume = Mathf.Clamp01(volume);
        source.pitch = pitch;
        source.PlayOneShot(clip);

        totalSoundsPlayed++;
    }

    /// <summary>
    /// 3D 효과음 재생 (적 사운드, 월드 이벤트 등)
    /// </summary>
    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ SFXController: AudioClip is null!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        source.transform.position = position;
        source.spatialBlend = 1f; // 3D
        source.volume = Mathf.Clamp01(volume);
        source.pitch = pitch;
        source.PlayOneShot(clip);

        totalSoundsPlayed++;
    }

    /// <summary>
    /// 랜덤 피치로 효과음 재생 (반복음 방지)
    /// </summary>
    public void PlaySFXRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f, float volume = 1f)
    {
        float randomPitch = Random.Range(minPitch, maxPitch);
        PlaySFX(clip, volume, randomPitch);
    }

    /// <summary>
    /// 여러 클립 중 하나를 랜덤으로 재생 (발소리 등 변화를 위해)
    /// </summary>
    public void PlaySFXRandom(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("⚠️ SFXController: Clips array is null or empty!");
            return;
        }

        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        PlaySFX(randomClip, volume);
    }

    /// <summary>
    /// 루프 효과음 재생 (주변음 등)
    /// </summary>
    public AudioSource PlayLoopingSFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ SFXController: AudioClip is null!");
            return null;
        }

        AudioSource source = GetAvailableAudioSource();
        source.transform.position = transform.position;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = 1f;
        source.loop = true;
        source.clip = clip;
        source.Play();

        return source; // 나중에 정지할 수 있도록 반환
    }

    /// <summary>
    /// 특정 AudioSource 정지
    /// </summary>
    public void StopLoopingSFX(AudioSource source)
    {
        if (source != null)
        {
            source.loop = false;
            source.Stop();
        }
    }

    /// <summary>
    /// 모든 효과음 정지
    /// </summary>
    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxPool)
        {
            source.Stop();
        }
    }

    /// <summary>
    /// 재생 중인 효과음 개수 확인
    /// </summary>
    public int GetActiveSFXCount()
    {
        int count = 0;
        foreach (AudioSource source in sfxPool)
        {
            if (source.isPlaying)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 통계 출력 (디버그용)
    /// </summary>
    public void LogStatistics()
    {
        int activeSounds = GetActiveSFXCount();
        Debug.Log("=== SFX Controller Statistics ===");
        Debug.Log($"Pool Size: {sfxPool.Count}");
        Debug.Log($"Active Sounds: {activeSounds}");
        Debug.Log($"Total Sounds Played: {totalSoundsPlayed}");
        Debug.Log("==================================");
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Log SFX Statistics")]
    private void DebugLogStatistics()
    {
        LogStatistics();
    }

    [ContextMenu("Debug: Stop All SFX")]
    private void DebugStopAllSFX()
    {
        StopAllSFX();
        Debug.Log("🔇 All SFX stopped");
    }
#endif
}
