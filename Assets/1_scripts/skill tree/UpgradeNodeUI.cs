using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
    public GameObject NodeObject;
    public Image background, iconImage;

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

        button.onClick.AddListener(() => OnClick());
    }

    public void OnClick()
    {
        // Stop any previous animation and snap to final rotation
        transform.DOKill(true);

        // Reset rotation to ensure no drift
        transform.localRotation = Quaternion.identity;

        // A nice rotational punch on Z axis
        transform.DOPunchRotation(
            punch: new Vector3(0, 0, 20f),   // degrees of punch
            duration: 0.25f,                 // how long punch lasts
            vibrato: 12,                     // how many shakes
            elasticity: 0.6f                 // how much punch retains energy
        );

        manager.OpenDetailsPanel(this);
    }

    public void RefreshVisual()
    {
        switch (state)
        {
            case NodeState.Hidden:
                background.color = hiddenColor;
                button.interactable = false;
                break;

            case NodeState.Locked:
                background.color = lockedColor;
                button.interactable = true;
                break;

            case NodeState.CanUpgrade:
                background.color = canUpgradeColor;
                button.interactable = true;
                break;

            case NodeState.Maxed:
                background.color = maxedColor;
                button.interactable = false;
                break;
        }

        NodeObject.SetActive(state != NodeState.Hidden);
    }
}
