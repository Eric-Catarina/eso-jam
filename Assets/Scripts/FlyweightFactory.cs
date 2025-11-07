// Assets/Scripts/FlyweightFactory.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// O objeto Flyweight. Contém o estado intrínseco (compartilhado).
/// Neste caso, é o sprite do inimigo.
/// </summary>
public class EnemyVisualsFlyweight
{
    public readonly Sprite Sprite;

    public EnemyVisualsFlyweight(Sprite sprite)
    {
        this.Sprite = sprite;
    }
}

/// <summary>
/// A fábrica de Flyweights. Garante que os flyweights sejam compartilhados.
/// É uma classe estática para ser acessível globalmente sem precisar de uma instância na cena.
/// </summary>
public static class FlyweightFactory
{
    private static Dictionary<EnemyType, EnemyVisualsFlyweight> flyweights = new Dictionary<EnemyType, EnemyVisualsFlyweight>();

    /// <summary>
    /// Obtém o flyweight visual para um tipo de inimigo.
    /// Se já existir, retorna a instância compartilhada.
    /// Se não, carrega o sprite do Resources, cria um novo flyweight, armazena-o e o retorna.
    /// </summary>
    public static EnemyVisualsFlyweight GetEnemyVisuals(EnemyType type)
    {
        if (flyweights.TryGetValue(type, out EnemyVisualsFlyweight flyweight))
        {
            return flyweight;
        }
        else
        {
            // Carrega o sprite de uma pasta "Resources" com base no nome do enum.
            // Ex: EnemyType.Common -> Carrega "EnemySprites/Common.png"
            Sprite sprite = Resources.Load<Sprite>($"EnemySprites/{type.ToString()}");
            if (sprite == null)
            {
                Debug.LogError($"Sprite não encontrado para o inimigo do tipo '{type}' no caminho 'Resources/EnemySprites/{type.ToString()}'");
                return null;
            }
            
            EnemyVisualsFlyweight newFlyweight = new EnemyVisualsFlyweight(sprite);
            flyweights.Add(type, newFlyweight);
            
            Debug.Log($"Novo Flyweight criado para: {type}");
            return newFlyweight;
        }
    }
}