// Assets/Scripts/RarityManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RarityManager : MonoBehaviour
{
    public static RarityManager Instance { get; private set; }

    // Classe interna para configurar cada raridade no Inspector
    [System.Serializable]
    public class RarityInfo
    {
        public Rarity rarity;
        public Color displayColor;
        [Range(0f, 100f)]
        public float baseDropChance; // Chance em porcentagem (ex: 60 para 60%)
    }

    [Header("Configuração de Raridades")]
    [Tooltip("Defina a cor e a chance de drop para cada raridade.")]
    public List<RarityInfo> raritySettings;
    
    [Header("Modificadores Globais")]
    [Tooltip("Multiplicador para a chance de raridades altas (Épico, Lendário). Alterado por upgrades como 'Coroa de Midas'.")]
    public float highTierChanceMultiplier = 1.0f;

    private Dictionary<Rarity, RarityInfo> rarityMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        // Converte a lista em um dicionário para acesso rápido e fácil
        rarityMap = new Dictionary<Rarity, RarityInfo>();
        foreach (var setting in raritySettings)
        {
            if (!rarityMap.ContainsKey(setting.rarity))
            {
                rarityMap.Add(setting.rarity, setting);
            }
        }
    }

    /// <summary>
    /// Retorna a cor associada a uma raridade específica.
    /// </summary>
    public Color GetRarityColor(Rarity rarity)
    {
        if (rarityMap.TryGetValue(rarity, out RarityInfo info))
        {
            return info.displayColor;
        }
        return Color.white; // Cor padrão caso não encontre
    }

    /// <summary>
    /// Seleciona uma raridade aleatória com base nas chances configuradas.
    /// </summary>
    public Rarity SelectRandomRarity()
    {
        // Cria uma lista de chances ponderadas
        Dictionary<Rarity, float> weightedChances = new Dictionary<Rarity, float>();
        foreach (var info in raritySettings)
        {
            float currentChance = info.baseDropChance;
            // Aplica o bônus da "Coroa de Midas" para raridades altas
            if (info.rarity == Rarity.Epic || info.rarity == Rarity.Legendary)
            {
                currentChance *= highTierChanceMultiplier;
            }
            weightedChances.Add(info.rarity, currentChance);
        }

        float totalChanceWeight = weightedChances.Values.Sum();
        float randomRoll = Random.Range(0f, totalChanceWeight);

        // Percorre as raridades e subtrai suas chances do valor aleatório.
        // A primeira raridade que fizer o valor ficar abaixo de zero é a escolhida.
        foreach (var weightedChance in weightedChances)
        {
            randomRoll -= weightedChance.Value;
            if (randomRoll <= 0f)
            {
                return weightedChance.Key;
            }
        }

        // Como fallback, caso algo dê errado, retorna a raridade Comum.
        return Rarity.Common;
    }
}