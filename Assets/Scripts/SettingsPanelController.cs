using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Controller for the Settings Panel.
/// Handles brightness, BGM volume, and SFX volume settings.
/// Saves settings to PlayerPrefs for persistence.
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Brightness slider")]
    [SerializeField] private Slider sliderBrightness;

    [Tooltip("BGM volume slider")]
    [SerializeField] private Slider sliderBGM;

    [Tooltip("SFX volume slider")]
    [SerializeField] private Slider sliderSFX;

    [Header("Audio Settings")]
    [Tooltip("Audio Mixer (optional, for advanced volume control)")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("BGM AudioMixer parameter name")]
    [SerializeField] private string bgmMixerParameter = "BGMVolume";

    [Tooltip("SFX AudioMixer parameter name")]
    [SerializeField] private string sfxMixerParameter = "SFXVolume";

    [Header("Brightness Settings")]
    [Tooltip("Optional: Reference to Light2D or PostProcessing for brightness control")]
    [SerializeField] private Light lightReference;

    [Header("Default Values")]
    [SerializeField] private float defaultBrightness = 1.0f;
    [SerializeField] private float defaultBGMVolume = 0.75f;
    [SerializeField] private float defaultSFXVolume = 0.75f;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;

    // PlayerPrefs keys
    private const string KEY_BRIGHTNESS = "Settings_Brightness";
    private const string KEY_BGM_VOLUME = "Settings_BGM";
    private const string KEY_SFX_VOLUME = "Settings_SFX";

    private void Start()
    {
        // Auto-find sliders from UIReferenceManager if not assigned
        if (sliderBrightness == null || sliderBGM == null || sliderSFX == null)
        {
            AutoFindSliders();
        }

        // Load saved settings (이 안에서 리스너도 추가됨)
        LoadSettings();

        LogDebug("✅ SettingsPanelController: Initialized and settings loaded");
    }

    /// <summary>
    /// Auto-find sliders from UIReferenceManager
    /// </summary>
    private void AutoFindSliders()
    {
        if (UIReferenceManager.Instance == null) return;

        if (sliderBrightness == null)
        {
            sliderBrightness = UIReferenceManager.Instance.sliderBrightness;
        }

        if (sliderBGM == null)
        {
            sliderBGM = UIReferenceManager.Instance.sliderBGM;
        }

        if (sliderSFX == null)
        {
            sliderSFX = UIReferenceManager.Instance.sliderSFX;
        }

        LogDebug("✅ SettingsPanelController: Auto-found sliders from UIReferenceManager");
    }


    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    private void LoadSettings()
    {
        // Load brightness
        float brightness = PlayerPrefs.GetFloat(KEY_BRIGHTNESS, defaultBrightness);
        if (sliderBrightness != null)
        {
            sliderBrightness.value = brightness;
            sliderBrightness.onValueChanged.AddListener(OnBrightnessChanged);
            LogDebug($"✅ SettingsPanelController: Brightness slider listener added");
        }
        else
        {
            Debug.LogWarning("⚠ SettingsPanelController: sliderBrightness is not assigned!");
        }
        ApplyBrightness(brightness);

        // Load BGM volume
        float bgmVolume = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, defaultBGMVolume);
        if (sliderBGM != null)
        {
            sliderBGM.value = bgmVolume;
            sliderBGM.onValueChanged.AddListener(OnBGMVolumeChanged);
            LogDebug($"✅ SettingsPanelController: BGM slider listener added");
        }
        else
        {
            Debug.LogWarning("⚠ SettingsPanelController: sliderBGM is not assigned!");
        }
        ApplyBGMVolume(bgmVolume);

        // Load SFX volume
        float sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, defaultSFXVolume);
        if (sliderSFX != null)
        {
            sliderSFX.value = sfxVolume;
            sliderSFX.onValueChanged.AddListener(OnSFXVolumeChanged);
            LogDebug($"✅ SettingsPanelController: SFX slider listener added");
        }
        else
        {
            Debug.LogWarning("⚠ SettingsPanelController: sliderSFX is not assigned!");
        }
        ApplySFXVolume(sfxVolume);

        LogDebug($"📂 SettingsPanelController: Loaded settings - Brightness: {brightness:F2}, BGM: {bgmVolume:F2}, SFX: {sfxVolume:F2}");
    }

    /// <summary>
    /// Called when brightness slider changes
    /// </summary>
    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat(KEY_BRIGHTNESS, value);
        PlayerPrefs.Save();
        LogDebug($"🔆 SettingsPanelController: Brightness changed to {value:F2}");
    }

    /// <summary>
    /// Called when BGM volume slider changes
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        ApplyBGMVolume(value);
        PlayerPrefs.SetFloat(KEY_BGM_VOLUME, value);
        PlayerPrefs.Save();
        LogDebug($"🎵 SettingsPanelController: BGM volume changed to {value:F2}");
    }

    /// <summary>
    /// Called when SFX volume slider changes
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        ApplySFXVolume(value);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, value);
        PlayerPrefs.Save();
        LogDebug($"🔊 SettingsPanelController: SFX volume changed to {value:F2}");
    }

    /// <summary>
    /// Apply brightness value
    /// </summary>
    private void ApplyBrightness(float value)
    {
        // Clamp value between 0 and 1
        value = Mathf.Clamp01(value);

        // Option 1: Use Light reference
        if (lightReference != null)
        {
            lightReference.intensity = value * 2f; // Adjust multiplier as needed
        }

        // Option 2: Use RenderSettings (for global ambient light)
        RenderSettings.ambientIntensity = value;

        // TODO: If using Post Processing Volume, adjust exposure here
        // Example:
        // if (postProcessVolume != null && postProcessVolume.profile.TryGet<AutoExposure>(out var exposure))
        // {
        //     exposure.keyValue.value = value;
        // }
    }

    /// <summary>
    /// Apply BGM volume value
    /// </summary>
    private void ApplyBGMVolume(float value)
    {
        // Clamp value between 0 and 1
        value = Mathf.Clamp01(value);

        // 🔊 AudioManager를 통해 BGM 볼륨 설정
        if (AudioManager.I != null)
        {
            AudioManager.I.SetBGMVolume(value);
        }

        // Option 1: Use AudioMixer (fallback if AudioManager not available)
        if (audioMixer != null)
        {
            // Convert 0-1 range to decibels (-80 to 0)
            float volumeDB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(bgmMixerParameter, volumeDB);
        }
    }

    /// <summary>
    /// Apply SFX volume value
    /// </summary>
    private void ApplySFXVolume(float value)
    {
        // Clamp value between 0 and 1
        value = Mathf.Clamp01(value);

        // 🔊 AudioManager를 통해 SFX 볼륨 설정
        if (AudioManager.I != null)
        {
            AudioManager.I.SetSFXVolume(value);
        }

        // Option 1: Use AudioMixer (fallback if AudioManager not available)
        if (audioMixer != null)
        {
            // Convert 0-1 range to decibels (-80 to 0)
            float volumeDB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat(sfxMixerParameter, volumeDB);
        }
    }

    /// <summary>
    /// Reset settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        if (sliderBrightness != null) sliderBrightness.value = defaultBrightness;
        if (sliderBGM != null) sliderBGM.value = defaultBGMVolume;
        if (sliderSFX != null) sliderSFX.value = defaultSFXVolume;

        PlayerPrefs.DeleteKey(KEY_BRIGHTNESS);
        PlayerPrefs.DeleteKey(KEY_BGM_VOLUME);
        PlayerPrefs.DeleteKey(KEY_SFX_VOLUME);
        PlayerPrefs.Save();

        LogDebug("🔄 SettingsPanelController: Settings reset to defaults");
    }

    /// <summary>
    /// Helper method to log debug messages
    /// </summary>
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Reset Settings to Defaults")]
    private void DebugResetSettings()
    {
        ResetToDefaults();
    }

    [ContextMenu("Debug: Log Current Settings")]
    private void DebugLogSettings()
    {
        float brightness = PlayerPrefs.GetFloat(KEY_BRIGHTNESS, defaultBrightness);
        float bgm = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, defaultBGMVolume);
        float sfx = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, defaultSFXVolume);

        Debug.Log("=== Current Settings ===");
        Debug.Log($"Brightness: {brightness:F2}");
        Debug.Log($"BGM Volume: {bgm:F2}");
        Debug.Log($"SFX Volume: {sfx:F2}");
        Debug.Log("=======================");
    }
#endif
}
