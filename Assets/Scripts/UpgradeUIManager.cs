// Assets/Scripts/UpgradeUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UpgradeUIManager : MonoBehaviour
{
    public GameObject upgradePanel;
    public List<Button> upgradeButtons;
    public List<TextMeshProUGUI> upgradeNameTexts;
    public List<TextMeshProUGUI> upgradeDescriptionTexts;
    public List<Image> upgradeIcons;

    public void ShowUpgradeOptions(List<UpgradeData> upgrades)
    {
        upgradePanel.SetActive(true);
        GetComponent<UIJuice>().PlayAnimation();

        Debug.Log($"Mostrando {upgrades.Count} opções de upgrade.");
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (i < upgrades.Count)
            {
                // Ativa e configura o botão
                upgradeButtons[i].gameObject.SetActive(true);
                upgradeNameTexts[i].text = upgrades[i].upgradeName;
                upgradeDescriptionTexts[i].text = upgrades[i].description;
                upgradeIcons[i].sprite = upgrades[i].icon;

                // Limpa listeners antigos e adiciona o novo
                upgradeButtons[i].onClick.RemoveAllListeners();
                var selectedUpgrade = upgrades[i]; // Variável local para evitar problemas de closure
                upgradeButtons[i].onClick.AddListener(() => HidePanel());
                upgradeButtons[i].onClick.AddListener(() => GameManager.Instance.ApplyUpgrade(selectedUpgrade));

            }
            else
            {
                // Desativa botões não utilizados
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HidePanel()
    {
        GetComponent<UIJuice>().PlayReverseAnimation();
        // disable buttons and remove listeners
        foreach (var button in upgradeButtons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }


    }
}