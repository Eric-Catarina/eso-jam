// Assets/Scripts/UpgradeUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UpgradeUIManager : MonoBehaviour
{
    public GameObject upgradePanel;
    public List<Button> upgradeButtons;
    public List<Image> upgradeRarityBorders; // << NOVO: Referência para a borda/fundo do card
    public List<TextMeshProUGUI> upgradeNameTexts;
    public List<TextMeshProUGUI> upgradeDescriptionTexts;
    public List<Image> upgradeIcons;

    public void ShowUpgradeOptions(List<UpgradeData> upgrades)
    {
        upgradePanel.SetActive(true);
        DisableButtons();
        // Assumindo que você tem um script de animação, como UIJuice. Se não, remova esta linha.
        // GetComponent<UIJuice>().PlayAnimation();

        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (i < upgrades.Count && upgrades[i] != null)
            {
                upgradeButtons[i].gameObject.SetActive(true);
                upgradeNameTexts[i].text = upgrades[i].upgradeName;
                upgradeDescriptionTexts[i].text = upgrades[i].description;
                upgradeIcons[i].sprite = upgrades[i].icon;

                // Define a cor da borda com base na raridade
                if (RarityManager.Instance != null && i < upgradeRarityBorders.Count)
                {
                    upgradeRarityBorders[i].color = RarityManager.Instance.GetRarityColor(upgrades[i].rarity);
                }

                upgradeButtons[i].onClick.RemoveAllListeners();
                var selectedUpgrade = upgrades[i];
                upgradeButtons[i].onClick.AddListener(() => GameManager.Instance.ApplyUpgrade(selectedUpgrade));
                // O painel agora é escondido pelo próprio GameManager após aplicar o upgrade
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HidePanel()
    {
        // Assumindo que você tem um script de animação. Se não, apenas desative o painel.
        // GetComponent<UIJuice>().PlayReverseAnimation();
        upgradePanel.SetActive(false); // Alternativa simples

        foreach (var button in upgradeButtons)
        {
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }
    }
    
    public void EnableButtons()
    {
        foreach (var button in upgradeButtons)
        {
            button.interactable = true;
        }
    }
    
    public void DisableButtons()
    {
        foreach (var button in upgradeButtons)
        {
            button.interactable = false;
        }
    }
}