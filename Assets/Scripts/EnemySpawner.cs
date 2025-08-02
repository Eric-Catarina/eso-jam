// EnemySpawner.cs
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f; // Inimigos por segundo
    public float spawnRadius = 15f; // Distância do jogador onde os inimigos aparecem

    private Transform player;
    private Transform bonfire;

    void Start()
    {
    }

    void SpawnEnemy()
    {
        // Pega uma direção aleatória e a multiplica pelo raio
        Vector2 spawnPos = transform.position;
        spawnPos += Random.insideUnitCircle.normalized * spawnRadius;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    public void StartSpawning()
    {
        CancelInvoke("SpawnEnemy");
        InvokeRepeating("SpawnEnemy", 1f, 1f / spawnRate);
    }
}