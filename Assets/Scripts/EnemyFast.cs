// Assets/Scripts/EnemyFast.cs
using UnityEngine;

public class EnemyFast : Enemy
{
    public override void PerformMovement()
    {
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
        float spiralOffset = Mathf.Sin(Time.time * enemyData.spiralFrequency) * enemyData.spiralMagnitude;
        Vector2 finalDirection = (directionToTarget + perpendicular * spiralOffset).normalized;
        transform.Translate(finalDirection * finalSpeed * Time.deltaTime, Space.World);
    }
}