using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUiManager : MonoBehaviour
{
    public static GameUiManager Instance;

    [Header("References")]
    public DynamicTextData textData_damage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
    }

    void Update()
    {
        
    }

    public void SpawnFloatingText(DynamicTextData data, string DataString, Vector3 position)
    {
        if (!SettingManager.instance.isFloatTextOn) return;

        if (transform != null)
        {
            position.x += (Random.value - 0.5f);
            position.y += (Random.value);
            position.z += (Random.value - 0.5f);
            DynamicTextManager.CreateText(position, DataString, data);
        }
    }
}
