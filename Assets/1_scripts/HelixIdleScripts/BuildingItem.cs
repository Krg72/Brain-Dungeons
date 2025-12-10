using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingItem : MonoBehaviour
{
    [SerializeField] TMP_Text NameTxt, LevelTxt, ValueTxt, CostTxt, UnlockTxt;
    [SerializeField] Button UnlockObj, UpgradeObj;
    [SerializeField] Image IconImg;

    BuildingData buildingData;

    float currentCost;

    public void SetBuildingItem(BuildingData data)
    {
        buildingData = data;
        SetData();
        // Set IconImg.sprite based on building type or other criteria
    }

    public void SetData()
    {
        // Update buildingData based on current state

        IconImg.sprite = GameUiManager.Instance.BuildingTypeIcons[(int)buildingData.buildingType];
        NameTxt.text = buildingData.buildingName.ToUpper();
        LevelTxt.text = "Level " + GameManager.Instance.BuildingTypeLevels[(int)buildingData.buildingType].ToString();
        ValueTxt.text = BuildingDataSO.GetBuildingDescription(buildingData.buildingType).ToUpper();

        bool isLocked = GameManager.Instance.BuildingTypeLevels[(int)buildingData.buildingType] <= 0;

        if (!isLocked)
        {
            float cost = GameManager.Instance.getBuildingCost(buildingData.buildingType);
            currentCost = cost;

            UnlockObj.gameObject.SetActive(false);
            UpgradeObj.gameObject.SetActive(true);
            
            CostTxt.text = cost.ToString("f0");
            CostTxt.color = GameManager.Instance.gems >= cost ? Color.white : Color.red;
        }
        else
        {
            UnlockObj.gameObject.SetActive(true);
            UpgradeObj.gameObject.SetActive(false);
            UnlockTxt.text = buildingData.unlockCost.ToString("f0");
            UnlockTxt.color = GameManager.Instance.gems >= buildingData.unlockCost ? Color.white : Color.red;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CostTxt.color = GameManager.Instance.gems >= (int)currentCost ? Color.white : Color.red;
        UnlockTxt.color = GameManager.Instance.CurrentPrestigePoints >= buildingData?.unlockCost ? Color.white : Color.red;
    }

    public void OnUnlock()
    {
        if(GameManager.Instance.CurrentPrestigePoints >= buildingData.unlockCost)
        {
            AudioManager.instance.OnButtonClickSound();
            GameManager.Instance.CurrentPrestigePoints -= buildingData.unlockCost;

            GameManager.Instance.BuildingTypeLevels[(int)buildingData.buildingType] = 1;
            
            GameUiManager.Instance.BuildingPanel.RefreshItems();

            GameUiManager.Instance.RefreshUpgradeButtons();
            SaveLoadManager.instance.SaveGame();
        }
        else
        {
            AudioManager.instance.PlayOneShot(AudioClipNames.ButtonFail);
        }
    }

    public void OnUpgrade()
    {
        float cost = GameManager.Instance.getBuildingCost(buildingData.buildingType);
        if (GameManager.Instance.gems >= cost)
        {
            AudioManager.instance.OnButtonClickSound();
            GameManager.Instance.AddGems((int)-cost);

            GameManager.Instance.BuildingTypeLevels[(int)buildingData.buildingType] += 1;
            GameUiManager.Instance.BuildingPanel.RefreshItems();
            GameUiManager.Instance.RefreshUpgradeButtons();
            SaveLoadManager.instance.SaveGame();
        }
        else
        {
            AudioManager.instance.PlayOneShot(AudioClipNames.ButtonFail);
        }
    }
}
