// Assets/Scripts/Bonfire.cs
using UnityEngine;
using UnityEngine.UI; // Necessário para controlar o Slider

public class Bonfire : MonoBehaviour
{
    [Header("Health & Burning")]
    [Tooltip("Vida máxima atual da fogueira.")]
    public float maxHealth = 100f;
    [Tooltip("Vida atual da fogueira.")]
    public float currentHealth;

    [Tooltip("A taxa de queima base quando a fogueira está com vida baixa.")]
    public float baseBurnRate = 0.5f; // Perda de energia por segundo
    [Tooltip("Multiplicador da taxa de queima quando a fogueira está com vida máxima.")]
    public float maxHealthBurnMultiplier = 2.5f; // A 100% de vida, queima 2.5x mais rápido
    
    // Curva para um controle suave da taxa de queima
    [Tooltip("Controla como a taxa de queima aumenta com a vida. X=0 (0% vida), Y=1 (taxa base). X=1 (100% vida), Y=2.5 (taxa máxima).")]
    public AnimationCurve burnRateCurve = AnimationCurve.EaseInOut(0, 1, 1, 15f);

    [Header("Referências de UI e VFX")]
    [Tooltip("O Slider da UI que representa a vida da fogueira.")]
    public Slider healthSlider;
    [Tooltip("O sistema de partículas principal da fogueira.")]
    public ParticleSystem fireParticleSystem;
    [Tooltip("O componente de luz da fogueira.")]
    public LightFlicker bonfireLight;

    // --- Variáveis Privadas ---
    private float initialParticleGravity;
    private float initialParticleStartSize;
    private float initialParticleStartSpeed;
    private float initialParticleEmissionRate;

    void Start()
    {
        currentHealth = maxHealth;

        // Validações iniciais
        if (bonfireLight == null)
        {
            Debug.LogError("Referência para LightFlicker não definida no Bonfire!");
        }
        if (fireParticleSystem == null)
        {
            Debug.LogError("Referência para ParticleSystem não definida no Bonfire!");
        }
        else
        {
            // Guarda os valores iniciais do sistema de partículas para referência
            var mainModule = fireParticleSystem.main;
            initialParticleGravity = mainModule.gravityModifier.constant;
            initialParticleStartSize = mainModule.startSize.constant;
            initialParticleStartSpeed = mainModule.startSpeed.constant;
            initialParticleEmissionRate = fireParticleSystem.emission.rateOverTime.constant;
        }

        if (healthSlider == null)
        {
            Debug.LogError("Referência para o Health Slider não definida no Bonfire!");
        }
        
        // Configura o slider com os valores iniciais
        UpdateHealthSlider();
    }

    void Update()
    {
        // Calcula a porcentagem de vida atual (0.0 a 1.0)
        float healthPercent = currentHealth / maxHealth;
        
        // Calcula a taxa de queima atual baseada na curva
        float currentBurnMultiplier = burnRateCurve.Evaluate(healthPercent);
        float currentBurnRate = baseBurnRate * currentBurnMultiplier;

        // Perde vida com o tempo
        currentHealth -= currentBurnRate * Time.deltaTime;
        
        // Garante que a vida não fique negativa
        currentHealth = Mathf.Max(currentHealth, 0);

        // Atualiza todos os sistemas dependentes
        UpdateFireVisuals(healthPercent);
        UpdateHealthSlider();

        // Condição de derrota (Game Over)
        if (currentHealth <= 0)
        {
            GameManager.Instance.LoseGame(); // Chama o método de Game Over do GameManager
            Debug.Log("A FOGUEIRA APAGOU! GAME OVER.");
            // Lógica de Game Over...
            // Time.timeScale = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lenha"))
        {
            AudioManager.Instance.PlaySoundEffect(6);

            // Aqui você pode adicionar lógica para coletar a lenha, como:
            GameManager.Instance.AddXp(1);
            Destroy(other.gameObject); // Remove a lenha do jogo
            currentHealth += 10; // Por exemplo, adiciona 10 de vida à fogueira
            currentHealth = Mathf.Min(currentHealth, maxHealth); // Garante que não ultrapasse o máximo
            Debug.Log("Lenha coletada! Vida da fogueira aumentada.");
            GameManager.Instance.SpawnOrangeExplosion(transform.position); // Efeito visual de coleta
        }
          

    }

    /// <summary>
    /// Atualiza o slider de vida da UI.
    /// </summary>
    private void UpdateHealthSlider()
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    /// <summary>
    /// Atualiza os efeitos visuais (partículas e luz) baseados na porcentagem de vida.
    /// </summary>
    private void UpdateFireVisuals(float healthPercent)
    {
        // --- Atualiza as Partículas ---
        if (fireParticleSystem != null)
        {
            var main = fireParticleSystem.main;

            // Quanto mais vida, menos gravidade (fogo mais alto e "selvagem")
            main.gravityModifier = Mathf.Lerp(initialParticleGravity * 0.5f, initialParticleGravity * 3, healthPercent);

            // Quanto mais vida, maior o tamanho inicial das partículas
            main.startSize = Mathf.Lerp(initialParticleStartSize * 0.5f, initialParticleStartSize * 2f, healthPercent);

            // Quanto mais vida, mais rápido as partículas sobem
            main.startSpeed = Mathf.Lerp(initialParticleStartSpeed * 0.75f, initialParticleStartSpeed * 5f, healthPercent);

            var emission = fireParticleSystem.emission;
            emission.rateOverTime = Mathf.Lerp(initialParticleEmissionRate * 0.5f, initialParticleEmissionRate * 3f, healthPercent);
        }

        // --- Atualiza a Luz ---
        if (bonfireLight != null)
        {
            // Mapeia a vida para a intensidade e raio da luz
            bonfireLight.UpdateLightStrength(healthPercent);
        }
    }

    /// <summary>
    /// Adiciona vida à fogueira (chamado ao entregar lenha, por exemplo).
    /// </summary>
    public void AddHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Garante que não ultrapasse o máximo
        GameManager.Instance.SpawnOrangeExplosionOnBonfire(); // Feedback visual
    }

    public void ReceberDano(int dano)
    {
        currentHealth -= dano;
    }

    // --- MÉTODOS PARA UPGRADES ---
    public void IncreaseMaxHealth(float multiplier)
    {
        float oldMaxHealth = maxHealth;
        maxHealth *= multiplier;
        float healthGained = maxHealth - oldMaxHealth;
        currentHealth += healthGained;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthSlider();
        
        // Avisa o componente de luz que seus valores máximos também aumentaram.
        if (bonfireLight != null)
        {
            bonfireLight.UpgradeLightMaximums(multiplier);
        }
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
    }
}