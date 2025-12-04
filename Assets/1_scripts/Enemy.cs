using UnityEngine;
using UnityEngine.UI;
using System;

public class Enemy : MonoBehaviour
{
    public event Action<Enemy> OnEnemyDied;

    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private Image healthFill;  // assign the fill Image here (only the bar, not the frame)

    private float currentHealth;

    public bool IsAlive => currentHealth > 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            UpdateHealthBar();
            Die();
            return;
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            float pct = currentHealth / maxHealth;
            healthFill.fillAmount = pct;
        }
    }

    private void Die()
    {
        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }
}
