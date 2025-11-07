// Assets/Scripts/SaveSystemAdapter.cs
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SaveSystemAdapter : MonoBehaviour
{
    public EnemyFactory enemyFactory;
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
    }

    public void SaveGame()
    {
        var player = FindObjectOfType<PlayerController>();
        var bonfire = FindObjectOfType<Bonfire>();
        var enemies = FindObjectsOfType<Enemy>().ToList();

        // Adaptar dados do jogo para um formato serializável
        var playerData = new PlayerSaveData
        {
            position = player.transform.position,
            lenha = player.lenhaNoInventario
        };

        var bonfireData = new BonfireSaveData
        {
            currentHealth = bonfire.currentHealth
        };

        var enemySaveData = new List<EnemySaveData>();
        foreach (var enemy in enemies)
        {
            enemySaveData.Add(enemy.GetSaveData());
        }

        var gameSaveData = new GameSaveData
        {
            playerData = playerData,
            bonfireData = bonfireData,
            enemies = enemySaveData
        };

        string json = JsonUtility.ToJson(gameSaveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Jogo salvo em: {savePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Nenhum jogo salvo encontrado.");
            return;
        }

        string json = File.ReadAllText(savePath);
        GameSaveData gameSaveData = JsonUtility.FromJson<GameSaveData>(json);

        // Limpar a cena atual
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            Destroy(enemy.gameObject);
        }

        // Carregar estado adaptado de volta para os objetos do jogo
        var player = FindObjectOfType<PlayerController>();
        player.transform.position = gameSaveData.playerData.position;
        player.lenhaNoInventario = gameSaveData.playerData.lenha;

        var bonfire = FindObjectOfType<Bonfire>();
        bonfire.currentHealth = gameSaveData.bonfireData.currentHealth;

        foreach (var enemyData in gameSaveData.enemies)
        {
            Enemy newEnemy = enemyFactory.CreateEnemy(enemyData.type, enemyData.position);
            newEnemy.LoadFromSaveData(enemyData);
        }

        Debug.Log("Jogo carregado com sucesso.");
    }
}

// Estruturas de dados para serialização
[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData playerData;
    public BonfireSaveData bonfireData;
    public List<EnemySaveData> enemies;
}

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public int lenha;
}

[System.Serializable]
public class BonfireSaveData
{
    public float currentHealth;
}

[System.Serializable]
public class EnemySaveData
{
    public EnemyType type;
    public Vector3 position;
    public float currentHealth;
}