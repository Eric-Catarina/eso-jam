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
    CritChance,             // Aumenta a chance de acerto crítico
    CritDamage,             // Aumenta o dano do acerto crítico
    LifeSteal,              // Rouba vida ao causar dano
    ExplosionOnKill,        // Causa uma explosão ao matar um inimigo
    DashDealsDamage,        // O dash causa dano e atravessa inimigos
    SlowOnHit,              // Ataques aplicam lentidão
    BurnOnHit,              // Ataques aplicam dano de queimadura ao longo do tempo
    BonfirePulseDamage,     // Fogueira pulsa dano em área
    GoldPlatedEnemies,      // Inimigos têm mais chance de dropar Cerne-Chama, mas são mais fortes
    // ProjectilePierce,       // O Cerne-Chama arremessado atravessa inimigos
    PlayerHealthRegen,      // Regeneração passiva de vida do jogador
    DamageAuraPlayer,       // Dano em área constante ao redor do jogador
    EnemySpawnDelay,        // Aumenta o tempo entre spawns de inimigos
    HealthForSpeed,         // Troca vida máxima por velocidade de movimento
    AttackDamagePerMissingHealth, // Aumenta dano com base na vida perdida do jogador
    BonfireShield,          // Fogueira recebe escudo temporário
    ExplosionChanceOnHit,   // Chance de explosão no impacto do ataque
    DamageOnDashEnd,        // Causa dano em área ao final do dash
    BonfireHealOnPlayerKill,
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