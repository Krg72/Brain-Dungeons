using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum BallUpgradeType
{
    AddBall,
    MergeBall,
    Income
}

public class BallUpgradeBtn : MonoBehaviour
{
    public BallUpgradeType upgradeType;

    [SerializeField] TMPro.TMP_Text levelText, PriceTxt, NameTxt;
    [SerializeField] Image icon;

    [SerializeField] GameObject NotBuyableOverlay;

    int currentLevel = 1;
    float price;

    void Start()
    {
        SetData();
    }

    public void SetData()
    {
        currentLevel = GameManager.Instance.UpgradeTypeLevels[(int)upgradeType] + 1;
        price = GameManager.Instance.getBallUpgradeCost(upgradeType);

        NameTxt.text = GetUpgradeName(upgradeType);
        levelText.text = "LEVEL " + currentLevel.ToString();

        PriceTxt.text = GameUiManager.FormatNumber(price);

        icon.sprite = GameUiManager.Instance.UpgradeTypeIcons[(int)upgradeType];

        NotBuyableOverlay.SetActive(GameManager.Instance.coins < price);

        if (upgradeType == BallUpgradeType.MergeBall)
        {
            if (HelixManager.Instance.CanMerge() && GameManager.Instance.coins >= price)
            {
                NotBuyableOverlay.SetActive(false);
            }
            else
            {
                NotBuyableOverlay.SetActive(true);
            }
        }

        if (upgradeType == BallUpgradeType.AddBall)
        {
            if (HelixManager.Instance.balls.Count < GameManager.Instance.getBuildingValue(BuildingType.MaxBalls) && GameManager.Instance.coins >= price)
            {
                NotBuyableOverlay.SetActive(false);
            }
            else
            {
                NotBuyableOverlay.SetActive(true);
            }
        }
    }

    private void Update()
    {
        
    }

    string GetUpgradeName(BallUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case BallUpgradeType.AddBall:
                return "ADD";
            case BallUpgradeType.MergeBall:
                return "MERGE";
            case BallUpgradeType.Income:
                return "INCOME";
            default:
                return upgradeType.ToString();
        }
    }

    public void OnClick()
    {
        transform.DOKill();
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        if (NotBuyableOverlay.activeInHierarchy)
        {
            AudioManager.instance.PlayOneShot(AudioClipNames.ButtonFail);
            transform.DOPunchRotation(new Vector3(0, 0, 20), 0.2f, 10, 1);

            if(upgradeType == BallUpgradeType.AddBall)
            {
                if (HelixManager.Instance.balls.Count >= GameManager.Instance.getBuildingValue(BuildingType.MaxBalls))
                {
                    GameUiManager.Instance.MaxBallsTxt.transform.localRotation = Quaternion.identity;
                    GameUiManager.Instance.MaxBallsTxt.transform.DOPunchPosition(new Vector3(0, 10, 0), 0.2f, 10, 1);
                }
            }

            GameUiManager.Instance.RefreshUpgradeButtons();
            return;
        }
        else
        {
            AudioManager.instance.PlayOneShot(AudioClipNames.ButtonClick);
            transform.DOScale(Vector3.one * 0.9f, 0.1f).OnComplete(() =>
            {
                transform.DOScale(Vector3.one, 0.1f);
            });
        }

        GameManager.Instance.UpgradeTypeLevels[(int)upgradeType] += 1;
        GameManager.Instance.AddCoins(-(int)price);

        switch (upgradeType)
        {
            case BallUpgradeType.AddBall:
                HelixManager.Instance.SpawnBall();
                break;
            case BallUpgradeType.MergeBall:
                HelixManager.Instance.MergeAny();
                break;
            case BallUpgradeType.Income:
                break;
        }

        GameUiManager.Instance.RefreshUpgradeButtons();

        SaveLoadManager.instance.SaveGame();
    }

}
