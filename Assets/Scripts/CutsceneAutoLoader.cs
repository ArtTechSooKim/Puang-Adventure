using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 컷씬 Scene에서 자동으로 대화를 재생하고, 종료 후 다음 Scene으로 이동합니다.
/// PeuangSadScene: 컷씬 재생 → UnkillableBossScene 자동 이동
/// </summary>
public class CutsceneAutoLoader : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private string[] cutsceneDialogues = new string[]
    {
        "\"저 거대 버섯 고기가 그렇게 맛있다던데.. 푸앙이는 힘이 없어 사냥도 못한다 퓨앙!\"",
        "\"저게 푸앙이가 원하던 거대 버섯...! 재빨리 해치우자.\""
    };

    [SerializeField] private string nextSceneName = "06_UnkillableBossScene";
    [SerializeField] private float dialogueWaitTime = 3f; // 각 대화 표시 시간
    [SerializeField] private float transitionDelay = 1f;  // 전환 전 대기 시간
    [SerializeField] private bool showDebugMessages = true;

    private void Start()
    {
        if (showDebugMessages)
            Debug.Log("🎬 CutsceneAutoLoader: Starting cutscene...");

        // 컷씬 자동 재생
        StartCoroutine(PlayCutsceneAndTransition());
    }

    /// <summary>
    /// 컷씬 재생 및 다음 Scene으로 전환
    /// </summary>
    private IEnumerator PlayCutsceneAndTransition()
    {
        // Stage 확인 및 진행
        if (QuestManager.Instance != null)
        {
            QuestStage currentStage = QuestManager.Instance.GetCurrentStage();

            if (showDebugMessages)
                Debug.Log($"🎬 Current Stage: {currentStage}");

            // Stage4가 아니면 자동으로 Stage4로 설정
            if (currentStage == QuestStage.Stage3_CaveExploration)
            {
                QuestManager.Instance.AdvanceStage(); // Stage3 → Stage4
                if (showDebugMessages)
                    Debug.Log("📈 Advanced to Stage4_PeuangSadCutscene");
            }
        }

        // 대화 재생
        if (DialogueManager.Instance != null)
        {
            foreach (string dialogue in cutsceneDialogues)
            {
                DialogueManager.Instance.StartDialogue(new System.Collections.Generic.List<string> { dialogue });

                if (showDebugMessages)
                    Debug.Log($"💬 Playing dialogue: {dialogue}");

                yield return new WaitForSeconds(dialogueWaitTime);
            }
        }
        else
        {
            Debug.LogWarning("⚠ DialogueManager not found! Skipping dialogue.");
            yield return new WaitForSeconds(dialogueWaitTime * cutsceneDialogues.Length);
        }

        // 전환 전 대기
        yield return new WaitForSeconds(transitionDelay);

        // 다음 Scene으로 이동
        if (showDebugMessages)
            Debug.Log($"🌀 Transitioning to: {nextSceneName}");

        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// 디버그: 즉시 다음 Scene으로 이동
    /// </summary>
    [ContextMenu("Debug: Skip Cutscene")]
    private void DebugSkipCutscene()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(nextSceneName);
    }
}
