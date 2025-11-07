// Assets/Scripts/AttackState.cs
public class AttackState : IEnemyState
{
    public void OnEnter(Enemy enemy)
    {
        Bonfire bonfire = enemy.target.GetComponent<Bonfire>();
        if (bonfire != null)
        {
            bonfire.ReceberDano(enemy.dano);
        }
        enemy.SetFireDeath();
        enemy.ForceDie();
    }
    public void Execute(Enemy enemy) { }
}