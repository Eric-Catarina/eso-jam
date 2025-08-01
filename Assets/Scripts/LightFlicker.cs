using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Header("Intensidade")]
    public float baseIntensity = 1f;
    public float intensityVariation = 0.2f;

    [Header("Raio da luz")]
    public float baseOuterRadius = 5f;
    public float radiusVariation = 0.2f;

    [Header("Velocidade da oscilação")]
    public float flickerSpeed = 0.1f;

    private Light2D light2D;
    private float flickerTimer;

    void Start()
    {
        light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        flickerTimer += Time.deltaTime;
        if (flickerTimer >= flickerSpeed)
        {
            flickerTimer = 0f;

            float newIntensity = baseIntensity + Random.Range(-intensityVariation, intensityVariation);
            float newOuterRadius = baseOuterRadius + Random.Range(-radiusVariation, radiusVariation);

            light2D.intensity = Mathf.Clamp(newIntensity, 0, 10);
            light2D.pointLightOuterRadius = Mathf.Clamp(newOuterRadius, 0, 20);
        }
    }
}
