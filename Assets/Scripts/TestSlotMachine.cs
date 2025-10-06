using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestSlotMachine : MonoBehaviour
{
    [SerializeField] private GameObject reelPrefab;
    
    [SerializeField] private Transform reelsContainer;

    [SerializeField] private HorizontalLayoutGroup reelsGroup;
    [SerializeField] private ReelGroupData[] reelGroupsData;
    private int currentReelGroupDataIndex = 0;
    
    private List<Reel> reels = new List<Reel>();
    
    private SlotData[] slotsData;
    
    private const string SLOTS_DATA_PATH = "SlotsData";

    private void Start()
    {
        slotsData = Resources.LoadAll<SlotData>(SLOTS_DATA_PATH);
        ChangeReelsGroupSpacing();
        DisplayReels();
    }

    private void DisplayReels()
    {
        reels.Clear();

        foreach (Transform t in reelsContainer)
        {
            Destroy(t.gameObject);
        }

        for (int i = 0; i < reelGroupsData[currentReelGroupDataIndex].Count; i++)
        {
            GameObject reelObject = Instantiate(reelPrefab, reelsContainer);
            if (reelObject.TryGetComponent(out Reel reel))
            {
                reel.SetSlotsData(GetRandomSlotsData(reel.SlotsCount));
            }
        }
    }

    public void Spin()
    {
        DisplayReels();
    }

    public void AddReel()
    {
        ChangeReelsCount(true);
        ChangeReelsGroupSpacing();
        DisplayReels();
    }

    public void RemoveReel()
    {
        ChangeReelsCount(false);
        ChangeReelsGroupSpacing();
        DisplayReels();
    }

    private void ChangeReelsCount(bool isIncrease)
    {
        currentReelGroupDataIndex = (currentReelGroupDataIndex + (isIncrease ? 1 : -1) + reelGroupsData.Length) % reelGroupsData.Length;
    }

    private void ChangeReelsGroupSpacing()
    {
        reelsGroup.spacing = reelGroupsData[currentReelGroupDataIndex].Spacing;
    }

    private SlotData[] GetRandomSlotsData(int count)
    {
        SlotData[] randomSlotData = new SlotData[count];
        for (int i = 0; i < randomSlotData.Length; i++)
        {
            randomSlotData[i] = slotsData[Random.Range(0, slotsData.Length)];
        }

        return randomSlotData;
    }
}
