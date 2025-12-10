using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coins, gems;
    public int DestroyedRings;

    public int CurrentPrestige;
    public int CurrentPrestigePoints;
    public int[] RequiredRingsToPrestige;

    public BuildingDataSO buildingDataSO;
    public List<int> UpgradeTypeLevels;
    public List<int> BuildingTypeLevels;

    public Material flashMaterial;

    [SerializeField] public UpgradeManager UpgradeManager;

    //[SerializeField] ParticleSystem ChainLoop, ChainBomb, FlameThrower, SmokeBomb, SwordTornado, Tornado;

    private void Awake()
    {
        Instance = this;

         if (PlayerPrefs.GetInt("FirstTime", 1) == 1)
        {
            PlayerPrefs.SetInt("FirstTime", 0);

            UpgradeTypeLevels = new List<int>();
            BuildingTypeLevels = new List<int>();
            foreach (var upgradeType in Enum.GetValues(typeof(UpgradeType)).Cast<UpgradeType>())
            {
                UpgradeTypeLevels.Add(0);
            }
            foreach (var buildingType in Enum.GetValues(typeof(BuildingType)).Cast<BuildingType>())
            {
                BuildingTypeLevels.Add(0);
            }

            SaveLoadManager.instance.SaveGame();
        }
        else
        {
            SaveLoadManager.instance.LoadFromFile();
        }

    }

    private void Start()
    {
        // LoadNextLevel(false);
    }

    private void Update()
    {
    }

    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public void AddGems(int amount)
    {
        gems += amount;
    }

    private void StartLevel()
    {
        // In future you can reset player health, load waves, etc.
        Debug.Log("Level started.");
    }

    public void DoPrestige()
    {
        int rewardCoins = (CurrentPrestige + 1) * 1000;
        int rewardPoints = (CurrentPrestige + 1) * 5;

        CurrentPrestigePoints += rewardPoints;
        coins += rewardCoins;

        DestroyedRings -= RequiredRingsToPrestige[CurrentPrestige];
        CurrentPrestige += 1;

        //reset ball upgrade levels and destory helic and regenerate new one
        for (int i = 0; i < UpgradeTypeLevels.Count; i++)
        {
            UpgradeTypeLevels[i] = 0;
        }

        HelixManager.Instance.StartPrestigeSequence();

        GameUiManager.Instance.RefreshUpgradeButtons();
    }

    public int GetBallLevelDamage(int level)
    {
        if (level < 1)
        {
            Debug.LogError("Level must be 1 or higher.");
            return 0;
        }

        return (int)Mathf.Pow(2, level - 1);
    }

    public float GetIncomePerBounce()
    {
        float incomeMult = getBuildingValue(BuildingType.Income);
        return getBallUpgradeValue(BallUpgradeType.Income) * incomeMult;
    }

    public int GetRequiredPrestigeRings()
    {
        if (CurrentPrestige < RequiredRingsToPrestige.Length)
        {
            return RequiredRingsToPrestige[CurrentPrestige];
        }
        else
        {
            // If prestige level exceeds defined array, return a high value or calculate based on a formula
            return RequiredRingsToPrestige.Last() + (CurrentPrestige - RequiredRingsToPrestige.Length + 1) * RequiredRingsToPrestige.Last();
        }
    }

    public float getBallUpgradeValue(BallUpgradeType upgradeType)
    {
        int level = UpgradeTypeLevels[(int)upgradeType];

        UpgradeData upgradeData = buildingDataSO.upgradeDataList.Find(bd => bd.upgradeType == upgradeType);

        if (upgradeData != null)
        {
            return upgradeData.baseValue + (upgradeData.IncreasePerLevel * level);
        }
        else
        {
            Debug.LogWarning("Upgrade data not found for type: " + upgradeType);
            return 0f;
        }
    }

    public float getBuildingValue(BuildingType buildingType)
    {
        int level = BuildingTypeLevels[(int)buildingType];
        BuildingData buildingData = buildingDataSO.buildingDataList.Find(bd => bd.buildingType == buildingType);
        if (buildingData != null)
        {
            return buildingData.baseValue + (buildingData.IncreasePerLevel * level);
        }
        else
        {
            Debug.LogWarning("Building data not found for type: " + buildingType);
            return 0f;
        }
    }

    public float getBuildingCost(BuildingType buildingType)
    {
        int level = BuildingTypeLevels[(int)buildingType];
        BuildingData buildingData = buildingDataSO.buildingDataList.Find(bd => bd.buildingType == buildingType);
        if (buildingData != null)
        {
            return buildingData.baseCost * Mathf.Pow(buildingData.costMultiplier, level);
        }
        else
        {
            Debug.LogWarning("Building data not found for type: " + buildingType);
            return 0f;
        }
    }

    public float getBallUpgradeCost(BallUpgradeType upgradeType)
    {
        int level = UpgradeTypeLevels[(int)upgradeType];
        UpgradeData upgradeData = buildingDataSO.upgradeDataList.Find(bd => bd.upgradeType == upgradeType);
        if (upgradeData != null)
        {
            return upgradeData.baseCost * Mathf.Pow(upgradeData.costMultiplier, level);
        }
        else
        {
            Debug.LogWarning("Upgrade data not found for type: " + upgradeType);
            return 0f;
        }
    }
}
