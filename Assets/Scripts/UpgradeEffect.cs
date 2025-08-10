// Assets/Scripts/UpgradeEffect.cs
using System;

// Marcamos como [Serializable] para que possa ser editado no Inspector da Unity
// dentro da lista de efeitos do UpgradeData.
[Serializable]
public class UpgradeEffect
{
    public UpgradeType type; // O tipo de efeito (ex: aumentar velocidade de ataque)
    public float value;      // O valor do efeito (ex: 1.25 para +25%)
}