// Assets/Scripts/EnemyCommon.cs
using UnityEngine;

public class EnemyCommon : Enemy
{
    [Header("Movimento Comum")]
    public float swaySpeed = 2f;
    public float swayAmount = 1.5f;
    private float randomOffset;

    protected override void Start()
    {
        base.Start(); // Executa o Start() da classe mãe
        randomOffset = Random.Range(0f, 100f); // Garante que cada inimigo se mova diferente
    }

    protected override void Move()
    {
        // Direção principal para a fogueira
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        
        // Direção perpendicular para o balanço
        Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
        
        // Usa Perlin Noise para um movimento de balanço suave e orgânico
        float sway = (Mathf.PerlinNoise(Time.time * swaySpeed, randomOffset) - 0.5f) * 2f; // Valor entre -1 e 1
        
        // Combina a direção principal com o balanço
        Vector2 finalDirection = (directionToTarget + perpendicular * sway * swayAmount).normalized;
        
        transform.Translate(finalDirection * speed * Time.deltaTime, Space.World);
    }
}