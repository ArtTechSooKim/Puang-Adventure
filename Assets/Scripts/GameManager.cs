using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Audio;

/// <summary>
/// Central game manager that persists across scenes.
/// Manages game state, score, and coordinates with other persistent managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("UI References")]
    [SerializeField] private Slider playerHpSlider; // optional: 자동 연결 원하면 PlayerHealth에서 drag
    [SerializeField] private Text scoreText; // optional
    [SerializeField] private GameObject gameOverCanvas; // GameOverCanvas Prefab (DontDestroyOnLoad)

    [Header("Game State")]
    private int score = 0;
    private bool isGameOver = false;

    // GameOverCanvas 인스턴스 (런타임 생성)
    private GameObject gameOverCanvasInstance;
    private GameOverController gameOverController;

    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameManager: Initialized and persisting across scenes");
        }
        else
        {
            Debug.LogWarning("⚠ GameManager: Duplicate instance detected - destroying");
            Destroy(gameObject);
            return;
        }

        InitializeGame();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬 로드 시 호출 - TutorialScene/TitleScene에서 GameOver 상태 초기화
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // TutorialScene 또는 TitleScene에서 게임 상태 초기화
        if (scene.name == "00_TitleScene" || scene.name == "01_TutorialScene")
        {
            ResetGameOverState();
        }
    }

    /// <summary>
    /// GameOver 상태 초기화 (게임 재시작)
    /// </summary>
    private void ResetGameOverState()
    {
        if (isGameOver)
        {
            isGameOver = false;
            Time.timeScale = 1f;

            // GameOverCanvas 비활성화
            if (gameOverCanvasInstance != null)
            {
                gameOverCanvasInstance.SetActive(false);
            }

            // 플레이어 상태 복구 (PlayerHealth에서 비활성화된 것들 재활성화)
            ResetPlayerState();

            Debug.Log("🔄 GameManager: GameOver 상태 초기화됨 (TutorialScene/TitleScene)");
        }
    }

    /// <summary>
    /// 플레이어 상태 복구 (사망 시 비활성화된 컴포넌트들 재활성화)
    /// </summary>
    private void ResetPlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠ GameManager: Player를 찾을 수 없습니다.");
            return;
        }

        // PlayerController 재활성화
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && !playerController.enabled)
        {
            playerController.enabled = true;
            Debug.Log("✅ GameManager: PlayerController 재활성화");
        }

        // PlayerHealth 재활성화 및 상태 초기화
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.enabled)
        {
            playerHealth.enabled = true;
            Debug.Log("✅ GameManager: PlayerHealth 재활성화");
        }

        // Rigidbody2D 복구 (Static → Dynamic)
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType == RigidbodyType2D.Static)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Debug.Log("✅ GameManager: Rigidbody2D 복구 (Dynamic)");
        }

        // Animator 초기화
        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsWalking", false);
            anim.Rebind(); // 애니메이터 상태 초기화
            Debug.Log("✅ GameManager: Animator 초기화");
        }
    }

    private void InitializeGame()
    {
        // Ensure game time is running normally at start
        Time.timeScale = 1f;

        UpdateScoreUI();
        isGameOver = false;

        // GameOverCanvas 인스턴스 생성 (아직 없으면)
        if (gameOverCanvas != null && gameOverCanvasInstance == null)
        {
            gameOverCanvasInstance = Instantiate(gameOverCanvas);
            DontDestroyOnLoad(gameOverCanvasInstance);
            gameOverController = gameOverCanvasInstance.GetComponent<GameOverController>();

            // 초기에는 비활성화
            gameOverCanvasInstance.SetActive(false);

            Debug.Log("✅ GameManager: GameOverCanvas 인스턴스 생성됨 (DontDestroyOnLoad)");
        }
    }

    public void OnPlayerDeath()
    {
        if (isGameOver) return; // Prevent multiple calls

        // UnkillableBossScene에서는 GameOver 처리하지 않음
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "05_UnkillableBossScene")
        {
            Debug.Log("⚠ GameManager: OnPlayerDeath 호출되었지만 UnkillableBossScene에서는 무시됨");
            return;
        }

        isGameOver = true;
        Debug.Log("💀 GameManager: Player died - Game Over");

        // GameOverCanvas 활성화
        if (gameOverCanvasInstance != null)
        {
            gameOverCanvasInstance.SetActive(true);

            if (gameOverController != null)
            {
                gameOverController.ShowGameOver();
            }
        }
        else
        {
            Debug.LogWarning("⚠ GameManager: GameOverCanvas 인스턴스가 없습니다!");
        }

        // 게임 시간 정지 (GameOverController에서도 처리하지만 확실하게)
        Time.timeScale = 0f;

        // 🔊 게임오버 사운드 재생 (선택사항)
        if (AudioManager.I != null)
        {
            AudioManager.I.PlayGameOverSound();
        }
    }

    public void OnEnemyKilled(int value)
    {
        score += Mathf.Max(0, value);
        UpdateScoreUI();
    }

    /// <summary>
    /// Check if game is currently in GameOver state
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    // 유틸: 재시작/종료 버튼에서 호출
    public void Restart()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        score = 0;
        UpdateScoreUI();

        // Reset player state if available
        if (PlayerPersistent.Instance != null)
        {
            PlayerPersistent.Instance.ResetPlayerState();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Debug.Log("🔄 GameManager: Game restarted");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}