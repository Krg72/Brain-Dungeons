using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    #region SaveLoad

    public GameSaveData gameSaveData = new GameSaveData();
    private const string SAVE_FILE_NAME = "/savegame.json";
    private const string GLOBAL_SAVE_FILE_NAME = "/savegame_global.json";

    [System.Serializable]
    public class GlobalSaveData
    {
    }

    [System.Serializable]
    public class GameSaveData
    {
        public SaveData saveData = new SaveData();


    }

    [System.Serializable]
    public class SaveData
    {
        public int coins, gems;

        public int CurrentPrestige;
        public int CurrentPrestigePoints;
        public int DestroyedRings;

        public List<int> UpgradeTypeLevels;
        public List<int> BuildingTypeLevels;

        public int CurrentMaxLevel = 1;
        public float CurrentTopRingHealth = 1;

        public List<BallSaveData> ballSaveDatas = new List<BallSaveData>();
    }

    public void SaveGlobalState()
    {
        GlobalSaveData data = new GlobalSaveData();


        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + GLOBAL_SAVE_FILE_NAME, json);
    }

    public void LoadGlobalData()
    {
        GlobalSaveData data = new GlobalSaveData();

        string path = Application.persistentDataPath + GLOBAL_SAVE_FILE_NAME;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<GlobalSaveData>(json);
        }
    }

    public void SaveGame()
    {
        SaveGameData();
        SaveGlobalState();
    }

    void SaveGameData()
    {
        SaveData data = new SaveData();

        data.coins = GameManager.Instance.coins;
        data.gems = GameManager.Instance.gems;
        data.CurrentPrestige = GameManager.Instance.CurrentPrestige;
        data.CurrentPrestigePoints = GameManager.Instance.CurrentPrestigePoints;
        data.DestroyedRings = GameManager.Instance.DestroyedRings;
        data.UpgradeTypeLevels = GameManager.Instance.UpgradeTypeLevels;
        data.BuildingTypeLevels = GameManager.Instance.BuildingTypeLevels;
        data.CurrentMaxLevel = HelixManager.Instance.CurrentMaxLevel;
        data.CurrentTopRingHealth = HelixManager.Instance.CurrentTopRingHealth;

        foreach (var ball in HelixManager.Instance.balls)
        {
            BallSaveData ballData = new BallSaveData();
            ballData.level = ball.level;
            ballData.count = 1;
            data.ballSaveDatas.Add(ballData);
        }

        gameSaveData.saveData = data;

        SaveTofile();

        Debug.Log("saved to file ");
    }

    private void SaveTofile()
    {
        string json = JsonUtility.ToJson(gameSaveData);
        File.WriteAllText(Application.persistentDataPath + SAVE_FILE_NAME, json);
    }

    public void LoadFromFile()
    {
        string path = Application.persistentDataPath + SAVE_FILE_NAME;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
        }
        else
        {
            gameSaveData = new GameSaveData();
        }

        if (gameSaveData != null)
        {
            SaveData data = gameSaveData.saveData;


            GameManager.Instance.coins = data.coins;
            GameManager.Instance.gems = data.gems;
            GameManager.Instance.CurrentPrestige = data.CurrentPrestige;
            GameManager.Instance.CurrentPrestigePoints = data.CurrentPrestigePoints;
            GameManager.Instance.DestroyedRings = data.DestroyedRings;
            GameManager.Instance.UpgradeTypeLevels = data.UpgradeTypeLevels;
            GameManager.Instance.BuildingTypeLevels = data.BuildingTypeLevels;
            HelixManager.Instance.CurrentMaxLevel = data.CurrentMaxLevel;
            HelixManager.Instance.CurrentTopRingHealth = data.CurrentTopRingHealth;

            HelixManager.Instance.SpawnBallsFromSaveData(data.ballSaveDatas);
        }

        Debug.Log("Data loaded from file");

    }


    public IEnumerator saveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SaveGame();
    }
    #endregion
}
