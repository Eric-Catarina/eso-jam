// Assets/Scripts/EnemyTank.cs
using UnityEngine;

public class EnemyTank : Enemy
{
    protected override void Start()
    {
        // Stats: Lento e com mais vida
        baseSpeed *= 0.5f;
        baseMaxHealth *= 3;
        
        // Chama o Start da classe base, que vai aplicar os modificadores de dificuldade
        base.Start();
    }

    // Movimento simples e direto
    protected override void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, finalSpeed * Time.deltaTime);    }

    // Sobrescreve o método Die para dropar 3 lenhas
    protected override void Die()
    {
        GameManager.Instance.SpawnBlueExplosion(transform.position);

        if (fireDeath)
        {
            GameManager.Instance.SpawnOrangeExplosion(transform.position);
            Destroy(gameObject);
            return;
        }

        // Lógica de drop especial: sempre dropa 3 lenhas
        if (woodDropPrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                // Pequeno offset aleatório para não spawnar todas no mesmo lugar
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
                Instantiate(woodDropPrefab, spawnPos, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }
}