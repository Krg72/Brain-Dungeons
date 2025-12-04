using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int initialSpawnCount = 5;
    [SerializeField] private SpriteRenderer spawnAreaRenderer;

    public readonly List<Enemy> activeEnemies = new List<Enemy>();
    private Bounds spawnBounds;
    [SerializeField] private float spawnEdgeOffset = 0.5f;

    private void Start()
    {
        if (spawnAreaRenderer == null)
        {
            Debug.LogError("EnemyManager requires a sprite renderer for spawn bounds.");
            enabled = false;
            return;
        }

        spawnBounds = spawnAreaRenderer.bounds;

        for (int i = 0; i < initialSpawnCount; i++)
            SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        Vector2 pos = GetRandomPositionInBounds(spawnBounds);
        Enemy e = Instantiate(enemyPrefab, pos, Quaternion.identity);

        activeEnemies.Add(e);
        e.OnEnemyDied += HandleEnemyDeath;
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        SpawnEnemy();
    }

    private Vector2 GetRandomPositionInBounds(Bounds b)
    {
        float minX = b.min.x + spawnEdgeOffset;
        float maxX = b.max.x - spawnEdgeOffset;

        float minY = b.min.y + spawnEdgeOffset;
        float maxY = b.max.y - spawnEdgeOffset;

        // Clamp in case offset is too large for small sprites
        if (minX > maxX) minX = maxX = b.center.x;
        if (minY > maxY) minY = maxY = b.center.y;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        return new Vector2(x, y);
    }

}
