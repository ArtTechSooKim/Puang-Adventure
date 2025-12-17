using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameOver Canvas 컨트롤러
/// 플레이어가 사망하면 GameOver UI를 표시하고 Space로 타이틀로 복귀
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("GameOver Canvas 그룹 (페이드 인 효과용)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("'Press SPACE to return to Title' 텍스트")]
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Settings")]
    [Tooltip("타이틀 씬 이름 (ReturnToTitle 선택 시)")]
    [SerializeField] private string titleSceneName = "00_TitleScene";

    [Tooltip("페이드 인 시간 (초)")]
    [SerializeField] private float fadeInDuration = 1.5f;

    [Tooltip("Space 키 입력을 허용하기 전 대기 시간 (초)")]
    [SerializeField] private float inputDelay = 1f;

    [Tooltip("텍스트 깜빡임 속도")]
    [SerializeField] private float blinkSpeed = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = false;

    private bool canReturnToTitle = false;
    private float blinkTimer = 0f;

    private void Start()
    {
        // 초기 상태: 완전 투명
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        // GameOver 표시 시작
        StartCoroutine(ShowGameOverSequence());
    }

    private void Update()
    {
        // Space 키 입력 감지
        if (canReturnToTitle && Input.GetKeyDown(KeyCode.Space))
        {
            ReturnToTitle();
        }

        // 텍스트 깜빡임 효과
        if (canReturnToTitle && instructionText != null)
        {
            blinkTimer += Time.unscaledDeltaTime;
            float alpha = Mathf.PingPong(blinkTimer / blinkSpeed, 1f);
            Color color = instructionText.color;
            color.a = alpha;
            instructionText.color = color;
        }
    }

    /// <summary>
    /// GameOver 화면 표시 시퀀스
    /// </summary>
    private System.Collections.IEnumerator ShowGameOverSequence()
    {
        if (showDebugMessages)
            Debug.Log("💀 GameOverController: 게임오버 화면 표시 시작");

        // 게임 시간 정지 (이미 정지되어 있을 수 있지만 확인)
        Time.timeScale = 0f;

        // 페이드 인
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaledDeltaTime 사용 (timeScale 0이어도 동작)
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            }
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // 입력 대기
        yield return new WaitForSecondsRealtime(inputDelay);

        // Space 키 입력 허용
        canReturnToTitle = true;

        if (showDebugMessages)
            Debug.Log("✅ GameOverController: Space 키 입력 활성화");
    }

    /// <summary>
    /// 타이틀 화면으로 복귀
    /// </summary>
    private void ReturnToTitle()
    {
        if (showDebugMessages)
            Debug.Log($"🌀 GameOverController: 타이틀로 복귀 ({titleSceneName})");

        // 게임 시간 복원
        Time.timeScale = 1f;

        // 🔊 버튼 클릭 사운드 재생
        if (AudioManager.I != null)
        {
            AudioManager.I.PlayUIClickSound();
        }

        // 플레이어 상태 초기화 (선택사항)
        // PlayerPersistent에서 처리하거나 씬 로드 시 자동 초기화됨

        // 타이틀 씬으로 이동
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// 공개 메서드: GameManager에서 호출하여 GameOver 화면 활성화
    /// </summary>
    public void ShowGameOver()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 디버그: 강제로 GameOver 화면 표시
    /// </summary>
    [ContextMenu("Debug: Show GameOver")]
    private void DebugShowGameOver()
    {
        ShowGameOver();
    }
}
