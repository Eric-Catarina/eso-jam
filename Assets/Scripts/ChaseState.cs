// Assets/Scripts/ChaseState.cs
using UnityEngine;

public class ChaseState : IEnemyState
{
    private const float ATTACK_DISTANCE_THRESHOLD = 1.5f;

    public void OnEnter(Enemy enemy) { }

    public void Execute(Enemy enemy)
    {
        if (enemy.target == null) return;

        enemy.PerformMovement();

        float distanceToTarget = Vector2.Distance(enemy.transform.position, enemy.target.position);
        if (distanceToTarget < ATTACK_DISTANCE_THRESHOLD)
        {
            enemy.ChangeState(new AttackState());
        }
    }
}