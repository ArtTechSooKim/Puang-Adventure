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
    private bool isDead = false; // 사망 상태 추적
    private Animator anim; // Player Animator
    private PlayerController playerController; // Player 조작 비활성화용

    void Awake()
    {
        currentHealth = maxHealth;
        RefreshUIReference();

        // Animator와 PlayerController 가져오기
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        if (anim == null)
            Debug.LogWarning("⚠ PlayerHealth: Animator를 찾을 수 없습니다. 사망 애니메이션이 재생되지 않습니다.");
        if (playerController == null)
            Debug.LogWarning("⚠ PlayerHealth: PlayerController를 찾을 수 없습니다.");
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
        // 이미 죽었으면 추가 데미지 무시
        if (isDead) return;

        Debug.Log($"[PlayerHealth] 데미지 {amount} 받음, 남은 체력: {currentHealth - amount}");
        if (amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();

        if (currentHealth == 0)
        {
            Die();
        }
        else
        {
            // 🔊 피격 사운드 재생 (사망하지 않았을 때만)
            AudioManager.I?.PlayPlayerHitSound();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();

        // 🔊 회복 사운드 재생
        AudioManager.I?.PlayPlayerHealSound();
    }

    private void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
    }

    private void Die()
    {
        if (isDead) return; // 중복 호출 방지
        isDead = true;

        Debug.Log("Player died");

        // 🎬 사망 애니메이션 재생
        if (anim != null)
        {
            anim.SetTrigger("Dead");
            Debug.Log("✅ PlayerHealth: 사망 애니메이션 트리거 발동");
        }

        // 🚫 Player 조작 비활성화
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("✅ PlayerHealth: PlayerController 비활성화");
        }

        // Rigidbody2D 정지 (물리 시뮬레이션 중단)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // 움직임 완전 정지
        }

        // 특수 씬에서는 GameManager의 사망 처리를 건너뜀
        if (!ignoreDeathProcessing)
        {
            // 사망 애니메이션 재생 후 게임오버 처리 (약간 딜레이)
            StartCoroutine(DelayedGameOver());
        }
        else
        {
            Debug.Log("⚠ PlayerHealth: Death processing ignored (special scene handling)");
        }
    }

    /// <summary>
    /// 사망 애니메이션 재생 후 게임오버 처리
    /// </summary>
    private IEnumerator DelayedGameOver()
    {
        // 사망 애니메이션이 재생될 시간 확보 (애니메이션 길이에 맞춰 조정)
        yield return new WaitForSeconds(1.5f);

        // GameManager에 사망 알림
        GameManager.I?.OnPlayerDeath();
        Debug.Log("✅ PlayerHealth: GameManager.OnPlayerDeath() 호출됨");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false; // 부활 시 사망 상태 해제
        UpdateUI();

        // PlayerController 재활성화
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Rigidbody2D 복구
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        Debug.Log("✅ PlayerHealth: 체력 리셋 및 부활");
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