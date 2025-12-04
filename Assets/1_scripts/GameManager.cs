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

    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] public UpgradeManager UpgradeManager;

    private void Awake()
    {
        Instance = this;

        // if (PlayerPrefs.GetInt("FirstTime", 1) == 1)
        // {
        //     PlayerPrefs.SetInt("FirstTime", 0);
        //     SaveLoadManager.instance.SaveGame();
        // }
        // else
        // {
        //     SaveLoadManager.instance.LoadFromFile();
        // }
    }

    private void Start()
    {
        // LoadNextLevel(false);
    }

    private void Update()
    {
    }

    private void StartLevel()
    {
        // In future you can reset player health, load waves, etc.
        Debug.Log("Level started.");
    }

}
