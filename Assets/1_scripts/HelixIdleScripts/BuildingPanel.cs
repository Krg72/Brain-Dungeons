using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanel : MonoBehaviour
{
    [SerializeField] BuildingItem buildingItemPrefab;
    [SerializeField] Transform contentTransform;
    [SerializeField] BuildingDataSO buildingDataSO;

    [SerializeField] GameObject LabPanel;

    [SerializeField] ScrollRect ScrollRect;

    public List<BuildingItem> buildingItemList = new List<BuildingItem>();

    public void SpawnItems()
    {
        foreach (BuildingData data in buildingDataSO.buildingDataList)
        {
            BuildingItem newItem = Instantiate(buildingItemPrefab, contentTransform);
            newItem.SetBuildingItem(data);
            buildingItemList.Add(newItem);
        }
    }

    public void RefreshItems()
    {
        foreach (BuildingItem item in buildingItemList)
        {
            item.SetData();
        }
    }

    void Start()
    {
        SpawnItems();
    }

    void Update()
    {
        
    }

    public void OnOpenLab()
    {
        AudioManager.instance.OnButtonClickSound();
        LabPanel.SetActive(true);
        ScrollRect.verticalNormalizedPosition = 1;
    }

    public void CloseLab()
    {
        AudioManager.instance.OnButtonClickSound();
        LabPanel.SetActive(false);
    }
}
