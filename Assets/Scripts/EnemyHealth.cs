using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Audio;
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int scoreValue = 1; // 죽였을 때 올릴 점수

    private int currentHealth;

    // Event for tutorial or other systems to listen to
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0) Die();
    }

    private void Die()
    {
        Debug.Log($"💀 EnemyHealth: Enemy died: {name}");

        if (OnDeath != null)
        {
            Debug.Log($"🔔 EnemyHealth: OnDeath has {OnDeath.GetInvocationList().Length} subscriber(s). Invoking...");
            OnDeath.Invoke();
        }
        else
        {
            Debug.LogWarning($"⚠️ EnemyHealth: OnDeath event has no subscribers for {name}!");
        }

        GameManager.I?.OnEnemyKilled(scoreValue);
        Destroy(gameObject);
    }
}