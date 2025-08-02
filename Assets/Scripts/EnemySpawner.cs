// Assets/Scripts/EnemySpawner.cs
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs dos Inimigos")]
    public GameObject commonEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;

    [Header("Configuração do Spawn")]
    public float spawnRate = 2f;
    public float spawnRadius = 15f;

    [Header("Chances de Spawn (0.0 a 1.0)")]
    [Range(0f, 1f)]
    public float fastEnemyChance = 0.1f; // 10%
    [Range(0f, 1f)]
    public float tankEnemyChance = 0.1f; // 10%

    private Transform bonfire;

    void Start()
    {
        bonfire = GameObject.FindGameObjectWithTag("Bonfire").transform;
        // InvokeRepeating("SpawnEnemy", 2f, 1f / spawnRate);
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn;
        float randomValue = Random.value; // Gera um número entre 0.0 e 1.0

        if (randomValue < fastEnemyChance)
        {
            // Chance de 0% a 10% -> Spawn Inimigo Rápido
            prefabToSpawn = fastEnemyPrefab;
        }
        else if (randomValue < fastEnemyChance + tankEnemyChance)
        {
            // Chance de 10% a 20% -> Spawn Inimigo Tanque
            prefabToSpawn = tankEnemyPrefab;
        }
        else
        {
            // Restante (80%) -> Spawn Inimigo Comum
            prefabToSpawn = commonEnemyPrefab;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Um dos prefabs de inimigo não foi atribuído no Spawner!");
            return;
        }

        // Lógica para spawnar em um círculo ao redor da fogueira
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle.normalized * spawnRadius;
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
    public void StartSpawning()
    {
        CancelInvoke("SpawnEnemy");
        InvokeRepeating("SpawnEnemy", 1f, 1f / spawnRate);
    }
}

