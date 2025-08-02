// Assets/Scripts/GameManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool isGameRunning = false;
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public PlayerController player;
    public Bonfire bonfire;
    public UpgradeUIManager upgradeUI;
    public EnemySpawner enemySpawner; // Adicione a referência ao spawner

    [Header("UI")]
    public GameObject losePanel;
    public GameObject winPanel; // Opcional: Um painel de vitória
    public GameObject startPanel; // Opcional: Um painel com um botão "Iniciar"

    [Header("Controle de Nível")]
    public int playerLevel = 1;
    private int currentXp = 0;
    // Custo de XP para cada nível (Nível 1 custa 1, Nível 2 custa 3, etc.)
    public int[] xpPerLevel = { 1, 2, 3, 4, 5, 7, 9, 30, 38, 47 }; 

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
            // DontDestroyOnLoad(gameObject); // Pode ser útil, mas cuidado ao reiniciar a cena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Garante que tudo esteja pausado ou inativo no começo
        Time.timeScale = 1f; // Reseta a escala de tempo
        losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        
        // Se tiver um painel inicial, mostre-o. Senão, inicie o jogo direto.
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo até o jogador clicar em "Start"
        }
        else
        {
            StartGame();
        }
    }

    // Este método será chamado pelo botão "Start" da sua UI
    public void StartGame()
    {
        if (isGameRunning) return; // Evita iniciar o jogo mais de uma vez
        isGameRunning = true;
        Time.timeScale = 1f;
        AudioManager.Instance.PlayBackgroundMusic(1); // Toca a música do menu ou inicial
        // Inicia os sistemas do jogo
        availableUpgrades = new List<UpgradeData>(allUpgrades);
        
        if (enemySpawner == null)
        {
            Debug.LogError("Referência ao EnemySpawner não foi definida no GameManager!");
            return;
        }
        enemySpawner.StartSpawning();
        
        // Inicia o timer de vitória (2 minutos)
        StartCoroutine(VictoryTimer());
    }

    private IEnumerator VictoryTimer()
    {
        yield return new WaitForSeconds(120); // Espera 2 minutos
        WinGame();
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
        AudioManager.Instance.PlaySoundEffect(5);

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
                bonfire.baseBurnRate *= upgrade.value; // Ex: 0.9x burn rate
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
            case UpgradeType.PlayerAttackRange:
                player.attackRadius *= upgrade.value; // Ex: 1.2x range
                break;
            case UpgradeType.PlayerDashCooldown:
                player.dashCooldown *= (1f / upgrade.value); // Ex: 0.8x cooldown -> cooldown * (1/0.8)
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

    public void WinGame()
    {
        Debug.Log("Você sobreviveu! Vitória!");
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
        DifficultyManager.Instance.StopTimer();
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
        Time.timeScale = 1f; // Garante que o tempo volte ao normal antes de recarregar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}