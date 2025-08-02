// Assets/Scripts/EnemyFast.cs
using UnityEngine;

public class EnemyFast : Enemy
{
    [Header("Movimento Rápido")]
    public float spiralFrequency = 3f; // Quão rápido ele oscila
    public float spiralMagnitude = 0.8f; // Quão largo é o círculo da espiral

    protected override void Start()
    {
        base.Start();
        // Stats: Rápido e com menos vida
        speed *= 2.0f;  // 2x a velocidade base
        maxHealth = (int)(maxHealth * 0.5f);
        currentHealth = maxHealth;
    }

    protected override void Move()
    {
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
        
        // Usa Seno para criar um movimento circular perfeito
        float spiralOffset = Mathf.Sin(Time.time * spiralFrequency) * spiralMagnitude;
        
        Vector2 finalDirection = (directionToTarget + perpendicular * spiralOffset).normalized;
        
        transform.Translate(finalDirection * speed * Time.deltaTime, Space.World);
    }
}