// ...existing code...
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel; // DialoguePanel (비활성화 상태로 둠)
    [SerializeField] private TextMeshProUGUI dialogueText;   // DialogueText (TextMeshProUGUI)

    private Queue<string> lines = new Queue<string>();
    private bool isOpen = false;

    // 시간 복원용
    private float previousTimeScale = 1f;

    void OnEnable()
    {
        // Subscribe to scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Scene 로드 시 UI 참조 재연결 (DontDestroyOnLoad로 인해 필요)
        RefreshUIReferences();
    }

    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ DialogueManager: Initialized and persisting across scenes");
        }
        else
        {
            Debug.LogWarning("⚠ DialogueManager: Duplicate instance detected - destroying");
            Destroy(gameObject);
            return;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"🔄 DialogueManager: Scene '{scene.name}' loaded, refreshing UI references...");
        RefreshUIReferences();
    }

    void Update()
    {
        if (!isOpen) return;

        // Space로 다음 문장 진행 (새 Input System 사용)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DisplayNextLine();
        }
    }

    public void StartDialogue(List<string> dialogueLines)
    {
        StartDialogue(dialogueLines.ToArray());
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("⚠ DialogueManager.StartDialogue: dialogueLines is null or empty");
            return;
        }

        Debug.Log($"🎬 DialogueManager.StartDialogue called with {dialogueLines.Length} lines");

        // 큐 초기화 및 채우기
        lines.Clear();
        foreach (var l in dialogueLines) lines.Enqueue(l);

        // UI 표시 및 시간 정지
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("✅ DialoguePanel activated");
        }
        else
        {
            Debug.LogError("❌ DialogueManager.StartDialogue: dialoguePanel is NULL!");
            Debug.LogError("   → RefreshUIReferences() may have failed to find DialoguePanel");
        }

        isOpen = true;

        // 이전 timeScale 저장 후 0으로 설정 (대화 중 게임 정지)
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Debug.Log($"⏸ Dialogue started - Time.timeScale: {previousTimeScale} → 0 (saved: {previousTimeScale})");

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        var line = lines.Dequeue();
        if (dialogueText != null) dialogueText.text = line;
    }

    private void EndDialogue()
    {
        isOpen = false;

        // UI 숨김 및 텍스트 클리어
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";

        // 시간 복원 (StartDialogue에서 저장한 값으로)
        // previousTimeScale이 0이면 1로 복원 (정상 게임 진행)
        float restoredTimeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
        Time.timeScale = restoredTimeScale;

        Debug.Log($"💬 Dialogue ended - Time.timeScale restored from {previousTimeScale} to {Time.timeScale}");
    }

    // 외부에서 대화 도중인지 확인할 용도
    public bool IsOpen() => isOpen;

    /// <summary>
    /// Refresh UI references after scene load (for DontDestroyOnLoad compatibility)
    /// </summary>
    public void RefreshUIReferences()
    {
        // Always refresh, even if dialoguePanel exists (it might be destroyed from previous scene)
        // Try to find DialoguePanel in HUD_Canvas first, then Canvas
        GameObject hudCanvas = GameObject.Find("HUD_Canvas");
        GameObject canvas = hudCanvas != null ? hudCanvas : GameObject.Find("Canvas");

        if (canvas != null)
        {
            Transform panelTransform = canvas.transform.Find("DialoguePanel");
            if (panelTransform != null)
            {
                dialoguePanel = panelTransform.gameObject;
                Debug.Log($"✅ DialogueManager: Found DialoguePanel in {canvas.name}");

                // Try to find DialogueText as child
                TextMeshProUGUI text = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    dialogueText = text;
                    Debug.Log("✅ DialogueManager: Found DialogueText in current scene");
                }
                else
                {
                    Debug.LogWarning("⚠ DialogueManager: DialogueText not found in DialoguePanel");
                }
            }
            else
            {
                Debug.LogWarning($"⚠ DialogueManager: DialoguePanel not found in {canvas.name}");
                dialoguePanel = null;
                dialogueText = null;
            }
        }
        else
        {
            Debug.LogWarning("⚠ DialogueManager: Neither HUD_Canvas nor Canvas found in scene");
            dialoguePanel = null;
            dialogueText = null;
        }

        // Ensure panel is hidden initially
        if (dialoguePanel != null && !isOpen)
            dialoguePanel.SetActive(false);
    }
}
// ...existing code...