// Assets/Scripts/GameManager.cs
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public PlayerController player;
    public Bonfire bonfire;
    public UpgradeUIManager upgradeUI; // Vamos criar este script a seguir

    [Header("Controle de Nível")]
    public int playerLevel = 1;
    private int currentXp = 0;
    // Custo de XP para cada nível (Nível 1 custa 1, Nível 2 custa 3, etc.)
    public int[] xpPerLevel = { 1, 3, 5, 8, 12, 17, 23, 30, 38, 47 }; // Curva até o Nível 10

    [Header("Upgrades")]
    [Tooltip("Lista com todos os upgrades possíveis no jogo.")]
    public List<UpgradeData> allUpgrades;
    private List<UpgradeData> availableUpgrades;

    public ParticleSystem confettiEffect, orangeExplosionEffect, blueExplosionEffect;
    // Stats Globais Modificáveis
    public float bonusWoodDropChance = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Reseta a lista de upgrades disponíveis no início
        availableUpgrades = new List<UpgradeData>(allUpgrades);
    }


    public void AddXp(int amount)
    {
        if (playerLevel > xpPerLevel.Length) return; // Nível máximo atingido

        currentXp += amount;
        Debug.Log($"Ganhou {amount} XP. Total: {currentXp}/{xpPerLevel[playerLevel - 1]}");

        // Verifica se alcançou o XP necessário para o próximo nível
        if (currentXp >= xpPerLevel[playerLevel - 1])
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Subtrai o custo do nível atual
        currentXp -= xpPerLevel[playerLevel - 1];
        playerLevel++;
        ShowConfetti();
        Debug.Log($"LEVEL UP! Novo nível: {playerLevel}");

        Time.timeScale = 0f; // Pausa o jogo

        // Seleciona 3 upgrades aleatórios
        List<UpgradeData> selectedUpgrades = new List<UpgradeData>();
        var randomUpgrades = availableUpgrades.OrderBy(x => Random.value).ToList();

        int count = Mathf.Min(3, randomUpgrades.Count);
        for (int i = 0; i < count; i++)
        {
            selectedUpgrades.Add(randomUpgrades[i]);
        }

        // Mostra a tela de upgrade
        upgradeUI.ShowUpgradeOptions(selectedUpgrades);
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        Debug.Log($"Upgrade selecionado: {upgrade.upgradeName}");
        switch (upgrade.type)
        {
            case UpgradeType.WoodDropChance:
                bonusWoodDropChance += upgrade.value; // Acumula a chance bônus
                break;
            case UpgradeType.PlayerAttackSpeed:
                player.attackCooldown *= (1f / upgrade.value); // Ex: 1.25x speed -> cooldown * (1/1.25)
                break;
            case UpgradeType.BonfireBurnRate:
                bonfire.taxaDePerdaDeEnergia *= upgrade.value; // Ex: 0.9x burn rate
                break;
            case UpgradeType.BonfireMaxHealth:
                bonfire.IncreaseMaxHealth(upgrade.value);
                break;
            case UpgradeType.HealBonfire:
                bonfire.HealToFull();
                break;
            case UpgradeType.PlayerMoveSpeed:
                player.moveSpeed *= upgrade.value;
                break;
        }

        // Remove o upgrade da lista para não ser oferecido de novo (opcional, mas bom)
        // availableUpgrades.Remove(upgrade);

        // Despausa o jogo
        Time.timeScale = 1f;
        upgradeUI.HidePanel();

    }

    public void ShowConfetti()
    {
        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }
    }

    public void SpawnOrangeExplosion(Vector3 position)
    {
        if (orangeExplosionEffect != null)
        {
            var effect = Instantiate(orangeExplosionEffect, position, Quaternion.identity);
            // move a little in z
            effect.transform.position += new Vector3(0, 0, -1f);
        }
    }
    
    public void SpawnOrangeExplosionOnBonfire()
    {
        if (bonfire != null)
        {
            SpawnOrangeExplosion(bonfire.transform.position);
        }
    }

    public void SpawnBlueExplosion(Vector3 position)
    {
        if (blueExplosionEffect != null)
        {
            var effect = Instantiate(blueExplosionEffect, position, Quaternion.identity);
            // move a little in z
            effect.transform.position += new Vector3(0, 0, -1f);

        }
    }

}