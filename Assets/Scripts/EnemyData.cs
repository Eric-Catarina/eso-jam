// Assets/Scripts/EnemyData.cs
using UnityEngine;

public enum EnemyType { Common, Fast, Tank }

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identificação")]
    public EnemyType enemyType;

    [Header("Atributos Base")]
    public float baseSpeed = 3f;
    public float baseMaxHealth = 2;
    public float dano = 4;
    public float baseWoodToDrop = 0.5f;

    [Header("Configuração de Movimento Específico")]
    // Comum
    public float swaySpeed = 2f;
    public float swayAmount = 1.5f;
    // Rápido
    public float spiralFrequency = 3f;
    public float spiralMagnitude = 0.8f;
}