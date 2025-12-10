using UnityEngine;
using UnityEngine.UI;

public class UpgradeConnectorUI : MonoBehaviour
{
    public UpgradeNode parentNode;
    public UpgradeNode childNode;

    public Image lineImage;

    public void Refresh(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
