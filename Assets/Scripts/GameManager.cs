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

    [Header("UI")]
    public GameObject losePanel;
    public GameObject firstWinPanel, winPanel;
    public GameObject startPanel;
    public TextMeshProUGUI levelText;
    public Slider xpSlider;

    [Header("Controle de Nível")]
    public int playerLevel = 1;
    private float currentXp = 0f; // XP agora é um float para mais precisão.
    public int[] xpPerLevel = { 1, 2, 3, 4, 5, 7, 9, 30, 38, 47 };

    [Header("Upgrades")]
    [Tooltip("Lista com todos os upgrades possíveis no jogo.")]
    public List<UpgradeData> allUpgrades;
    private List<UpgradeData> offeredUpgradesPool;

    public ParticleSystem confettiEffect, orangeExplosionEffect, blueExplosionEffect;

    [Header("Stats Globais Modificáveis por Upgrades")]
    [Tooltip("Multiplicador para a QUANTIDADE de lenha dropada pelos inimigos. 1 = 100% do base, 1.5 = 150% do base.")]
    public float woodDropMultiplier = 1f; // Nome alterado para clareza.
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
        
        // Se inscreve nos eventos globais para reagir a eles.
        GameEvents.OnWoodCollected += AddXp;
        GameEvents.OnEnemyKilled += HandleEnemyKill;
        GameEvents.OnGameOver += HandleGameOver;
    }

    private void OnDestroy()
    {
        // Sempre cancele a inscrição ao destruir o objeto para evitar memory leaks.
        GameEvents.OnWoodCollected -= AddXp;
        GameEvents.OnEnemyKilled -= HandleEnemyKill;
        GameEvents.OnGameOver -= HandleGameOver;
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

        if (enemySpawner == null) Debug.LogError("Referência ao EnemySpawner não foi definida no GameManager!");
        if (rarityManager == null) Debug.LogError("Referência ao RarityManager não foi definida no GameManager!");
        
        enemySpawner.StartSpawning();
        StartCoroutine(VictoryTimer());
        StartCoroutine(SecondPartTimer());
        GameEvents.RaiseGameStart(); // Dispara evento de início de jogo.
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

    // Handler para o evento de morte de inimigo (usado para upgrades específicos).
    private void HandleEnemyKill(Enemy killedEnemy)
    {
        if (dashCooldownReductionOnKill > 0)
        {
            player.ReduceDashCooldown(dashCooldownReductionOnKill);
        }
    }

    // Método que ouve o evento OnWoodCollected. Aceita float.
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
        GameEvents.RaisePlayerLevelUp(playerLevel); // Dispara evento de level up.

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
            Debug.Log($"Escolhendo upgrade de raridade: {targetRarity}");

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
                case UpgradeType.WoodDropChance: // O enum continua com este nome
                    woodDropMultiplier *= effect.value; // mas a lógica usa a variável correta.
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
        GameEvents.RaiseGameWin(); // Dispara evento de vitória.
    }

    public void FirstPartWin()
    {
        Debug.Log("Você sobreviveu à primeira parte! Vitória Parcial!");
        if (firstWinPanel != null) firstWinPanel.SetActive(true);
        confettiEffect.Play();
        Time.timeScale = 0f;
        DifficultyManager.Instance.StopTimer();
        GameEvents.RaiseFirstPartWin(); // Dispara evento de vitória parcial.
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
        // Apenas dispara o evento. A lógica de UI e pause está no handler.
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