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

    public float logHealingAmount = 10f; // Quanto de vida a fogueira ganha ao coletar lenha
    [Tooltip("Controla como a taxa de queima aumenta com a vida. X=0 (0% vida), Y=1 (taxa base). X=1 (100% vida), Y=2.5 (taxa máxima).")]
    public AnimationCurve burnRateCurve = AnimationCurve.EaseInOut(0, 1, 1, 15f);

    [Header("Referências de UI e VFX")]
    [Tooltip("O Slider da UI que representa a vida da fogueira.")]
    public Slider healthSlider;
    [Tooltip("O sistema de partículas principal da fogueira.")]
    public ParticleSystem fireParticleSystem;
    [Tooltip("O componente de luz da fogueira.")]
    public LightFlicker bonfireLight;

    private float initialParticleGravity;
    private float initialParticleStartSize;
    private float initialParticleStartSpeed;
    private float initialParticleEmissionRate;

    public bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (bonfireLight == null) Debug.LogError("Referência para LightFlicker não definida no Bonfire!");
        if (fireParticleSystem == null) Debug.LogError("Referência para ParticleSystem não definida no Bonfire!");
        else
        {
            var mainModule = fireParticleSystem.main;
            initialParticleGravity = mainModule.gravityModifier.constant;
            initialParticleStartSize = mainModule.startSize.constant;
            initialParticleStartSpeed = mainModule.startSpeed.constant;
            initialParticleEmissionRate = fireParticleSystem.emission.rateOverTime.constant;
        }

        if (healthSlider == null) Debug.LogError("Referência para o Health Slider não definida no Bonfire!");

        UpdateHealthSlider();
    }

    void Update()
    {
        TakeBurnDamage();
        UpdateHealthSlider();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lenha"))
        {
            ThrownWood woodScript = other.GetComponent<ThrownWood>();
            if (woodScript != null)
            {
                AudioManager.Instance.PlaySoundEffect(6);

                // Dispara o evento global de coleta de lenha, passando o valor de XP.
                GameEvents.RaiseWoodCollected(woodScript.xpValue);

                Destroy(other.gameObject); // Remove a lenha do jogo

                // Cura a fogueira (lógica de cura permanece a mesma).
                currentHealth += logHealingAmount;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                GameManager.Instance.SpawnOrangeExplosion(transform.position); // Efeito visual.
            }
        }
    }

    private void TakeBurnDamage()
    {
        if (isInvincible) return;
        float healthPercent = currentHealth / maxHealth;
        float currentBurnMultiplier = burnRateCurve.Evaluate(healthPercent);
        float currentBurnRate = baseBurnRate * currentBurnMultiplier;
        currentHealth -= currentBurnRate * Time.deltaTime;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateFireVisuals(healthPercent);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // O GameManager agora ouve o evento OnGameOver, então a chamada direta pode ser removida se preferir.
            // Por enquanto, manter a chamada direta garante que a lógica funcione.
            GameManager.Instance.LoseGame();
            Debug.Log("A FOGUEIRA APAGOU! GAME OVER.");
        }
    }

    private void UpdateHealthSlider()
    {
        if (healthSlider == null) return;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    private void UpdateFireVisuals(float healthPercent)
    {
        if (fireParticleSystem != null)
        {
            var main = fireParticleSystem.main;
            main.gravityModifier = Mathf.Lerp(initialParticleGravity * 0.5f, initialParticleGravity * 3, healthPercent);
            main.startSize = Mathf.Lerp(initialParticleStartSize * 0.5f, initialParticleStartSize * 2f, healthPercent);
            main.startSpeed = Mathf.Lerp(initialParticleStartSpeed * 0.75f, initialParticleStartSpeed * 5f, healthPercent);
            var emission = fireParticleSystem.emission;
            emission.rateOverTime = Mathf.Lerp(initialParticleEmissionRate * 0.5f, initialParticleEmissionRate * 3f, healthPercent);
        }

        if (bonfireLight != null)
        {
            bonfireLight.UpdateLightStrength(healthPercent);
        }
    }

    public void AddHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        GameManager.Instance.SpawnOrangeExplosionOnBonfire();
    }

    public void ReceberDano(float dano)
    {
        if (isInvincible) return;
        currentHealth -= dano;
    }

    public void IncreaseMaxHealth(float multiplier)
    {
        float oldMaxHealth = maxHealth;
        maxHealth *= multiplier;
        float healthGained = maxHealth - oldMaxHealth;
        currentHealth += healthGained;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthSlider();

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