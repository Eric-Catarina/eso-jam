// Assets/Scripts/GameManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private bool isGameRunning = false;
    public float timeToWin = 300, timeToWinFirst = 120;
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public PlayerController player;
    public Bonfire bonfire;
    public UpgradeUIManager upgradeUI;
    public EnemySpawner enemySpawner;
    public RarityManager rarityManager;
    public SaveSystemAdapter saveSystem; // << NOVO: Referência ao Adapter

    [Header("UI")]
    public GameObject losePanel;
    public GameObject firstWinPanel, winPanel;
    public GameObject startPanel;
    public TextMeshProUGUI levelText;
    public Slider xpSlider;

    [Header("Controle de Nível")]
    public int playerLevel = 1;
    private float currentXp = 0f;
    public int[] xpPerLevel = { 1, 2, 3, 4, 5, 7, 9, 30, 38, 47 };

    [Header("Upgrades")]
    public List<UpgradeData> allUpgrades;
    private List<UpgradeData> offeredUpgradesPool;
    public GameObject woodDropPrefab;

    public ParticleSystem confettiEffect, orangeExplosionEffect, blueExplosionEffect;

    [Header("Stats Globais")]
    public float woodDropMultiplier = 1f;
    public float dashCooldownReductionOnKill = 0f;
    private bool godMode = false;

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
        
        GameEvents.OnWoodCollected += AddXp;
        GameEvents.OnEnemyKilled += HandleEnemyKill;
        GameEvents.OnGameOver += HandleGameOver;
    }

    private void OnDestroy()
    {
        GameEvents.OnWoodCollected -= AddXp;
        GameEvents.OnEnemyKilled -= HandleEnemyKill;
        GameEvents.OnGameOver -= HandleGameOver;
    }
    
    private void Update()
    {
        // Debug para salvar e carregar
        if (Input.GetKeyDown(KeyCode.F5))
        {
            saveSystem.SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            saveSystem.LoadGame();
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
        
        enemySpawner.StartSpawning();
        StartCoroutine(VictoryTimer());
        StartCoroutine(SecondPartTimer());
        GameEvents.RaiseGameStart();
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

    private void HandleEnemyKill(Enemy killedEnemy)
    {
        if (dashCooldownReductionOnKill > 0)
        {
            player.ReduceDashCooldown(dashCooldownReductionOnKill);
        }
    }

    public void AddXp(float amount)
    {
        if (playerLevel > xpPerLevel.Length) return;
        currentXp += amount;
        xpSlider.value = currentXp / xpPerLevel[playerLevel - 1];
        Debug.Log($"Ganhou {amount:F2} XP. Total: {currentXp:F2}/{xpPerLevel[playerLevel - 1]}");

        if (currentXp >= xpPerLevel[playerLevel - 1])
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        currentXp -= xpPerLevel[playerLevel - 1];
        playerLevel++;
        levelText.text = $"Lv.: {playerLevel}";
        ShowConfetti();
        Debug.Log($"LEVEL UP! Novo nível: {playerLevel}");
        AudioManager.Instance.PlaySoundEffect(5);
        GameEvents.RaisePlayerLevelUp(playerLevel);

        Time.timeScale = 0f;

        List<UpgradeData> selectedUpgrades = GetUpgradeChoices();
        upgradeUI.ShowUpgradeOptions(selectedUpgrades);
    }

    private List<UpgradeData> GetUpgradeChoices()
    {
        List<UpgradeData> choices = new List<UpgradeData>();
        offeredUpgradesPool.Clear();

        for (int i = 0; i < 3; i++)
        {
            Rarity targetRarity = rarityManager.SelectRandomRarity();

            List<UpgradeData> availableByRarity = allUpgrades
                .Where(u => u.rarity == targetRarity && !offeredUpgradesPool.Contains(u))
                .ToList();

            if (availableByRarity.Count == 0)
            {
                availableByRarity = allUpgrades
                    .Where(u => u.rarity <= targetRarity && !offeredUpgradesPool.Contains(u))
                    .ToList();
            }

            if (availableByRarity.Count == 0)
            {
                offeredUpgradesPool.Clear();
                availableByRarity = allUpgrades.Where(u => u.rarity <= targetRarity).ToList();
            }

            if (availableByRarity.Count > 0)
            {
                UpgradeData chosenUpgrade = availableByRarity[Random.Range(0, availableByRarity.Count)];
                choices.Add(chosenUpgrade);
                offeredUpgradesPool.Add(chosenUpgrade);
            }
        }
        return choices;
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        Debug.Log($"Upgrade '{upgrade.upgradeName}' (Raridade: {upgrade.rarity}) selecionado. Aplicando {upgrade.effects.Count} efeito(s).");

        foreach (var effect in upgrade.effects)
        {
            switch (effect.type)
            {
                case UpgradeType.PlayerAttackSpeed:
                    player.attackCooldown /= effect.value;
                    break;
                case UpgradeType.PlayerMoveSpeed:
                    player.moveSpeed *= effect.value;
                    break;
                case UpgradeType.PlayerAttackRange:
                    player.attackRadius *= effect.value;
                    break;
                case UpgradeType.PlayerDashCooldown:
                    player.dashCooldown /= effect.value;
                    break;
                case UpgradeType.PlayerAttackDamage:
                    player.attackDamage *= effect.value;
                    break;
                case UpgradeType.BonfireBurnRate:
                    bonfire.baseBurnRate *= effect.value;
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
                case UpgradeType.WoodDropChance:
                    woodDropMultiplier *= effect.value;
                    break;
                case UpgradeType.DashCooldownOnKill:
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
        GameEvents.RaiseGameWin();
    }

    public void FirstPartWin()
    {
        Debug.Log("Você sobreviveu à primeira parte! Vitória Parcial!");
        if (firstWinPanel != null) firstWinPanel.SetActive(true);
        confettiEffect.Play();
        Time.timeScale = 0f;
        DifficultyManager.Instance.StopTimer();
        GameEvents.RaiseFirstPartWin();
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
        if (godMode) return;
        GameEvents.RaiseGameOver();
    }

    private void HandleGameOver()
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

    public void SwitchGodMode()
    {
        godMode = !godMode;
        if (godMode)
        {
            bonfire.HealToFull();
            bonfire.isInvincible = true;
        }
        else
        {
            bonfire.isInvincible = false;
        }
    }
    #endregion
}