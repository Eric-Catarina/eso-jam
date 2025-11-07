// Assets/Scripts/IEnemyState.cs
public interface IEnemyState
{
    void Execute(Enemy enemy);
    void OnEnter(Enemy enemy);
}