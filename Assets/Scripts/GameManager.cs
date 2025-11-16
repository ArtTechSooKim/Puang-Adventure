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
    [SerializeField] private GameObject gameOverPanel; // optional: 활성화하면 게임오버 UI 보여줌

    [Header("Game State")]
    private int score = 0;
    private bool isGameOver = false;

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

    private void InitializeGame()
    {
        // Ensure game time is running normally at start
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateScoreUI();
        isGameOver = false;
    }

    public void OnPlayerDeath()
    {
        if (isGameOver) return; // Prevent multiple calls

        isGameOver = true;
        Debug.Log("💀 GameManager: Player died - Game Over");

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnEnemyKilled(int value)
    {
        score += Mathf.Max(0, value);
        UpdateScoreUI();
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