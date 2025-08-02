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
        player = FindObjectOfType<PlayerController>().transform; // Encontra o jogador
        bonfire = GameObject.FindGameObjectWithTag("Bonfire").transform; // Encontra a fogueira
        InvokeRepeating("SpawnEnemy", 1f, 1f / spawnRate);
    }

    void SpawnEnemy()
    {
        // Pega uma direção aleatória e a multiplica pelo raio
        Vector2 spawnPos = bonfire.position;
        spawnPos += Random.insideUnitCircle.normalized * spawnRadius;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}