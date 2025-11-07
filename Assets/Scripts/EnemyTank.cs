// Assets/Scripts/EnemyTank.cs
using UnityEngine;

public class EnemyTank : Enemy
{
    public override void PerformMovement()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, finalSpeed * Time.deltaTime);
    }
}