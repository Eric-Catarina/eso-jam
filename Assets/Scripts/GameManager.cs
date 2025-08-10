// Assets/Scripts/GameManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool isGameRunning = false;
    public float timeToWin = 300, timeToWinFirst = 120; // Tempo para vencer em segundos (2 minutos)
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public PlayerController player;
    public Bonfire bonfire;
    public UpgradeUIManager upgradeUI;
    public EnemySpawner enemySpawner;
    public RarityManager rarityManager; // Referência para o novo manager

    [Header("UI")]
    public GameObject losePanel;
    public GameObject firstWinPanel, winPanel;
    public GameObject startPanel;

    [Header("Controle de Nível")]
    public int playerLevel = 1;
    private int currentXp = 0;
    public int[] xpPerLevel = { 1, 2, 3, 4, 5, 7, 9, 30, 38, 47 }; 

    [Header("Upgrades")]
    [Tooltip("Lista com todos os upgrades possíveis no jogo.")]
    public List<UpgradeData> allUpgrades;
    private List<UpgradeData> offeredUpgradesPool; // Pool de upgrades já oferecidos para evitar repetição imediata

    public ParticleSystem confettiEffect, orangeExplosionEffect, blueExplosionEffect;

    [Header("Stats Globais Modificáveis por Upgrades")]
    public float woodDropRate = 1f;
    public float dashCooldownReductionOnKill = 0f; // Valor do upgrade "Dash On Kill"

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        if (isGameRunning) return;
        isGameRunning = true;
        Time.timeScale = 1f;
        AudioManager.Instance.PlayBackgroundMusic(1);
        
        offeredUpgradesPool = new List<UpgradeData>();

        if (enemySpawner == null)
        {
            Debug.LogError("Referência ao EnemySpawner não foi definida no GameManager!");
            return;
        }
        if (rarityManager == null)
        {
            Debug.LogError("Referência ao RarityManager não foi definida no GameManager!");
            return;
        }
        
        enemySpawner.StartSpawning();
        StartCoroutine(VictoryTimer());
        StartCoroutine(SecondPartTimer());
    }

    private IEnumerator VictoryTimer()
    {
        yield return new WaitForSeconds(timeToWin);
        WinGame();
    }

    private IEnumerator SecondPartTimer()
    {
        yield return new WaitForSeconds(timeToWinFirst);
        FirstPartWin();
    }
    
    // Chamado pelo inimigo ao morrer
    public void OnEnemyKilled()
    {
        if (dashCooldownReductionOnKill > 0)
        {
            player.ReduceDashCooldown(dashCooldownReductionOnKill);
        }
    }

    public void AddXp(int amount)
    {
        if (playerLevel > xpPerLevel.Length) return;

        currentXp += amount;
        Debug.Log($"Ganhou {amount} XP. Total: {currentXp}/{xpPerLevel[playerLevel - 1]}");

        if (currentXp >= xpPerLevel[playerLevel - 1])
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        currentXp -= xpPerLevel[playerLevel - 1];
        playerLevel++;
        ShowConfetti();
        Debug.Log($"LEVEL UP! Novo nível: {playerLevel}");
        AudioManager.Instance.PlaySoundEffect(5);

        Time.timeScale = 0f;

        List<UpgradeData> selectedUpgrades = GetUpgradeChoices();
        upgradeUI.ShowUpgradeOptions(selectedUpgrades);
    }

    private List<UpgradeData> GetUpgradeChoices()
    {
        List<UpgradeData> choices = new List<UpgradeData>();
        
        // Garante que a pool de upgrades não disponíveis não cresça indefinidamente
        // if (offeredUpgradesPool.Count > allUpgrades.Count / 2)
        // {
            offeredUpgradesPool.Clear();
        // }

        for (int i = 0; i < 3; i++) // Oferece 3 opções
        {
            // 1. Sorteia uma raridade
            Rarity targetRarity = rarityManager.SelectRandomRarity();
            Debug.Log($"Escolhendo upgrade de raridade: {targetRarity}");

            // 2. Filtra os upgrades por essa raridade que ainda não foram oferecidos ou escolhidos
            List<UpgradeData> availableByRarity = allUpgrades
                .Where(u => u.rarity == targetRarity && !offeredUpgradesPool.Contains(u))
                .ToList();

            // 3. Se não houver nenhum, tenta uma raridade inferior como fallback
            if (availableByRarity.Count == 0)
            {
                availableByRarity = allUpgrades
                    .Where(u => u.rarity <= targetRarity && !offeredUpgradesPool.Contains(u))
                    .ToList();
            }
            
            // 4. Se ainda assim não houver, limpa o pool e tenta de novo (caso raro)
            if (availableByRarity.Count == 0)
            {
                offeredUpgradesPool.Clear();
                 availableByRarity = allUpgrades.Where(u => u.rarity <= targetRarity).ToList();
            }

            // 5. Escolhe um upgrade aleatório da lista filtrada e adiciona à seleção
            if (availableByRarity.Count > 0)
            {
                UpgradeData chosenUpgrade = availableByRarity[Random.Range(0, availableByRarity.Count)];
                choices.Add(chosenUpgrade);
                offeredUpgradesPool.Add(chosenUpgrade); // Adiciona ao pool para não repetir logo
            }
        }
        return choices;
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        Debug.Log($"Upgrade '{upgrade.upgradeName}' (Raridade: {upgrade.rarity}) selecionado. Aplicando {upgrade.effects.Count} efeito(s).");

        // Itera sobre todos os efeitos do upgrade e os aplica
        foreach (var effect in upgrade.effects)
        {
            switch (effect.type)
            {
                // PLAYER
                case UpgradeType.PlayerAttackSpeed:
                    player.attackCooldown /= effect.value; // Ex: 1.25x speed -> cooldown / 1.25
                    break;
                case UpgradeType.PlayerMoveSpeed:
                    player.moveSpeed *= effect.value;
                    break;
                case UpgradeType.PlayerAttackRange:
                    player.attackRadius *= effect.value;
                    break;
                case UpgradeType.PlayerDashCooldown:
                    player.dashCooldown /= effect.value; // Ex: 0.8x cooldown -> cooldown / 0.8 (melhor)
                    break;
                case UpgradeType.PlayerAttackDamage:
                    player.attackDamage *= effect.value;
                    break;

                // BONFIRE
                case UpgradeType.BonfireBurnRate:
                    bonfire.baseBurnRate *= effect.value; // Ex: 0.9x burn rate (melhor)
                    break;
                case UpgradeType.BonfireMaxHealth:
                    bonfire.IncreaseMaxHealth(effect.value);
                    break;
                case UpgradeType.HealBonfire:
                    bonfire.HealToFull();
                    break;
                case UpgradeType.WoodHealingAmount:
                    bonfire.logHealingAmount += effect.value;
                    break;

                // GLOBAL & COMPLEX
                case UpgradeType.WoodDropChance:
                    woodDropRate *= effect.value;
                    break;
                case UpgradeType.DashCooldownOnKill:
                     // Acumula o valor. Múltiplos upgrades desse tipo somarão seus efeitos.
                    dashCooldownReductionOnKill += effect.value;
                    break;
                case UpgradeType.IncreaseHighRarityChance:
                    rarityManager.highTierChanceMultiplier *= effect.value;
                    break;
            }
        }

        Time.timeScale = 1f;
        upgradeUI.HidePanel();
    }

    // --- MÉTODOS DE CONTROLE DE JOGO E EFEITOS (sem alterações) ---
    #region Game Control & VFX
    public void ShowConfetti()
    {
        if (confettiEffect != null) confettiEffect.Play();
    }

    public void SpawnOrangeExplosion(Vector3 position)
    {
        if (orangeExplosionEffect != null)
        {
            var effect = Instantiate(orangeExplosionEffect, position, Quaternion.identity);
            effect.transform.position += new Vector3(0, 0, -1f);
        }
    }

    public void SpawnOrangeExplosionOnBonfire()
    {
        if (bonfire != null) SpawnOrangeExplosion(bonfire.transform.position);
    }

    public void SpawnBlueExplosion(Vector3 position)
    {
        if (blueExplosionEffect != null)
        {
            var effect = Instantiate(blueExplosionEffect, position, Quaternion.identity);
            effect.transform.position += new Vector3(0, 0, -1f);
        }
    }

    public void WinGame()
    {
        Debug.Log("Você sobreviveu! Vitória!");
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
        DifficultyManager.Instance.StopTimer();
    }

    public void FirstPartWin()
    {
        Debug.Log("Você sobreviveu à primeira parte! Vitória Parcial!");
        if (firstWinPanel != null) firstWinPanel.SetActive(true);
        confettiEffect.Play();
        Time.timeScale = 0f;
        DifficultyManager.Instance.StopTimer();
    }

    public void StartSecondPart()
    {
        Debug.Log("Iniciando a segunda parte do jogo...");
        Time.timeScale = 1f;
        isGameRunning = true;
        DifficultyManager.Instance.ResetTimer();
    }

    public void LoseGame()
    {
        Debug.Log("Você perdeu! Exibindo painel de derrota.");
        losePanel.SetActive(true);
        Time.timeScale = 0f;
        DifficultyManager.Instance.StopTimer();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}