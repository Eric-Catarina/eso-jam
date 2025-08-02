// Assets/Scripts/UpgradeData.cs
using UnityEngine;

// Enum para identificar facilmente o tipo de upgrade
public enum UpgradeType
{
    WoodDropChance,
    PlayerAttackSpeed,
    PlayerAttackRange,
    BonfireBurnRate,
    BonfireMaxHealth,
    HealBonfire,
    PlayerMoveSpeed,
    PlayerDashCooldown,
    PlayerAttackDamage,
    WoodHealingAmount,
}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Configuração")]
    public UpgradeType type;
    public float value; // O valor do upgrade (ex: 1.15 para 15% de chance)
}