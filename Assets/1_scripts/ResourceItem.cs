using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResourceTypes
{
    coins,
    gems,
    Prestige
}

public class ResourceItem : MonoBehaviour
{
    public ResourceTypes resourceType;
    public Image icon;
    public TMP_Text countTxt;

    void Start()
    {
        icon.sprite = GameUiManager.Instance.ResourceTypeIcons[(int)resourceType];
    }

    void Update()
    {
        switch(resourceType)
        {
            case ResourceTypes.coins:
                countTxt.text = GameUiManager.FormatNumber(GameManager.Instance.coins);
                break;
            case ResourceTypes.gems:
                countTxt.text = GameManager.Instance.gems.ToString();
                break;
            case ResourceTypes.Prestige:
                countTxt.text = GameManager.Instance.CurrentPrestigePoints.ToString();
                break;
        }
    }
}
