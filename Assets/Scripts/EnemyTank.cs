// Assets/Scripts/EnemyTank.cs
using UnityEngine;

public class EnemyTank : Enemy
{
    protected override void Start()
    {
        // Stats: Lento e com mais vida.
        // Define a quantidade base de lenha que o Tank dropa. Este valor será
        // afetado por multiplicadores de upgrades.
        baseWoodToDrop = 2f; // Ex: Dropa 2 lenhas garantidas.

        // Chama o Start da classe base, que vai aplicar os modificadores de dificuldade.
        base.Start();
    }

    // Movimento simples e direto.
    protected override void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, finalSpeed * Time.deltaTime);
    }

    // O método Die() foi REMOVIDO. Agora ele usa a lógica unificada da classe base 'Enemy',
    // tornando o código mais limpo e evitando duplicação. A lógica de drop especial
    // é controlada pela variável 'baseWoodToDrop'.
}