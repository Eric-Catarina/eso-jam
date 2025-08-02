// Assets/Scripts/EnemySpawner.cs
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs dos Inimigos")]
    public GameObject commonEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;

    [Header("Configuração do Spawn")]
    [Tooltip("A taxa de spawn base (inimigos por segundo) no início do jogo.")]
    public float baseSpawnRate = 1f;
    public float spawnRadius = 15f;

    [Header("Chances de Spawn (0.0 a 1.0)")]
    [Range(0f, 1f)]
    public float fastEnemyChance = 0.1f;
    [Range(0f, 1f)]
    public float tankEnemyChance = 0.1f;

    private Transform bonfire;
    private bool isSpawning = false; // Flag para garantir que a rotina não inicie duas vezes

    void Start()
    {
        // Apenas pega a referência, não inicia o spawn
        bonfire = GameObject.FindGameObjectWithTag("Bonfire").transform;
    }

    // Este método é o único ponto de entrada para começar a spawnar
    public void StartSpawning()
    {
        if (isSpawning) return; // Se já estiver spawnando, não faz nada
        isSpawning = true;
        
        Debug.Log("Iniciando a rotina de spawn de inimigos...");
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // Calcula a taxa de spawn atual usando o DifficultyManager
            float currentSpawnRate = baseSpawnRate * DifficultyManager.Instance.SpawnRateMultiplier;
            float waitTime = 1f / currentSpawnRate;
            
            yield return new WaitForSeconds(waitTime);

            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        GameObject prefabToSpawn;
        float randomValue = Random.value;

        if (randomValue < fastEnemyChance) prefabToSpawn = fastEnemyPrefab;
        else if (randomValue < fastEnemyChance + tankEnemyChance) prefabToSpawn = tankEnemyPrefab;
        else prefabToSpawn = commonEnemyPrefab;
        
        if (prefabToSpawn == null) return;

        Vector2 spawnPos = (Vector2)bonfire.position + Random.insideUnitCircle.normalized * spawnRadius;
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}