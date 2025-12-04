using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum NodeState
{
    Hidden,         // Not visible at all yet
    Locked,         // Visible but cannot upgrade yet
    CanUpgrade,     // Visible and can afford upgrade AND parent is leveled
    Maxed           // Level == max level
}

public class UpgradeNodeUI : MonoBehaviour
{
    public Button button;
    public TMP_Text label;
    public Image background;

    [HideInInspector] public UpgradeNode data;
    [HideInInspector] public NodeState state;

    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f);
    public Color canUpgradeColor = new Color(0.2f, 0.8f, 0.2f);
    public Color maxedColor = new Color(0.8f, 0.8f, 0.2f);
    public Color hiddenColor = new Color(1, 1, 1, 0);

    private UpgradeManager manager;

    public void Setup(UpgradeNode node, UpgradeManager mgr)
    {
        manager = mgr;
        data = node;
        label.text = node.displayName;

        button.onClick.AddListener(() => manager.OpenDetailsPanel(this));
    }

    public void RefreshVisual()
    {
        switch (state)
        {
            case NodeState.Hidden:
                background.color = hiddenColor;
                label.color = new Color(1, 1, 1, 0);
                button.interactable = false;
                break;

            case NodeState.Locked:
                background.color = lockedColor;
                label.color = Color.white;
                button.interactable = false;
                break;

            case NodeState.CanUpgrade:
                background.color = canUpgradeColor;
                label.color = Color.white;
                button.interactable = true;
                break;

            case NodeState.Maxed:
                background.color = maxedColor;
                label.color = Color.white;
                button.interactable = false;
                break;
        }
    }
}
