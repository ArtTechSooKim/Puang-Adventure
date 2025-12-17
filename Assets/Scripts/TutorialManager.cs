using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if CINEMACHINE_INSTALLED
using Cinemachine;
#endif

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private CanvasGroup anyStatePanel;
    [SerializeField] private TextMeshProUGUI anyStateText;
    [SerializeField] private Button skipButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject weaponTier2Prefab;
    [SerializeField] private GameObject batPrefab;

    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private Inventory inventory;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    private MonoBehaviour cinemachineVirtualCamera;
    private Transform originalCameraFollow;
    private Vector3 originalTransposerOffset;

    [Header("Settings")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float defaultFontSize = 50f;
    [SerializeField] private float largeFontSize = 100f;
    [SerializeField] private float smallFontSize = 30f;

    // Tutorial state
    private bool wasdPressed = false;
    private float sprintTime = 0f;
    private bool weaponPickedUp = false;
    private int enemiesKilled = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private GameObject spawnedWeapon = null;

    // AnyState triggers
    private bool hpWarningShown = false;
    private bool staminaWarningShown = false;
     IEnumerator ShowMessage(string message, float displayTime, bool useFade)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;

            // Set font size based on message content
            tutorialText.fontSize = GetFontSizeForMessage(message);
        }

        if (useFade && tutorialPanel != null)
        {
            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(tutorialPanel, 0f, 1f, fadeTime));
        }
        else
        {
            if (tutorialPanel != null)
                tutorialPanel.alpha = 1f;
        }

        if (displayTime > 0)
        {
            // Use WaitForSecondsRealtime when time is paused
            if (Time.timeScale == 0f)
            {
                yield return new WaitForSecondsRealtime(displayTime);
            }
            else
            {
                yield return new WaitForSeconds(displayTime);
            }
        }

        if (useFade && tutorialPanel != null)
        {
            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(tutorialPanel, 1f, 0f, fadeTime));
        }
    }

    float GetFontSizeForMessage(string message)
    {
        // Large font for specific messages
        if (message == "튜토리얼" ||
            message == "좋습니다!" ||
            message == "처치했습니다!" ||
            message == "튜토리얼 완료!" ||
            message == "게임을 시작합니다")
        {
            return largeFontSize; // 100
        }
        // Medium font (50) for messages that wrap
        else if (message == "Shift로 2초간 달리세요." ||
                 message == "다가가서 검을 획득하세요")
        {
            return 50f;
        }
        // Small font for long messages
        else if (message == "검은 반드시 아이템 슬롯 1번에 위치해야 사용할 수 있습니다." ||
                 message == "드래그해서 아이템을 월드에 배치하거나 \n아이템 슬롯을 바꿀 수 있습니다.")
        {
            return smallFontSize; // 30
        }
        // Default font size
        return defaultFontSize; // 50
    }
    void Start()
    {
        // Setup skip button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }

        // Subscribe to enemy item drop events for kill tracking
        EnemyItemDropper.OnItemDropped += OnEnemyKilled;

        // Start initialization coroutine
        StartCoroutine(InitializeTutorial());
    }


    IEnumerator InitializeTutorial()
    {
        // Wait for player to be loaded from DontDestroyOnLoad
        // Try to find player references for up to 5 seconds
        float waitTime = 0f;
        float maxWaitTime = 5f;

        while ((playerController == null || playerHealth == null || playerStamina == null) && waitTime < maxWaitTime)
        {
            // Get references if not assigned
            if (playerController == null)
                playerController = FindObjectOfType<PlayerController>();
            if (playerHealth == null)
                playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerStamina == null)
                playerStamina = FindObjectOfType<PlayerStamina>();

            if (playerController != null && playerHealth != null && playerStamina != null)
                break;

            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        // Check if player was found
        if (playerController == null || playerHealth == null || playerStamina == null)
        {
            Debug.LogError("TutorialManager: Failed to find Player references! Make sure Player is loaded from DontDestroyOnLoad.");
            yield break;
        }

        Debug.Log("✅ TutorialManager: Player references found successfully!");

        // Get other references
        if (inventory == null)
            inventory = Inventory.instance;
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Find and cache Cinemachine Virtual Camera using reflection
        FindCinemachineCamera();

        // Start tutorial
        StartCoroutine(RunTutorial());
        StartCoroutine(MonitorPlayerStats());
    }

    void Update()
    {
        // Check WASD input
        if (!wasdPressed)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                wasdPressed = true;
            }
        }

        // Check sprint time
        if (Input.GetKey(KeyCode.LeftShift))
        {
            sprintTime += Time.deltaTime;
        }

        // Check weapon pickup
        if (!weaponPickedUp && inventory != null)
        {
            for (int i = 0; i < inventory.items.Length; i++)
            {
                if (inventory.items[i] != null && inventory.items[i].isWeapon)
                {
                    weaponPickedUp = true;
                    break;
                }
            }
        }
    }

    IEnumerator RunTutorial()
    {
        // 1. "튜토리얼"
        yield return ShowMessage("튜토리얼", 2f, true);

        // 2. "WASD로 움직이세요."
        yield return ShowMessageUntilCondition("WASD로 움직이세요.", () => wasdPressed);

        // 3. "좋습니다!"
        yield return ShowMessage("좋습니다!", 2f, true);

        // 4. "Shift로 2초간 달리세요."
        sprintTime = 0f;
        yield return ShowMessageUntilCondition("Shift로 2초간 달리세요.", () => sprintTime >= 2f);

        // 5. "좋습니다!"
        yield return ShowMessage("좋습니다!", 2f, true);

        // 6. Spawn weapon and "다가가서 검을 획득하세요"
        // Teleport player to the left of the weapon
        if (playerController != null)
        {
            playerController.transform.position = new Vector3(-5f, 0f, 0f);
        }
        SpawnWeapon(Vector3.zero);
        yield return ShowMessageUntilCondition("다가가서 검을 획득하세요", () => weaponPickedUp);
        // 7. "좋습니다!"
        yield return ShowMessage("좋습니다!", 1f, false);

        // 8. "검은 반드시 아이템 슬롯 1번에 위치해야 사용할 수 있습니다."
        yield return new WaitForSeconds(1f);
        yield return ShowMessage("검은 반드시 아이템 슬롯 1번에 위치해야 사용할 수 있습니다.", 3f, false);

        // 9. "드래그해서 아이템을 월드에 배치하거나 아이템 슬롯을 바꿀 수 있습니다."
        yield return new WaitForSeconds(1.0f);
        yield return ShowMessage("드래그해서 아이템을 월드에 배치하거나 \n아이템 슬롯을 바꿀 수 있습니다.", 3f, true);
        // 10. Teleport player to safe position and spawn enemies
        if (playerController != null)
        {
            playerController.transform.position = new Vector3(0f, 3.5f, 0f);
        }

        Vector3[] enemyPositions = new Vector3[]
        {
            new Vector3(-9, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(9, 0, 0)
        };

        // Spawn all enemies first (while time is still running)
        foreach (var pos in enemyPositions)
        {
            SpawnEnemy(pos);
        }

        // Wait a frame to ensure all enemies are spawned and initialized
        yield return null;

        // Show attack message directly (skip camera work if Cinemachine not found)
        // if (cinemachineVirtualCamera != null)
        // {
        //     // Pause time for camera work
        //     Time.timeScale = 0f;

        //     // Show camera work for each enemy position
        //     foreach (var pos in enemyPositions)
        //     {
        //         yield return StartCoroutine(MoveCameraTo(pos, 1f));
        //     }

        //     // Resume time
        //     Time.timeScale = 1f;
        // }

        yield return ShowMessage("적이 있습니다! 좌클릭으로 공격하세요.", 1f, true);

        // 11. Wait for 3 enemies killed
        Debug.Log($"🎯 TutorialManager: Waiting for 3 enemies to be killed. Current: {enemiesKilled}/3");
        yield return new WaitUntil(() => enemiesKilled >= 3);
        Debug.Log($"✅ TutorialManager: All 3 enemies killed! Proceeding to next step.");

        // 12. "처치했습니다!"
        yield return ShowMessage("처치했습니다!", 2f, true);

        // 13. "tab 토글로 설정 창을 열 수 있습니다."
        yield return ShowMessage("tab 토글로 설정 창을 열 수 있습니다.", 2f, true);

        // 14. "튜토리얼 완료!"
        Time.timeScale = 0f;
        yield return ShowMessage("튜토리얼 완료!", 1f, false);

        // 15. "게임을 시작합니다"
        yield return ShowMessage("게임을 시작합니다", 1f, false);

        // 16. Reset player state and load next scene
        ResetPlayerState();
        Time.timeScale = 1f;
        SceneManager.LoadScene("02_VillageScene");
    }

    IEnumerator MonitorPlayerStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // Restore HP every second
            if (playerHealth != null)
            {
                int currentHP = playerHealth.GetCurrentHealth();
                if (currentHP < 100)
                {
                    playerHealth.Heal(100 - currentHP);
                }

                // Show HP warning if HP drops below 95 for the first time
                if (!hpWarningShown && currentHP <= 95)
                {
                    hpWarningShown = true;
                    StartCoroutine(ShowAnyStateWarning("HP가 0이되면 GameOver이니 주의하세요.", 2f));
                }
            }

            // Check stamina
            if (playerStamina != null && !staminaWarningShown)
            {
                float currentStamina = playerStamina.GetCurrentStamina();
                if (currentStamina <= 50f)
                {
                    staminaWarningShown = true;
                    StartCoroutine(ShowStaminaWarnings());
                }
            }
        }
    }

   

    IEnumerator ShowMessageUntilCondition(string message, System.Func<bool> condition)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;
            // Set font size based on message content
            tutorialText.fontSize = GetFontSizeForMessage(message);
        }

        if (tutorialPanel != null)
        {
            // Fade in and wait for completion
            yield return StartCoroutine(FadeCanvasGroup(tutorialPanel, 0f, 1f, fadeTime));
        }

        // Wait for condition after fade in is complete
        yield return new WaitUntil(condition);

        // Fade out smoothly
        if (tutorialPanel != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(tutorialPanel, 1f, 0f, fadeTime));
        }
    }

    IEnumerator ShowAnyStateWarning(string message, float displayTime)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Hide TutorialPanel while showing AnyStatePanel
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(false);
        }

        if (anyStateText != null)
        {
            anyStateText.text = message;
            anyStateText.fontSize = defaultFontSize; // Use default size for HP warning
        }

        if (tutorialText != null)
        {
            tutorialText.text = message;
            tutorialText.fontSize = defaultFontSize; // Use default size for HP warning
        }
        
        if (anyStatePanel != null)
        {
            anyStatePanel.gameObject.SetActive(true);
            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(anyStatePanel, 0f, 1f, fadeTime));
        }

        // Wait for display time
        yield return new WaitForSecondsRealtime(displayTime);

        if (anyStatePanel != null)
        {
            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(anyStatePanel, 1f, 0f, fadeTime));
            anyStatePanel.gameObject.SetActive(false);
        }

        // Restore TutorialPanel
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
        }

        Time.timeScale = originalTimeScale;
    }

    IEnumerator ShowStaminaWarnings()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Hide TutorialPanel while showing AnyStatePanel
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(false);
        }

        // First message
        if (anyStateText != null)
        {
            anyStateText.text = "달리기와 공격은 스테미나를 소모합니다.";
            anyStateText.fontSize = defaultFontSize;
        }

        if (anyStatePanel != null)
        {
            anyStatePanel.gameObject.SetActive(true);
            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(anyStatePanel, 0f, 1f, fadeTime));
        }

        // Wait for first message display time
        yield return new WaitForSecondsRealtime(2f);

        // Fade out first message
        if (anyStatePanel != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(anyStatePanel, 1f, 0f, fadeTime));
        }

        // Small delay between messages
        yield return new WaitForSecondsRealtime(1.0f);

        // Second message (smaller font for long text)
        if (anyStateText != null)
        {
            anyStateText.text = "스테미나가 소모되지 않는 행동을 하면 자동으로\n조금씩 회복하지만 0이되면 느리게 회복되니 주의하세요!";
            anyStateText.fontSize = smallFontSize; // Use small font for long message
        }

        if (anyStatePanel != null)
        {
            // Fade in second message
            yield return StartCoroutine(FadeCanvasGroup(anyStatePanel, 0f, 1f, fadeTime));
        }

        // Wait for second message display time
        yield return new WaitForSecondsRealtime(2.0f);

        if (anyStatePanel != null)
        {
            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(anyStatePanel, 1f, 0f, fadeTime));
            anyStatePanel.gameObject.SetActive(false);
        }

        // Restore TutorialPanel
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
        }

        Time.timeScale = originalTimeScale;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    void SpawnWeapon(Vector3 position)
    {
        if (weaponTier2Prefab != null)
        {
            spawnedWeapon = Instantiate(weaponTier2Prefab, position, Quaternion.identity);
        }
    }

    void SpawnEnemy(Vector3 position)
    {
        if (batPrefab != null)
        {
            GameObject enemy = Instantiate(batPrefab, position, Quaternion.identity);
            spawnedEnemies.Add(enemy);

            Debug.Log($"✅ TutorialManager: Enemy spawned at {position} (Name: {enemy.name})");
        }
        else
        {
            Debug.LogError("⚠️ TutorialManager: Bat prefab is null!");
        }
    }

    void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"🔥 TutorialManager: Enemy killed via EnemyItemDropper! Total: {enemiesKilled}/3");

        // Force check if condition is met
        if (enemiesKilled >= 3)
        {
            Debug.Log("🎉 TutorialManager: All 3 enemies killed! WaitUntil should complete now.");
        }
    }

    void FindCinemachineCamera()
    {
        Debug.Log("🔍 TutorialManager: Searching for Cinemachine Virtual Camera...");

        // Method 1: Try to find by GameObject name
        GameObject cameraObj = GameObject.Find("Cinemachine");
        if (cameraObj == null)
        {
            cameraObj = GameObject.Find("CM vcam1");
        }
        if (cameraObj == null)
        {
            cameraObj = GameObject.Find("Virtual Camera");
        }

        if (cameraObj != null)
        {
            Debug.Log($"✅ TutorialManager: Found GameObject: {cameraObj.name}");

            // Get all MonoBehaviour components and find Cinemachine one
            MonoBehaviour[] components = cameraObj.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                Debug.Log($"🔍 Component on {cameraObj.name}: {typeName}");

                if (typeName.Contains("CinemachineVirtualCamera"))
                {
                    cinemachineVirtualCamera = comp;
                    Debug.Log($"✅ TutorialManager: Found Cinemachine Virtual Camera component!");
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ TutorialManager: GameObject search failed - no 'Cinemachine', 'CM vcam1', or 'Virtual Camera' found");
            Debug.LogWarning("⚠️ TutorialManager: Listing all GameObjects in scene...");

            // List all root GameObjects in scene
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                Debug.Log($"🔍 Root GameObject: {root.name}");
            }
        }

        // Method 2: Search all MonoBehaviours if Method 1 failed
        if (cinemachineVirtualCamera == null)
        {
            Debug.Log("🔍 TutorialManager: GameObject search failed, trying component search...");
            MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Debug.Log($"🔍 TutorialManager: Found {allComponents.Length} MonoBehaviour components to check");

            int cinemachineComponentCount = 0;
            foreach (var component in allComponents)
            {
                if (component == null) continue;

                string typeName = component.GetType().Name;
                string fullTypeName = component.GetType().FullName;

                // Debug: Print all Cinemachine-related components
                if (typeName.Contains("Cinemachine") || fullTypeName.Contains("Cinemachine"))
                {
                    cinemachineComponentCount++;
                    Debug.Log($"🔍 Found Cinemachine component: GameObject={component.name}, Type={typeName}, Full={fullTypeName}");
                }

                // Check if this is a Cinemachine Virtual Camera
                if (typeName.Contains("CinemachineVirtualCamera") ||
                    typeName == "CinemachineVirtualCamera" ||
                    fullTypeName.Contains("Cinemachine.CinemachineVirtualCamera"))
                {
                    cinemachineVirtualCamera = component;
                    Debug.Log($"🎥 TutorialManager: Found Cinemachine Virtual Camera via component search: {component.name}");
                    break;
                }
            }

            Debug.Log($"🔍 TutorialManager: Total Cinemachine components found: {cinemachineComponentCount}");

            if (cinemachineComponentCount == 0)
            {
                Debug.LogError("❌ TutorialManager: NO Cinemachine components found in scene at all!");
                Debug.LogError("❌ This means Cinemachine is either not installed or no Virtual Camera exists in TutorialScene");
            }
        }

        // Store original settings if camera was found
        if (cinemachineVirtualCamera != null)
        {
            // Store original Follow target
            var followProperty = cinemachineVirtualCamera.GetType().GetProperty("Follow");
            if (followProperty != null && followProperty.CanRead)
            {
                originalCameraFollow = followProperty.GetValue(cinemachineVirtualCamera) as Transform;
                Debug.Log($"✅ TutorialManager: Original Follow target: {originalCameraFollow?.name ?? "null"}");
            }

            // Get the Transposer component and store original offset
            var getComponentCinemachineMethod = cinemachineVirtualCamera.GetType().GetMethod("GetCinemachineComponent");
            if (getComponentCinemachineMethod != null)
            {
                var transposerType = cinemachineVirtualCamera.GetType().Assembly.GetType("Cinemachine.CinemachineTransposer");
                if (transposerType != null)
                {
                    var transposer = getComponentCinemachineMethod.Invoke(cinemachineVirtualCamera, new object[] { transposerType });
                    if (transposer != null)
                    {
                        var offsetProperty = transposerType.GetField("m_FollowOffset");
                        if (offsetProperty != null)
                        {
                            originalTransposerOffset = (Vector3)offsetProperty.GetValue(transposer);
                            Debug.Log($"✅ TutorialManager: Stored original Transposer offset: {originalTransposerOffset}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ TutorialManager: No Transposer component found on Virtual Camera");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ TutorialManager: Cinemachine Virtual Camera not found! Camera work will not function.");
            Debug.LogWarning("⚠️ TutorialManager: Make sure TutorialScene has a Cinemachine Virtual Camera in the scene.");
            Debug.LogWarning("⚠️ TutorialManager: Try naming it 'Cinemachine', 'CM vcam1', or 'Virtual Camera'");
        }
    }

    IEnumerator MoveCameraTo(Vector3 targetPosition, float duration)
    {
        if (cinemachineVirtualCamera == null)
        {
            Debug.LogWarning("⚠️ TutorialManager: Cannot move camera - Cinemachine Virtual Camera not found");
            yield return new WaitForSecondsRealtime(duration + 0.5f); // Wait equivalent time
            yield break;
        }

        // Get the Transposer component using reflection
        var getComponentCinemachineMethod = cinemachineVirtualCamera.GetType().GetMethod("GetCinemachineComponent");
        if (getComponentCinemachineMethod == null)
        {
            Debug.LogWarning("⚠️ TutorialManager: Cannot access Cinemachine components");
            yield break;
        }

        var transposerType = cinemachineVirtualCamera.GetType().Assembly.GetType("Cinemachine.CinemachineTransposer");
        if (transposerType == null)
        {
            Debug.LogWarning("⚠️ TutorialManager: Cannot find CinemachineTransposer type");
            yield break;
        }

        var transposer = getComponentCinemachineMethod.Invoke(cinemachineVirtualCamera, new object[] { transposerType });
        if (transposer == null)
        {
            Debug.LogWarning("⚠️ TutorialManager: Virtual Camera has no Transposer component");
            yield break;
        }

        var offsetField = transposerType.GetField("m_FollowOffset");
        if (offsetField == null)
        {
            Debug.LogWarning("⚠️ TutorialManager: Cannot access m_FollowOffset field");
            yield break;
        }

        // Calculate offset to look at target position from current Follow target
        var followProperty = cinemachineVirtualCamera.GetType().GetProperty("Follow");
        Transform currentFollow = followProperty?.GetValue(cinemachineVirtualCamera) as Transform;

        if (currentFollow == null)
        {
            Debug.LogWarning("⚠️ TutorialManager: No Follow target set on Virtual Camera");
            yield break;
        }

        // Calculate new offset: targetPosition - currentFollowPosition
        Vector3 startOffset = (Vector3)offsetField.GetValue(transposer);
        Vector3 endOffset = targetPosition - currentFollow.position;
        endOffset.z = startOffset.z; // Keep original Z offset

        Debug.Log($"🎥 TutorialManager: Moving camera to {targetPosition}, offset from {startOffset} to {endOffset}");

        // Lerp the offset over duration
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            Vector3 currentOffset = Vector3.Lerp(startOffset, endOffset, t);
            offsetField.SetValue(transposer, currentOffset);
            yield return null;
        }

        offsetField.SetValue(transposer, endOffset);

        // Wait a bit to show the position
        yield return new WaitForSecondsRealtime(0.5f);

        // Return to original offset
        Debug.Log($"🎥 TutorialManager: Returning camera to original offset: {originalTransposerOffset}");
        elapsed = 0f;
        startOffset = endOffset;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            Vector3 currentOffset = Vector3.Lerp(startOffset, originalTransposerOffset, t);
            offsetField.SetValue(transposer, currentOffset);
            yield return null;
        }

        offsetField.SetValue(transposer, originalTransposerOffset);
    }

    void SkipTutorial()
    {
        StopAllCoroutines();
        ResetPlayerState();
        Time.timeScale = 1f;
        SceneManager.LoadScene("02_VillageScene");
    }

    void ResetPlayerState()
    {
        // Reset inventory - clear all items
        if (inventory != null)
        {
            for (int i = 0; i < inventory.items.Length; i++)
            {
                inventory.items[i] = null;
            }

            // Refresh UI manually since RefreshUI() is private
            if (Hotbar.instance != null)
            {
                Hotbar.instance.UpdateUI();
            }

            var inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.UpdateUI();
            }

            Debug.Log("✅ TutorialManager: Inventory cleared");
        }

        // Reset player health to max
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
            Debug.Log("✅ TutorialManager: Player health reset");
        }

        // Reset player stamina to max
        if (playerStamina != null)
        {
            playerStamina.ResetStamina();
            Debug.Log("✅ TutorialManager: Player stamina reset");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from static event to prevent memory leaks
        EnemyItemDropper.OnItemDropped -= OnEnemyKilled;

        // Cleanup
        if (spawnedWeapon != null)
            Destroy(spawnedWeapon);

        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        // Ensure time is restored
        Time.timeScale = 1f;
    }
}
