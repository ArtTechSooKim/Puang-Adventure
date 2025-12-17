using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures the Player object persists across scene transitions.
/// Manages player position, health, stamina, and references during scene loads.
/// </summary>
public class PlayerPersistent : MonoBehaviour
{
    public static PlayerPersistent Instance { get; private set; }

    [Header("Persistence Settings")]
    [Tooltip("If true, player position will be saved and restored between scenes")]
    public bool savePosition = true;

    [Header("References (Auto-cached on Awake)")]
    private PlayerHealth playerHealth;
    private PlayerStamina playerStamina;
    private PlayerController playerController;
    private Animator animator;

    // Saved state
    private Vector3 savedPosition;
    private bool hasPositionData = false;

    // Pending dialogue (UnkillableBoss 씬 전환 시 사용)
    private string pendingDialogue = null;
    private bool shouldFadeInOnLoad = false; // Village 씬 로드 후 페이드 인 필요 여부

    private void Awake()
    {
        // TitleScene에서는 기존 Instance를 삭제하고 새로운 Player로 교체
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "00_TitleScene" && Instance != null)
        {
            Debug.Log($"🔄 PlayerPersistent: TitleScene에서 기존 Player 삭제 - 새로운 게임 시작");
            SceneManager.sceneLoaded -= OnSceneLoaded; // 이벤트 구독 해제
            Destroy(Instance.gameObject);
            Instance = null;
        }

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cache references
            CacheReferences();

            // Subscribe to scene loading
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log($"✅ PlayerPersistent: Player '{gameObject.name}' persistence enabled - moved to DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning($"⚠ PlayerPersistent: Duplicate Player '{gameObject.name}' detected - destroying (Instance: '{Instance.gameObject.name}')");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Cleanup event subscription
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log($"🗑 PlayerPersistent: Player '{gameObject.name}' is being destroyed");
        }
        else
        {
            Debug.Log($"🗑 PlayerPersistent: Duplicate Player '{gameObject.name}' destroyed as expected");
        }
    }

    /// <summary>
    /// Cache all player component references
    /// </summary>
    private void CacheReferences()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerStamina = GetComponent<PlayerStamina>();
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        if (playerHealth == null) Debug.LogWarning("⚠ PlayerPersistent: PlayerHealth not found");
        if (playerStamina == null) Debug.LogWarning("⚠ PlayerPersistent: PlayerStamina not found");
        if (playerController == null) Debug.LogWarning("⚠ PlayerPersistent: PlayerController not found");
    }

    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 PlayerPersistent: Scene '{scene.name}' loaded");

        // Restore position if available
        if (hasPositionData && savePosition)
        {
            transform.position = savedPosition;
            Debug.Log($"📍 PlayerPersistent: Restored position to {savedPosition}");
            hasPositionData = false;
        }
        else
        {
            // Try to find spawn point in new scene
            TryFindSpawnPoint(scene);
        }

        // Re-cache references in case components were affected by scene load
        CacheReferences();

        // Reconnect Cinemachine camera to follow player
        ReconnectCinemachine();

        // Reconnect Inventory UI references
        ReconnectInventoryUI();

        // Reconnect Health and Stamina UI references
        ReconnectHealthStaminaUI();

        // SlashFX 강제 초기화 (세이브/로드 시 남아있는 이펙트 제거)
        ForceResetSlashEffects();

        // Notify systems that player has entered a new scene
        OnPlayerEnteredScene(scene);

        // Fade in 처리 (UnkillableBoss → Village 전환 시)
        if (shouldFadeInOnLoad)
        {
            StartCoroutine(FadeInAndShowDialogue());
            shouldFadeInOnLoad = false;
        }
        // Pending dialogue 처리 (일반적인 경우)
        else if (!string.IsNullOrEmpty(pendingDialogue))
        {
            StartCoroutine(ShowPendingDialogueDelayed());
        }
    }

    /// <summary>
    /// Reconnect Cinemachine Virtual Camera to follow the persistent player
    /// Finds player by "Player" tag for compatibility
    /// </summary>
    private void ReconnectCinemachine()
    {
        // Find the player GameObject by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠ PlayerPersistent: No GameObject with 'Player' tag found");
            return;
        }

        Debug.Log($"🔍 PlayerPersistent: Searching for Cinemachine cameras to connect to Player '{player.name}'");

        // Find all components that might be Cinemachine cameras
        MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int cameraCount = 0;

        foreach (var component in allComponents)
        {
            if (component == null) continue;

            string typeName = component.GetType().Name;
            string fullTypeName = component.GetType().FullName;

            // Check if this is a Cinemachine Virtual Camera
            if (typeName.Contains("CinemachineVirtualCamera") ||
                typeName == "CinemachineVirtualCamera" ||
                fullTypeName.Contains("Cinemachine"))
            {
                Debug.Log($"🎥 Found component: {component.name} (Type: {typeName})");

                // Use reflection to set the Follow property
                var followProperty = component.GetType().GetProperty("Follow");
                if (followProperty != null && followProperty.CanWrite)
                {
                    followProperty.SetValue(component, player.transform);
                    cameraCount++;
                    Debug.Log($"✅ PlayerPersistent: Connected '{component.name}' to follow Player '{player.name}'");
                }
                else
                {
                    Debug.LogWarning($"⚠ PlayerPersistent: '{component.name}' has no writable 'Follow' property");
                }
            }
        }

        if (cameraCount == 0)
        {
            Debug.LogWarning("⚠ PlayerPersistent: No Cinemachine cameras found in scene. Camera may not follow player.");
        }
        else
        {
            Debug.Log($"📷 PlayerPersistent: Successfully connected {cameraCount} Cinemachine camera(s)");
        }
    }

    /// <summary>
    /// Reconnect Inventory UI references after scene load
    /// </summary>
    private void ReconnectInventoryUI()
    {
        // Refresh Inventory UI references
        if (Inventory.instance != null)
        {
            Inventory.instance.RefreshUIReferences();
        }

        Debug.Log("📦 PlayerPersistent: Reconnected Inventory UI");
    }

    /// <summary>
    /// Reconnect Health and Stamina UI references after scene load
    /// </summary>
    private void ReconnectHealthStaminaUI()
    {
        // Refresh Health UI reference
        if (playerHealth != null)
        {
            playerHealth.RefreshUIReference();
        }
        else
        {
            Debug.LogWarning("⚠ PlayerPersistent: PlayerHealth reference is null");
        }

        // Refresh Stamina UI reference
        if (playerStamina != null)
        {
            playerStamina.RefreshUIReference();
        }
        else
        {
            Debug.LogWarning("⚠ PlayerPersistent: PlayerStamina reference is null");
        }

        Debug.Log("💚 PlayerPersistent: Reconnected Health and Stamina UI");
    }

    /// <summary>
    /// SlashFX를 강제로 초기화 (세이브/로드 시 남아있는 이펙트 제거)
    /// </summary>
    private void ForceResetSlashEffects()
    {
        // Player의 자식 오브젝트에서 SlashEffectController 찾기
        SlashEffectController[] slashControllers = GetComponentsInChildren<SlashEffectController>(true);

        foreach (var controller in slashControllers)
        {
            if (controller != null)
            {
                controller.ForceHide();
                Debug.Log($"✅ PlayerPersistent: SlashFX 강제 초기화 - {controller.gameObject.name}");
            }
        }

        if (slashControllers.Length == 0)
        {
            Debug.Log("⚠ PlayerPersistent: SlashEffectController를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// Try to find and move player to a spawn point in the scene
    /// </summary>
    private void TryFindSpawnPoint(Scene scene)
    {
        // 먼저 PlayerPrefs에서 지정된 스폰 포인트 이름 확인
        string targetSpawnPointName = PlayerPrefs.GetString("TargetSpawnPoint", "");

        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            // 이름으로 오브젝트 찾기
            GameObject spawnPoint = GameObject.Find(targetSpawnPointName);

            if (spawnPoint != null)
            {
                // PortalSpawnPoint 컴포넌트가 있는지 확인
                PortalSpawnPoint portalSpawn = spawnPoint.GetComponent<PortalSpawnPoint>();

                if (portalSpawn != null)
                {
                    // 오프셋이 적용된 위치로 이동
                    transform.position = portalSpawn.GetSpawnPosition();
                    Debug.Log($"📍 PlayerPersistent: Moved to custom spawn point '{targetSpawnPointName}' with offset at {portalSpawn.GetSpawnPosition()}");

                    // 회전도 적용 (필요시)
                    if (portalSpawn != null)
                    {
                        transform.rotation = portalSpawn.GetSpawnRotation();
                    }
                }
                else
                {
                    // PortalSpawnPoint 컴포넌트가 없으면 기본 위치 사용
                    transform.position = spawnPoint.transform.position;
                    Debug.Log($"📍 PlayerPersistent: Moved to custom spawn point '{targetSpawnPointName}' at {spawnPoint.transform.position} (no PortalSpawnPoint component)");
                }

                // 사용 후 삭제
                PlayerPrefs.DeleteKey("TargetSpawnPoint");
                return;
            }
            else
            {
                Debug.LogWarning($"⚠ PlayerPersistent: Custom spawn point '{targetSpawnPointName}' not found in scene - falling back to PlayerSpawn tag");
                PlayerPrefs.DeleteKey("TargetSpawnPoint");
            }
        }

        // 기본 동작: PlayerSpawn 태그로 찾기
        GameObject defaultSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (defaultSpawnPoint != null)
        {
            // PortalSpawnPoint 컴포넌트 확인
            PortalSpawnPoint portalSpawn = defaultSpawnPoint.GetComponent<PortalSpawnPoint>();

            if (portalSpawn != null)
            {
                transform.position = portalSpawn.GetSpawnPosition();
                transform.rotation = portalSpawn.GetSpawnRotation();
                Debug.Log($"📍 PlayerPersistent: Moved to PlayerSpawn with offset at {portalSpawn.GetSpawnPosition()}");
            }
            else
            {
                transform.position = defaultSpawnPoint.transform.position;
                Debug.Log($"📍 PlayerPersistent: Moved to PlayerSpawn at {defaultSpawnPoint.transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ PlayerPersistent: No 'PlayerSpawn' tag found in scene - player position unchanged");
        }
    }

    /// <summary>
    /// Save current position before transitioning (called by PortalTrigger or scene transition system)
    /// </summary>
    public void SaveCurrentPosition()
    {
        savedPosition = transform.position;
        hasPositionData = true;
        Debug.Log($"💾 PlayerPersistent: Position saved: {savedPosition}");
    }

    /// <summary>
    /// Restore player to a specific position (useful for portal transitions)
    /// </summary>
    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        Debug.Log($"📍 PlayerPersistent: Position set to {newPosition}");
    }

    /// <summary>
    /// Called when player enters a new scene - hook for other systems
    /// </summary>
    private void OnPlayerEnteredScene(Scene scene)
    {
        // Notify other managers that player has entered a new scene
        // Example: QuestManager can update objectives, Inventory can refresh UI

        if (GameManager.I != null)
        {
            // GameManager can handle scene-specific logic
        }
    }

    /// <summary>
    /// Public getters for cached components
    /// </summary>
    public PlayerHealth Health => playerHealth;
    public PlayerStamina Stamina => playerStamina;
    public PlayerController Controller => playerController;
    public Animator Anim => animator;

    /// <summary>
    /// Check if player is alive
    /// </summary>
    public bool IsAlive()
    {
        if (playerHealth == null) return true;
        return playerHealth.GetCurrentHealth() > 0;
    }

    /// <summary>
    /// Reset player state (useful for respawn or new game)
    /// </summary>
    public void ResetPlayerState()
    {
        if (playerHealth != null) playerHealth.ResetHealth();
        if (playerStamina != null) playerStamina.ResetStamina();

        hasPositionData = false;
        Debug.Log("🔄 PlayerPersistent: Player state reset");
    }

    /// <summary>
    /// 씬 전환 후 표시할 대화 설정 (UnkillableBoss → Village 전환 시 사용)
    /// </summary>
    /// <param name="dialogue">표시할 대화 내용</param>
    /// <param name="withFadeIn">true: 검은 화면에서 페이드 인 후 대화 표시</param>
    public void SetPendingDialogue(string dialogue, bool withFadeIn = false)
    {
        pendingDialogue = dialogue;
        shouldFadeInOnLoad = withFadeIn;
        Debug.Log($"💬 PlayerPersistent: Pending dialogue set (fadeIn: {withFadeIn})");
    }

    /// <summary>
    /// Pending dialogue를 딜레이 후 표시 (일반적인 경우)
    /// </summary>
    private System.Collections.IEnumerator ShowPendingDialogueDelayed()
    {
        // 씬 로드 완료 및 UI 초기화 대기
        yield return new WaitForSeconds(1.0f);

        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(pendingDialogue))
        {
            // Split dialogue by newline to support multi-line dialogues
            System.Collections.Generic.List<string> dialogueLines = new System.Collections.Generic.List<string>(
                pendingDialogue.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
            );
            DialogueManager.Instance.StartDialogue(dialogueLines);
            Debug.Log($"💬 PlayerPersistent: Showing pending dialogue ({dialogueLines.Count} lines)");
        }

        // 표시 후 초기화
        pendingDialogue = null;
    }

    /// <summary>
    /// 검은 화면에서 페이드 인한 후 대화 표시 (UnkillableBoss → Village 전환 시)
    /// </summary>
    private System.Collections.IEnumerator FadeInAndShowDialogue()
    {
        // Fade Image 찾기 또는 생성
        UnityEngine.UI.Image fadeImage = FindOrCreateFadeImage();

        if (fadeImage != null)
        {
            // 검은 화면에서 시작
            fadeImage.color = new Color(0, 0, 0, 1);
            fadeImage.gameObject.SetActive(true);

            // 시간 정지 해제 (씬 전환 시 Time.timeScale이 0일 수 있음)
            Time.timeScale = 1f;

            yield return new WaitForSeconds(0.5f); // 잠깐 대기

            // 페이드 인
            float duration = 2f;
            float elapsed = 0f;
            Color startColor = new Color(0, 0, 0, 1);
            Color targetColor = new Color(0, 0, 0, 0);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                fadeImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            fadeImage.color = targetColor;
            fadeImage.gameObject.SetActive(false);

            Debug.Log("☀ PlayerPersistent: Faded in from black");
        }

        // 페이드 인 후 대화 표시
        yield return new WaitForSeconds(0.5f);

        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(pendingDialogue))
        {
            // Split dialogue by newline to support multi-line dialogues
            System.Collections.Generic.List<string> dialogueLines = new System.Collections.Generic.List<string>(
                pendingDialogue.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
            );
            DialogueManager.Instance.StartDialogue(dialogueLines);
            Debug.Log($"💬 PlayerPersistent: Showing dialogue after fade in ({dialogueLines.Count} lines)");
        }

        // 표시 후 초기화
        pendingDialogue = null;
    }

    /// <summary>
    /// Fade Image를 찾거나 생성
    /// </summary>
    private UnityEngine.UI.Image FindOrCreateFadeImage()
    {
        // 기존 Fade Image 찾기
        GameObject fadeObj = GameObject.Find("FadeImage");
        if (fadeObj != null)
        {
            UnityEngine.UI.Image img = fadeObj.GetComponent<UnityEngine.UI.Image>();
            if (img != null) return img;
        }

        // Canvas 찾기 또는 생성
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Fade Image 생성
        fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(canvas.transform, false);

        UnityEngine.UI.Image fadeImage = fadeObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        // 전체 화면 크기
        RectTransform rectTransform = fadeObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        Debug.Log("✅ PlayerPersistent: Created Fade Image");
        return fadeImage;
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Log Player State")]
    private void DebugLogPlayerState()
    {
        Debug.Log($"=== Player State ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Health: {(playerHealth != null ? playerHealth.GetCurrentHealth().ToString() : "N/A")}");
        Debug.Log($"Stamina: {(playerStamina != null ? playerStamina.GetCurrentStamina().ToString("F1") : "N/A")}");
        Debug.Log($"Has saved position: {hasPositionData}");
        Debug.Log($"===================");
    }
#endif
}
