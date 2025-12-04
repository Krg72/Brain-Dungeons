using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeDetailsPanel : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text priceText;
    public TMP_Text typeText;

    public Button upgradeButton;

    private UpgradeNodeUI currentUI;
    private UpgradeManager manager;

    public void Setup(UpgradeManager mgr)
    {
        manager = mgr;
        gameObject.SetActive(false);
    }

    public void Show(UpgradeNodeUI nodeUI)
    {
        currentUI = nodeUI;
        var node = nodeUI.data;
        int level = manager.GetNodeLevel(node.id);

        nameText.text = node.displayName;
        levelText.text = $"Level {level}/{node.maxLevel}";
        typeText.text = node.type.ToString();

        if (level >= node.maxLevel)
        {
            priceText.text = "Max Level";
            upgradeButton.interactable = false;
        }
        else
        {
            float cost = node.levelCosts[level];
            priceText.text = $"Cost: {cost}";
            upgradeButton.interactable = manager.CanAfford(cost);
        }

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => manager.TryUpgradeNode(currentUI));

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
