using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    Damage,
    Speed,
    Income,
    CriticalChance,
    CriticalFactor,
    ChainLoop,
    ChainBomb,
    FlameThrower,
    SmokeBomb,
    SwordTornado,
    Tornado,
    MaxBalls
}

[CreateAssetMenu(fileName = "BuildingDataSO", menuName = "Scriptable Objects/BuildingDataSO")]
public class BuildingDataSO : ScriptableObject
{
    public List<BuildingData> buildingDataList;
    public List<UpgradeData> upgradeDataList;

    public static string GetBuildingDescription(BuildingType type)
    {
        // Example description generation logic
        switch (type)
        {
            case BuildingType.Damage:
                return "Increase damage";
            case BuildingType.Speed:
                return "Increase bounce speed";
            case BuildingType.Income:
                return "Increase income";
            case BuildingType.CriticalChance:
                return "Increase critial hit chance";
            case BuildingType.CriticalFactor:
                return "Increase crtical hit factor";
            case BuildingType.ChainLoop:
                return "Get a looping chain";
            case BuildingType.ChainBomb:
                return "drop a chain bomb";
            case BuildingType.FlameThrower:
                return "Get Flame thrower";
            case BuildingType.SmokeBomb:
                return "Drop a smoke bomb";
            case BuildingType.SwordTornado:
                return "Get rotating swords";
            case BuildingType.Tornado:
                return "spwan tornado";
            case BuildingType.MaxBalls:
                return "max balls";
            default:
                return "No description available.";
        }
    }
}

[Serializable]
public class BuildingData
{
    public BuildingType buildingType;
    public string buildingName;

    public int unlockCost;

    public float baseCost;
    public float costMultiplier;

    // Upgrade effects
    public float baseValue;
    public float IncreasePerLevel;

    public int maxLevel;
}

[Serializable]
public class UpgradeData
{
    public BallUpgradeType upgradeType;
    public string upgradeName;

    public float baseCost;
    public float costMultiplier;

    // Upgrade effects
    public float baseValue;
    public float IncreasePerLevel;
}