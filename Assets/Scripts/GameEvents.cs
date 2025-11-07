// Assets/Scripts/GameEvents.cs
using System;
using UnityEngine;

/// <summary>
/// Classe estática central para todos os eventos do jogo, usando C# Actions.
/// Isso ajuda a desacoplar os sistemas, permitindo que eles reajam a eventos
/// sem precisarem de referências diretas uns aos outros.
/// </summary>
public static class GameEvents
{
    // Disparado quando um inimigo é morto. Passa o componente do Inimigo para contexto.
    public static event Action<Enemy> OnEnemyKilled;
    public static void RaiseEnemyKilled(Enemy enemy) => OnEnemyKilled?.Invoke(enemy);

    // Disparado quando a fogueira coleta lenha. Passa o valor de XP daquela lenha.
    public static event Action<float> OnWoodCollected;
    public static void RaiseWoodCollected(float xpValue) => OnWoodCollected?.Invoke(xpValue);

    // Disparado quando o jogador sobe de nível. Passa o novo nível.
    public static event Action<int> OnPlayerLevelUp;
    public static void RaisePlayerLevelUp(int newLevel) => OnPlayerLevelUp?.Invoke(newLevel);

    // Disparado quando o jogo começa oficialmente (após o painel inicial).
    public static event Action OnGameStart;
    public static void RaiseGameStart() => OnGameStart?.Invoke();

    // Disparado quando o jogo é perdido (fogueira apaga).
    public static event Action OnGameOver;
    public static void RaiseGameOver() => OnGameOver?.Invoke();

    // Disparado quando o jogo é vencido (tempo esgota).
    public static event Action OnGameWin;
    public static void RaiseGameWin() => OnGameWin?.Invoke();
    
    // Disparado quando a primeira parte do jogo é vencida.
    public static event Action OnFirstPartWin;
    public static void RaiseFirstPartWin() => OnFirstPartWin?.Invoke();
}