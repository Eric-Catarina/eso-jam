using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Header("Configuração do Efeito")]
    [Tooltip("Quão forte a luz pisca, como uma porcentagem da intensidade atual. (ex: 0.1 para 10%)")]
    [Range(0f, 1f)]
    public float intensityVariation = 0.1f;

    [Tooltip("Quão forte o raio pisca, como uma porcentagem do raio atual. (ex: 0.1 para 10%)")]
    [Range(0f, 1f)]
    public float radiusVariation = 0.1f;

    [Tooltip("A velocidade com que o efeito de piscar ocorre.")]
    public float flickerSpeed = 0.08f;

    [Header("Limites Mínimos")]
    [Tooltip("A intensidade mínima que a luz terá quando a fogueira estiver com 0 de vida.")]
    public float minIntensity = 0.2f;

    [Tooltip("O raio mínimo que a luz terá quando a fogueira estiver com 0 de vida.")]
    public float minOuterRadius = 1.5f;

    // --- Variáveis Privadas ---
    private Light2D light2D;
    private float flickerTimer;

    // Guardam os valores máximos possíveis, capturados no início ou aumentados por upgrades
    private float maxPossibleIntensity;
    private float maxPossibleOuterRadius;

    // Guardam o valor base ATUAL, que é definido pela fogueira a cada frame
    private float currentBaseIntensity;
    private float currentBaseOuterRadius;
    internal float baseIntensity;

    void Awake()
    {
        // É uma boa prática pegar componentes no Awake
        light2D = GetComponent<Light2D>();
    }

    void Start()
    {
        // REQUISITO 1: Captura os valores do Inspector no momento do Play como os MÁXIMOS iniciais.
        maxPossibleIntensity = light2D.intensity;
        maxPossibleOuterRadius = light2D.pointLightOuterRadius;
    }

    void Update()
    {
        flickerTimer += Time.deltaTime;
        if (flickerTimer >= flickerSpeed)
        {
            flickerTimer = 0f;

            // REQUISITO 3: O flicker (a variação) é calculado com base na FORÇA ATUAL da luz.
            float intensityFlickerRange = currentBaseIntensity * intensityVariation;
            float newIntensity = currentBaseIntensity + Random.Range(-intensityFlickerRange, intensityFlickerRange);

            float radiusFlickerRange = currentBaseOuterRadius * radiusVariation;
            float newOuterRadius = currentBaseOuterRadius + Random.Range(-radiusFlickerRange, radiusFlickerRange);
            
            // Aplica os valores finais à luz, garantindo que não passem do máximo possível
            light2D.intensity = Mathf.Clamp(newIntensity, minIntensity, maxPossibleIntensity);
            light2D.pointLightOuterRadius = Mathf.Clamp(newOuterRadius, minOuterRadius, maxPossibleOuterRadius);
        }
    }

    /// <summary>
    /// Este é o método principal que a Fogueira chamará para definir a força da luz.
    /// </summary>
    /// <param name="strengthPercent">A força da luz, de 0.0 (apagada) a 1.0 (força máxima).</param>
    public void UpdateLightStrength(float strengthPercent)
    {
        // REQUISITO 4: A força atual é baseada na vida da fogueira.
        // Usamos Lerp para encontrar o valor base entre o mínimo e o máximo possível.
        currentBaseIntensity = Mathf.Lerp(minIntensity, maxPossibleIntensity, strengthPercent);
        currentBaseOuterRadius = Mathf.Lerp(minOuterRadius, maxPossibleOuterRadius, strengthPercent);
    }
    
    /// <summary>
    /// Este método será chamado pelo upgrade para aumentar os limites máximos da luz.
    /// </summary>
    public void UpgradeLightMaximums(float multiplier)
    {
        // REQUISITO 2: O máximo só aumenta com upgrades.
        maxPossibleIntensity *= multiplier;
        maxPossibleOuterRadius *= multiplier;
    }
}
