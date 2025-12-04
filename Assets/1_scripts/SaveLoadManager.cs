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
