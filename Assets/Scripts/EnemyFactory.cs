// Assets/Scripts/EnemyFactory.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemyFactory : MonoBehaviour
{
    [Header("Prefabs dos Inimigos")]
    public GameObject commonEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;

    [Header("Dados (Flyweight)")]
    public List<EnemyData> enemyDataList;

    private Dictionary<EnemyType, GameObject> prefabMap;
    private Dictionary<EnemyType, EnemyData> dataMap;

    private void Awake()
    {
        prefabMap = new Dictionary<EnemyType, GameObject>
        {
            { EnemyType.Common, commonEnemyPrefab },
            { EnemyType.Fast, fastEnemyPrefab },
            { EnemyType.Tank, tankEnemyPrefab }
        };

        dataMap = new Dictionary<EnemyType, EnemyData>();
        foreach (var data in enemyDataList)
        {
            if (!dataMap.ContainsKey(data.enemyType))
            {
                dataMap.Add(data.enemyType, data);
            }
        }
    }

    public Enemy CreateEnemy(EnemyType type, Vector3 position)
    {
        if (!prefabMap.TryGetValue(type, out GameObject prefab) || !dataMap.TryGetValue(type, out EnemyData data))
        {
            Debug.LogError($"Tipo de inimigo ou dados não encontrados para: {type}");
            return null;
        }

        GameObject enemyInstance = Instantiate(prefab, position, Quaternion.identity);
        Enemy enemyScript = enemyInstance.GetComponent<Enemy>();

        if (enemyScript != null)
        {
            enemyScript.Initialize(data);
        }
        return enemyScript;
    }
}