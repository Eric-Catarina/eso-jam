// Assets/Scripts/UpgradeData.cs
using UnityEngine;
using System.Collections.Generic; // Necessário para usar List

// Enum para identificar facilmente o tipo de upgrade
public enum UpgradeType
{
    // Player Stats
    PlayerMoveSpeed,
    PlayerAttackSpeed,
    PlayerAttackRange,
    PlayerAttackDamage,
    PlayerDashCooldown,
    
    // Bonfire Stats
    BonfireMaxHealth,
    BonfireBurnRate,
    WoodHealingAmount,
    HealBonfire,

    // Global Modifiers
    WoodDropChance,
    IncreaseHighRarityChance, // Efeito da "Coroa de Midas"

    // Complex/Cursed Effects
    DashCooldownOnKill, // Matar inimigos reduz o cooldown do dash
}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("Configuração")]
    public Rarity rarity; // A raridade deste upgrade
    
    // Um upgrade agora pode ter múltiplos efeitos
    public List<UpgradeEffect> effects = new List<UpgradeEffect>();
}