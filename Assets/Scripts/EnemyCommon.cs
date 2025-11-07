// Assets/Scripts/EnemyCommon.cs
using UnityEngine;

public class EnemyCommon : Enemy
{
    private float randomOffset;

    private void Start()
    {
        randomOffset = Random.Range(0f, 100f);
    }
    
    public override void PerformMovement()
    {
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        Vector2 perpendicular = new Vector2(-directionToTarget.y, directionToTarget.x);
        float sway = (Mathf.PerlinNoise(Time.time * enemyData.swaySpeed, randomOffset) - 0.5f) * 2f;
        Vector2 finalDirection = (directionToTarget + perpendicular * sway * enemyData.swayAmount).normalized;
        transform.Translate(finalDirection * finalSpeed * Time.deltaTime, Space.World);
    }
}