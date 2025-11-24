using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Audio;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider hpSlider; // Optional: can be assigned manually or auto-found via UIReferenceManager

    private int currentHealth;
    private bool ignoreDeathProcessing = false; // 특수 씬에서 사망 처리 무시

    void Awake()
    {
        currentHealth = maxHealth;
        RefreshUIReference();
    }

    /// <summary>
    /// Reconnect to HP Slider in the current scene using UIReferenceManager
    /// NEW STRUCTURE: Uses HUD_Canvas/HPBar (not Canvas_UI/HPBar)
    /// </summary>
    public void RefreshUIReference()
    {
        // Try to get reference from UIReferenceManager first
        if (hpSlider == null && UIReferenceManager.Instance != null)
        {
            hpSlider = UIReferenceManager.Instance.GetHPSlider();
            if (hpSlider != null)
            {
                Debug.Log("✅ PlayerHealth: Connected to HPBar via UIReferenceManager");
            }
        }

        // Fallback: Find HPBar slider in the scene if UIReferenceManager didn't provide it
        if (hpSlider == null)
        {
            // NEW STRUCTURE: HUD_Canvas/HPBar (not Canvas_UI/HPBar)
            GameObject hpBarObj = GameObject.Find("HUD_Canvas/HPBar");
            if (hpBarObj != null)
            {
                hpSlider = hpBarObj.GetComponent<Slider>();
                if (hpSlider != null)
                {
                    Debug.Log("✅ PlayerHealth: Reconnected to HPBar in HUD_Canvas");
                }
            }
            else
            {
                Debug.LogWarning("⚠ PlayerHealth: HPBar not found in scene at HUD_Canvas/HPBar. Make sure UI structure is correct.");
            }
        }

        // Update UI with current values
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("⚠ PlayerHealth: hpSlider is still null after RefreshUIReference. UI will not update.");
        }
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"[PlayerHealth] 데미지 {amount} 받음, 남은 체력: {currentHealth - amount}");
        if (amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();
        if (currentHealth == 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
    }

    private void Die()
    {
        Debug.Log("Player died");

        // 특수 씬에서는 GameManager의 사망 처리를 건너뜀
        if (!ignoreDeathProcessing)
        {
            GameManager.I?.OnPlayerDeath();
            // 추가: 리스폰/게임오버 UI 로직은 GameManager에서 처리
        }
        else
        {
            Debug.Log("⚠ PlayerHealth: Death processing ignored (special scene handling)");
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    /// <summary>
    /// 특수 씬에서 사망 처리를 무시하도록 설정 (UnkillableBossScene 등)
    /// </summary>
    public void SetIgnoreDeathProcessing(bool ignore)
    {
        ignoreDeathProcessing = ignore;
        if (ignore)
            Debug.Log("⚠ PlayerHealth: Death processing will be ignored");
        else
            Debug.Log("✅ PlayerHealth: Death processing re-enabled");
    }
}