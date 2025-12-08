using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// 전체 오디오 시스템을 관리하는 싱글톤 매니저.
/// DontDestroyOnLoad로 씬 전환 시에도 지속됩니다.
/// BGMController와 SFXController를 총괄 관리합니다.
/// SettingsPanelController의 AudioMixer 설정과 연동됩니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Controllers")]
    [Tooltip("BGM 컨트롤러 (자동 생성됨)")]
    [SerializeField] private BGMController bgmController;

    [Tooltip("SFX 컨트롤러 (자동 생성됨)")]
    [SerializeField] private SFXController sfxController;

    [Header("Audio Clips")]
    [Tooltip("모든 오디오 클립을 담은 ScriptableObject")]
    [SerializeField] private AudioClipData audioClipData;

    [Header("Audio Mixer")]
    [Tooltip("메인 AudioMixer (SettingsPanelController와 동일한 것 사용)")]
    [SerializeField] private AudioMixer masterMixer;

    [Tooltip("BGM AudioMixer Group")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Tooltip("SFX AudioMixer Group")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Scene BGM Mapping")]
    [Tooltip("타이틀 씬 BGM")]
    [SerializeField] private AudioClip titleBGM;

    [Tooltip("튜토리얼 씬 BGM")]
    [SerializeField] private AudioClip tutorialBGM;

    [Tooltip("마을 씬 BGM")]
    [SerializeField] private AudioClip villageBGM;

    [Tooltip("숲 씬 BGM")]
    [SerializeField] private AudioClip forestBGM;

    [Tooltip("동굴 씬 BGM")]
    [SerializeField] private AudioClip caveBGM;

    [Tooltip("보스 씬 BGM (긴장감)")]
    [SerializeField] private AudioClip bossIntenseBGM;

    [Tooltip("최종 보스 씬 BGM (서사적)")]
    [SerializeField] private AudioClip bossEpicBGM;

    [Tooltip("엔딩 씬 BGM")]
    [SerializeField] private AudioClip endingBGM;

    [Header("Settings")]
    [Tooltip("씬 전환 시 BGM 페이드 시간")]
    [SerializeField] private float sceneTransitionFadeTime = 2f;

    [Tooltip("디버그 로그 출력")]
    [SerializeField] private bool showDebugLogs = true;

    // 현재 씬 BGM 추적
    private string currentSceneName;
    private AudioClip currentSceneBGM;

    private void Awake()
    {
        // 싱글톤 패턴
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystem();
            LogDebug("✅ AudioManager: Initialized and persisting across scenes");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 오디오 시스템 초기화
    /// </summary>
    private void InitializeAudioSystem()
    {
        // BGMController 생성 또는 가져오기
        if (bgmController == null)
        {
            GameObject bgmObj = new GameObject("BGMController");
            bgmObj.transform.SetParent(transform);
            bgmController = bgmObj.AddComponent<BGMController>();
            LogDebug("✅ AudioManager: Created BGMController");

            // BGMController에 MixerGroup 할당
            if (bgmMixerGroup != null)
            {
                bgmController.SetMixerGroup(bgmMixerGroup);
                LogDebug($"✅ AudioManager: Assigned bgmMixerGroup '{bgmMixerGroup.name}' to BGMController");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager: bgmMixerGroup is null! BGM volume control will not work.");
            }
        }

        // SFXController 생성 또는 가져오기
        if (sfxController == null)
        {
            GameObject sfxObj = new GameObject("SFXController");
            sfxObj.transform.SetParent(transform);
            sfxController = sfxObj.AddComponent<SFXController>();
            LogDebug("✅ AudioManager: Created SFXController");

            // SFXController에 MixerGroup 할당
            if (sfxMixerGroup != null)
            {
                sfxController.SetMixerGroup(sfxMixerGroup);
                LogDebug($"✅ AudioManager: Assigned sfxMixerGroup '{sfxMixerGroup.name}' to SFXController");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager: sfxMixerGroup is null! SFX volume control will not work.");
            }
        }

        // AudioClipData 검증
        if (audioClipData == null)
        {
            Debug.LogWarning("⚠️ AudioManager: AudioClipData is not assigned! Please assign it in the Inspector.");
        }
    }

    /// <summary>
    /// 씬 로드 시 호출되는 이벤트 핸들러
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        AudioClip bgmToPlay = GetBGMForScene(scene.name);

        if (bgmToPlay != null)
        {
            PlayBGM(bgmToPlay, sceneTransitionFadeTime);
            LogDebug($"🎵 AudioManager: Playing BGM for scene '{scene.name}'");
        }
        else
        {
            LogDebug($"⚠️ AudioManager: No BGM assigned for scene '{scene.name}'");
        }
    }

    /// <summary>
    /// 씬 이름에 따라 적절한 BGM 반환
    /// </summary>
    private AudioClip GetBGMForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "00_TitleScene":
                return titleBGM;

            case "01_InitialScene":
            case "TutorialScene":
                return tutorialBGM;

            case "02_VillageScene":
                return villageBGM;

            case "03_ForestScene":
                return forestBGM;

            case "04_CaveScene":
                return caveBGM;

            case "06_UnkillableBossScene":
                return bossIntenseBGM;

            case "07_BossScene":
                return bossEpicBGM;

            case "08_EndingScene":
                return endingBGM;

            case "05_PeuangSadScene":
                // 시네마틱 씬은 비디오 오디오 사용, BGM 없음
                return null;

            default:
                return null;
        }
    }

    // ==================== BGM 제어 ====================

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(AudioClip clip, float fadeTime = 1.5f)
    {
        if (bgmController != null)
        {
            bgmController.PlayBGM(clip, fadeTime);
            currentSceneBGM = clip;
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM(float fadeTime = 1.5f)
    {
        if (bgmController != null)
        {
            bgmController.StopBGM(fadeTime);
        }
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        if (bgmController != null)
        {
            bgmController.PauseBGM();
        }
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmController != null)
        {
            bgmController.ResumeBGM();
        }
    }

    /// <summary>
    /// 다이얼로그 시작 시 BGM 볼륨 감소
    /// </summary>
    public void DuckBGM()
    {
        if (bgmController != null)
        {
            bgmController.DuckBGM();
        }
    }

    /// <summary>
    /// 다이얼로그 종료 시 BGM 볼륨 복구
    /// </summary>
    public void RestoreBGM()
    {
        if (bgmController != null)
        {
            bgmController.RestoreBGM();
        }
    }

    // ==================== SFX 제어 ====================

    /// <summary>
    /// 2D 효과음 재생 (클립 직접 전달)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (sfxController != null)
        {
            sfxController.PlaySFX(clip, volume, pitch);
        }
    }

    /// <summary>
    /// 3D 효과음 재생 (클립 직접 전달)
    /// </summary>
    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (sfxController != null)
        {
            sfxController.PlaySFX3D(clip, position, volume, pitch);
        }
    }

    /// <summary>
    /// 랜덤 피치로 효과음 재생
    /// </summary>
    public void PlaySFXRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f, float volume = 1f)
    {
        if (sfxController != null)
        {
            sfxController.PlaySFXRandomPitch(clip, minPitch, maxPitch, volume);
        }
    }

    /// <summary>
    /// 모든 효과음 정지
    /// </summary>
    public void StopAllSFX()
    {
        if (sfxController != null)
        {
            sfxController.StopAllSFX();
        }
    }

    // ==================== AudioClipData 헬퍼 메서드 ====================
    // 자주 사용하는 효과음을 간편하게 재생할 수 있는 헬퍼 메서드들

    /// <summary>
    /// 플레이어 공격음
    /// </summary>
    public void PlayPlayerAttackSound()
    {
        if (audioClipData != null && audioClipData.swordSlash != null)
        {
            PlaySFXRandomPitch(audioClipData.swordSlash, 0.95f, 1.05f, 0.7f);
        }
    }

    /// <summary>
    /// 플레이어 대시음
    /// </summary>
    public void PlayPlayerDashSound()
    {
        if (audioClipData != null && audioClipData.dashWhoosh != null)
        {
            PlaySFX(audioClipData.dashWhoosh, 0.8f);
        }
    }

    /// <summary>
    /// 플레이어 피격음
    /// </summary>
    public void PlayPlayerHitSound()
    {
        if (audioClipData != null && audioClipData.playerHit != null)
        {
            PlaySFX(audioClipData.playerHit, 0.9f);
        }
    }

    /// <summary>
    /// 플레이어 사망음
    /// </summary>
    public void PlayPlayerDeathSound()
    {
        if (audioClipData != null && audioClipData.playerDeath != null)
        {
            PlaySFX(audioClipData.playerDeath, 1f);
        }
    }

    /// <summary>
    /// 플레이어 회복음
    /// </summary>
    public void PlayPlayerHealSound()
    {
        if (audioClipData != null && audioClipData.playerHeal != null)
        {
            PlaySFX(audioClipData.playerHeal, 0.8f);
        }
    }

    /// <summary>
    /// 궁극기 슬래시음
    /// </summary>
    public void PlayUltimateSlashSound()
    {
        if (audioClipData != null && audioClipData.ultimateSlash != null)
        {
            PlaySFX(audioClipData.ultimateSlash, 0.9f);
        }
    }

    /// <summary>
    /// 적 사망음 (3D)
    /// </summary>
    public void PlayEnemyDeathSound(Vector3 position)
    {
        if (audioClipData != null && audioClipData.enemyDeath != null)
        {
            PlaySFX3D(audioClipData.enemyDeath, position, 0.7f);
        }
    }

    /// <summary>
    /// 보스 사망음 (3D)
    /// </summary>
    public void PlayBossDeathSound(Vector3 position)
    {
        if (audioClipData != null && audioClipData.bossDeath != null)
        {
            PlaySFX3D(audioClipData.bossDeath, position, 0.9f);
        }
    }

    /// <summary>
    /// 적 피격음 (3D)
    /// </summary>
    public void PlayEnemyHitSound(Vector3 position)
    {
        if (audioClipData != null && audioClipData.enemyHit != null)
        {
            PlaySFX3D(audioClipData.enemyHit, position, 0.6f);
        }
    }

    /// <summary>
    /// UI 클릭음
    /// </summary>
    public void PlayUIClickSound()
    {
        if (audioClipData != null && audioClipData.uiClick != null)
        {
            PlaySFX(audioClipData.uiClick, 0.5f);
        }
    }

    /// <summary>
    /// UI 패널 열기음
    /// </summary>
    public void PlayUIPanelOpenSound()
    {
        if (audioClipData != null && audioClipData.uiPanelOpen != null)
        {
            PlaySFX(audioClipData.uiPanelOpen, 0.6f);
        }
    }

    /// <summary>
    /// UI 패널 닫기음
    /// </summary>
    public void PlayUIPanelCloseSound()
    {
        if (audioClipData != null && audioClipData.uiPanelClose != null)
        {
            PlaySFX(audioClipData.uiPanelClose, 0.6f);
        }
    }

    /// <summary>
    /// 다이얼로그 열기음
    /// </summary>
    public void PlayDialogueOpenSound()
    {
        if (audioClipData != null && audioClipData.dialogueOpen != null)
        {
            PlaySFX(audioClipData.dialogueOpen, 0.7f);
        }
    }

    /// <summary>
    /// 다이얼로그 닫기음
    /// </summary>
    public void PlayDialogueCloseSound()
    {
        if (audioClipData != null && audioClipData.dialogueClose != null)
        {
            PlaySFX(audioClipData.dialogueClose, 0.7f);
        }
    }

    /// <summary>
    /// 다이얼로그 진행음
    /// </summary>
    public void PlayDialogueAdvanceSound()
    {
        if (audioClipData != null && audioClipData.dialogueAdvance != null)
        {
            PlaySFX(audioClipData.dialogueAdvance, 0.4f);
        }
    }

    /// <summary>
    /// 포털 진입음
    /// </summary>
    public void PlayPortalEnterSound()
    {
        if (audioClipData != null && audioClipData.portalEnter != null)
        {
            PlaySFX(audioClipData.portalEnter, 0.8f);
        }
    }

    /// <summary>
    /// 퀘스트 진행음
    /// </summary>
    public void PlayQuestProgressSound()
    {
        if (audioClipData != null && audioClipData.questProgress != null)
        {
            PlaySFX(audioClipData.questProgress, 0.8f);
        }
    }

    /// <summary>
    /// 아이템 획득음
    /// </summary>
    public void PlayItemPickupSound()
    {
        if (audioClipData != null && audioClipData.itemPickup != null)
        {
            PlaySFX(audioClipData.itemPickup, 0.7f);
        }
    }

    /// <summary>
    /// 보스 승리음
    /// </summary>
    public void PlayBossVictorySound()
    {
        if (audioClipData != null && audioClipData.bossVictory != null)
        {
            PlaySFX(audioClipData.bossVictory, 1f);
        }
    }

    /// <summary>
    /// 능력 해금음
    /// </summary>
    public void PlayAbilityUnlockSound()
    {
        if (audioClipData != null && audioClipData.abilityUnlock != null)
        {
            PlaySFX(audioClipData.abilityUnlock, 0.9f);
        }
    }

    /// <summary>
    /// 게임 오버음
    /// </summary>
    public void PlayGameOverSound()
    {
        if (audioClipData != null && audioClipData.gameOver != null)
        {
            PlaySFX(audioClipData.gameOver, 1f);
        }
    }

    // ==================== 볼륨 설정 ====================

    /// <summary>
    /// BGM 볼륨 설정 (0.0 ~ 1.0)
    /// SettingsPanelController에서 호출됩니다.
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (masterMixer != null)
        {
            // 0-1 범위를 데시벨로 변환 (-80dB ~ 0dB)
            float volumeDB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            masterMixer.SetFloat("BGMVolume", volumeDB);
            LogDebug($"🎵 AudioManager: BGM volume set to {volume:F2} ({volumeDB:F1} dB)");
        }
        else
        {
            Debug.LogWarning("⚠️ AudioManager: masterMixer is null! Cannot set BGM volume.");
        }
    }

    /// <summary>
    /// SFX 볼륨 설정 (0.0 ~ 1.0)
    /// SettingsPanelController에서 호출됩니다.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (masterMixer != null)
        {
            // 0-1 범위를 데시벨로 변환 (-80dB ~ 0dB)
            float volumeDB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            masterMixer.SetFloat("SFXVolume", volumeDB);
            LogDebug($"🔊 AudioManager: SFX volume set to {volume:F2} ({volumeDB:F1} dB)");
        }
        else
        {
            Debug.LogWarning("⚠️ AudioManager: masterMixer is null! Cannot set SFX volume.");
        }
    }

    /// <summary>
    /// 현재 BGM 볼륨 가져오기
    /// </summary>
    public float GetBGMVolume()
    {
        if (masterMixer != null && masterMixer.GetFloat("BGMVolume", out float volumeDB))
        {
            // 데시벨을 0-1 범위로 변환
            return volumeDB <= -80f ? 0f : Mathf.Pow(10f, volumeDB / 20f);
        }
        return 1f; // 기본값
    }

    /// <summary>
    /// 현재 SFX 볼륨 가져오기
    /// </summary>
    public float GetSFXVolume()
    {
        if (masterMixer != null && masterMixer.GetFloat("SFXVolume", out float volumeDB))
        {
            // 데시벨을 0-1 범위로 변환
            return volumeDB <= -80f ? 0f : Mathf.Pow(10f, volumeDB / 20f);
        }
        return 1f; // 기본값
    }

    // ==================== 디버그 ====================

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Log Audio System Status")]
    private void DebugLogStatus()
    {
        Debug.Log("=== Audio Manager Status ===");
        Debug.Log($"Current Scene: {currentSceneName}");
        Debug.Log($"Current BGM: {(currentSceneBGM != null ? currentSceneBGM.name : "None")}");
        Debug.Log($"BGM Playing: {(bgmController != null ? bgmController.IsPlaying() : false)}");
        Debug.Log($"AudioClipData Assigned: {audioClipData != null}");

        if (sfxController != null)
        {
            sfxController.LogStatistics();
        }

        Debug.Log("============================");
    }

    [ContextMenu("Debug: Stop All Audio")]
    private void DebugStopAllAudio()
    {
        StopBGM(0.5f);
        StopAllSFX();
        Debug.Log("🔇 All audio stopped");
    }
#endif
}
