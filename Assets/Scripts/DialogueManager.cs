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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
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
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        // 큐 초기화 및 채우기
        lines.Clear();
        foreach (var l in dialogueLines) lines.Enqueue(l);

        // UI 표시 및 시간 정지
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        isOpen = true;

        // 이전 timeScale 저장 후 0으로 설정 (대화 중 게임 정지)
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

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
        Time.timeScale = Mathf.Max(0f, previousTimeScale);
    }

    // 외부에서 대화 도중인지 확인할 용도
    public bool IsOpen() => isOpen;
}
// ...existing code...