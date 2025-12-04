using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private float clickRadius = 1.5f;
    [SerializeField] private float enemyDamage = 3f;
    [SerializeField] private float selfDamage = 1f;
    [SerializeField] private float maxHealth = 20f;

    [SerializeField] private float attackRate = 0.25f;

    [Header("UI")]
    [SerializeField] private Image attackBar;
    // This is now a CHILD of the click visual object.

    private float currentHealth;
    private float nextAttackTime = 0f;

    private EnemyManager enemyManager;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();
        UpdateAttackUI(1);  // start full
    }

    private void Update()
    {
        UpdateAttackCooldownVisual();

        if (Input.GetMouseButtonDown(0))
            TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackRate;

        Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        DamageEnemies(clickPos);
        ApplySelfDamage();
    }

    private void DamageEnemies(Vector2 clickPos)
    {
        if (enemyManager == null) return;

        float sqrRadius = clickRadius * clickRadius;

        for (int i = enemyManager.activeEnemies.Count - 1; i >= 0; i--)
        {
            Enemy e = enemyManager.activeEnemies[i];
            if (e == null || !e.IsAlive) continue;

            float sqrDist = ((Vector2)e.transform.position - clickPos).sqrMagnitude;

            if (sqrDist <= sqrRadius)
                e.TakeDamage(enemyDamage);
        }
    }

    private void ApplySelfDamage()
    {
        currentHealth -= selfDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
            Debug.Log("Player died.");
    }

    private void UpdateAttackCooldownVisual()
    {
        if (attackBar == null) return;

        float elapsed = Mathf.Clamp(Time.time - (nextAttackTime - attackRate), 0f, attackRate);
        float pct = (attackRate == 0) ? 1f : elapsed / attackRate;

        UpdateAttackUI(pct);
    }

    private void UpdateAttackUI(float pct)
    {
        if (attackBar != null)
            attackBar.transform.localScale = new Vector3(pct, pct, pct);
    }
}
