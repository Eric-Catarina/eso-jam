// Assets/Scripts/EnemySpawner.cs
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Referências")]
    public EnemyFactory enemyFactory;

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
    private bool isSpawning = false;

    void Start()
    {
        bonfire = GameObject.FindGameObjectWithTag("Bonfire").transform;
        if (enemyFactory == null)
        {
            Debug.LogError("EnemyFactory não está atribuída no EnemySpawner!");
            this.enabled = false;
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;
        isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            float currentSpawnRate = baseSpawnRate * DifficultyManager.Instance.SpawnRateMultiplier;
            float waitTime = 1f / currentSpawnRate;
            yield return new WaitForSeconds(waitTime);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        EnemyType typeToSpawn;
        float randomValue = Random.value;

        if (randomValue < fastEnemyChance) typeToSpawn = EnemyType.Fast;
        else if (randomValue < fastEnemyChance + tankEnemyChance) typeToSpawn = EnemyType.Tank;
        else typeToSpawn = EnemyType.Common;
        
        Vector2 spawnPos = (Vector2)bonfire.position + Random.insideUnitCircle.normalized * spawnRadius;
        enemyFactory.CreateEnemy(typeToSpawn, spawnPos);
    }
}