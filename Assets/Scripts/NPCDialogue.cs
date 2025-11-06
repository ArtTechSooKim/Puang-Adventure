using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogue : MonoBehaviour
{
    [TextArea(2,6)]
    public List<string> dialogueLines = new List<string>();

    [Header("Options")]
    public bool autoOpenOnEnter = false; // 근처 진입 시 자동으로 열지 여부

    private bool playerInRange = false;

    private void Reset()
    {
        // 기본으로 Collider2D(Trigger) 권장
        var col = GetComponent<Collider2D>();
        if (col == null) gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (autoOpenOnEnter)
        {
            DialogueManager.Instance?.StartDialogue(dialogueLines);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
    }

    void Update()
    {
        if (!playerInRange) return;

        // E 키로 대화 시작 (새 Input System)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DialogueManager.Instance?.StartDialogue(dialogueLines);
        }
    }
}