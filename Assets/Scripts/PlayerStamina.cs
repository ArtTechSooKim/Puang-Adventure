// ...existing code...
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private Slider staminaSlider; // Optional: can be assigned manually or auto-found via UIReferenceManager
    [SerializeField] private float regenDelay = 1.5f;            // 마지막 소모 후 회복 시작까지 대기
    [SerializeField] private float regenRate = 20f;              // 초당 회복량 (정상)
    [SerializeField] private float exhaustedRegenRate = 6f;      // 0일 때 회복 속도 (느림)
    [SerializeField] private float walkRegenMultiplier = 1.0f;   // 걷고 있을 때 추가 회복 계수 (PlayerController에서 판단하여 SetWalking)
    [SerializeField] private float sprintDrainRate = 15f;       // 초당 소모량(달리기)
    [SerializeField] private float dashCost = 25f;              // 대시 시 즉시 소모량
    [SerializeField] private float attackCost = 10f;            // 공격 시 즉시 소모량
    [Tooltip("스태미나 0일 때 걷기 속도에 곱할 계수 (조금 느리게)")]
    [SerializeField] private float exhaustedSpeedMultiplier = 0.85f;

    private float currentStamina;
    private float lastUseTime = -99f;
    private bool isSprinting = false;
    private bool isWalking = false;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public bool IsSprinting => isSprinting;
    public bool IsExhausted => currentStamina <= 0.0001f;

    private void Awake()
    {
        currentStamina = maxStamina;
        RefreshUIReference();
    }

    /// <summary>
    /// Reconnect to Stamina Slider in the current scene using UIReferenceManager
    /// NEW STRUCTURE: Uses HUD_Canvas/STBar (not Canvas_UI/STBar)
    /// </summary>
    public void RefreshUIReference()
    {
        // Try to get reference from UIReferenceManager first
        if (staminaSlider == null && UIReferenceManager.Instance != null)
        {
            staminaSlider = UIReferenceManager.Instance.GetStaminaSlider();
            if (staminaSlider != null)
            {
                Debug.Log("✅ PlayerStamina: Connected to STBar via UIReferenceManager");
            }
        }

        // Fallback: Find STBar slider in the scene if UIReferenceManager didn't provide it
        if (staminaSlider == null)
        {
            // NEW STRUCTURE: HUD_Canvas/STBar (not Canvas_UI/STBar)
            GameObject stBarObj = GameObject.Find("HUD_Canvas/STBar");
            if (stBarObj != null)
            {
                staminaSlider = stBarObj.GetComponent<Slider>();
                if (staminaSlider != null)
                {
                    Debug.Log("✅ PlayerStamina: Reconnected to STBar in HUD_Canvas");
                }
            }
            else
            {
                Debug.LogWarning("⚠ PlayerStamina: STBar not found in scene at HUD_Canvas/STBar. Make sure UI structure is correct.");
            }
        }

        // Update UI with current values
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
        else
        {
            Debug.LogWarning("⚠ PlayerStamina: staminaSlider is still null after RefreshUIReference. UI will not update.");
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Sprint drain
        if (isSprinting)
        {
            float drain = sprintDrainRate * dt;
            ConsumeStamina(drain);
            // 자동으로 스프린트 중지(스태미나 바닥)
            if (IsExhausted)
                StopSprint();
        }
        else
        {
            // Regen only when not sprinting and not recently used dash/attack (handled by lastUseTime)
            if (Time.time >= lastUseTime + regenDelay)
            {
                float effectiveRegen = IsExhausted ? exhaustedRegenRate : regenRate;
                // 걷고 있으면 추가 보정
                effectiveRegen *= (isWalking ? walkRegenMultiplier : 1f);

                currentStamina = Mathf.Min(maxStamina, currentStamina + effectiveRegen * dt);
                UpdateSlider();
            }
        }
    }

    private void UpdateSlider()
    {
        if (staminaSlider != null) staminaSlider.value = currentStamina;
    }

    private void ConsumeStamina(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        lastUseTime = Time.time;
        UpdateSlider();
    }

    // Public API for PlayerController / other systems

    // Try to start sprint (toggle). Returns true if sprint started or already sprinting.
    public bool TryStartSprint()
    {
        if (isSprinting) return true;
        if (IsExhausted) return false;

        isSprinting = true;
        // immediate small consumption to avoid 0-frame exploit (optional)
        ConsumeStamina(0.1f);
        return true;
    }

    public void StopSprint()
    {
        isSprinting = false;
    }

    // Alternate: set sprint state directly
    public void SetSprint(bool sprint)
    {
        if (sprint) TryStartSprint();
        else StopSprint();
    }

    // Called by PlayerController when walking state changes (e.g., not idle and not sprinting)
    public void SetWalking(bool walking)
    {
        isWalking = walking;
    }

    // Dash: returns true and consumes cost if enough stamina
    public bool TryConsumeDash()
    {
        if (currentStamina >= dashCost)
        {
            ConsumeStamina(dashCost);
            return true;
        }

        return false;
    }

    // Attack: consumes attackCost if available; returns true on success
    public bool TryConsumeAttack()
    {
        if (currentStamina >= attackCost)
        {
            ConsumeStamina(attackCost);
            return true;
        }

        // allow attack even if 0? current design blocks consumption when insufficient
        return false;
    }

    // Utility: called to forcibly reduce stamina (e.g., environmental)
    public void ForceConsume(float amount)
    {
        ConsumeStamina(amount);
    }

    // External heal (e.g., potion)
    public void Restore(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        UpdateSlider();
    }

    // Speed modifier to apply when stamina exhausted
    public float GetExhaustedSpeedMultiplier()
    {
        return IsExhausted ? exhaustedSpeedMultiplier : 1f;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public void ResetStamina()
    {
        currentStamina = maxStamina;
        isSprinting = false;
        isWalking = false;
        lastUseTime = -99f;
        UpdateSlider();
    }

    /// <summary>
    /// Set stamina to a specific value (used by save/load system)
    /// </summary>
    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        UpdateSlider();
        Debug.Log($"✅ PlayerStamina: Stamina set to {currentStamina}/{maxStamina}");
    }
}
// ...existing code...