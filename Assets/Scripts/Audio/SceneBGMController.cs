using UnityEngine;

/// <summary>
/// 특정 씬에만 배치되는 BGM 컨트롤러.
/// AudioManager의 자동 BGM 시스템을 오버라이드하고 싶을 때 사용합니다.
///
/// 사용 예시:
/// - 보스 페이즈별로 다른 BGM 재생
/// - 이벤트 발생 시 일시적으로 BGM 변경
/// - 주변음(Ambient) 루프 재생
///
/// 주의: 이 컴포넌트는 DontDestroyOnLoad가 아니므로 씬 전환 시 파괴됩니다.
/// </summary>
public class SceneBGMController : MonoBehaviour
{
    [Header("Scene-Specific BGM")]
    [Tooltip("이 씬에서 재생할 BGM (비어있으면 AudioManager의 기본 BGM 사용)")]
    [SerializeField] private AudioClip sceneBGM;

    [Tooltip("씬 시작 시 자동으로 BGM 재생")]
    [SerializeField] private bool playOnStart = true;

    [Tooltip("BGM 페이드 인 시간")]
    [SerializeField] private float fadeInTime = 2f;

    [Tooltip("AudioManager의 자동 BGM 시스템 비활성화 (이 씬에서만)")]
    [SerializeField] private bool overrideAudioManager = false;

    [Header("Ambient Sounds")]
    [Tooltip("주변음/환경음 (루프)")]
    [SerializeField] private AudioClip ambientSound;

    [Tooltip("주변음 볼륨")]
    [SerializeField] private float ambientVolume = 0.3f;

    [Tooltip("씬 시작 시 주변음 재생")]
    [SerializeField] private bool playAmbientOnStart = true;

    private AudioSource ambientSource;

    private void Start()
    {
        if (AudioManager.I == null)
        {
            Debug.LogWarning("⚠️ SceneBGMController: AudioManager not found!");
            return;
        }

        // BGM 재생
        if (playOnStart && sceneBGM != null)
        {
            if (overrideAudioManager)
            {
                // AudioManager의 자동 BGM 정지 후 이 BGM 재생
                AudioManager.I.StopBGM(0.5f);
            }

            AudioManager.I.PlayBGM(sceneBGM, fadeInTime);
            Debug.Log($"🎵 SceneBGMController: Playing scene-specific BGM '{sceneBGM.name}'");
        }

        // 주변음 재생
        if (playAmbientOnStart && ambientSound != null)
        {
            PlayAmbientSound();
        }
    }

    /// <summary>
    /// 주변음 재생
    /// </summary>
    private void PlayAmbientSound()
    {
        if (AudioManager.I == null) return;

        // SFXController를 통해 루프 사운드 재생
        // 직접 AudioSource 생성하여 관리
        if (ambientSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSound");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
            ambientSource.clip = ambientSound;
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.playOnAwake = false;
            ambientSource.spatialBlend = 0f; // 2D
            ambientSource.Play();

            Debug.Log($"🔊 SceneBGMController: Playing ambient sound '{ambientSound.name}'");
        }
    }

    /// <summary>
    /// 주변음 정지
    /// </summary>
    public void StopAmbientSound()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }

    /// <summary>
    /// 씬 BGM 변경 (런타임에서 호출 가능)
    /// </summary>
    public void ChangeBGM(AudioClip newBGM, float fadeTime = 1.5f)
    {
        if (newBGM != null && AudioManager.I != null)
        {
            AudioManager.I.PlayBGM(newBGM, fadeTime);
            sceneBGM = newBGM;
        }
    }

    private void OnDestroy()
    {
        // 주변음 정리
        StopAmbientSound();
    }

#if UNITY_EDITOR
    [ContextMenu("Play Scene BGM Now")]
    private void DebugPlaySceneBGM()
    {
        if (Application.isPlaying && sceneBGM != null && AudioManager.I != null)
        {
            AudioManager.I.PlayBGM(sceneBGM, fadeInTime);
            Debug.Log($"🎵 Playing '{sceneBGM.name}'");
        }
    }

    [ContextMenu("Play Ambient Sound Now")]
    private void DebugPlayAmbient()
    {
        if (Application.isPlaying && ambientSound != null)
        {
            PlayAmbientSound();
        }
    }
#endif
}
