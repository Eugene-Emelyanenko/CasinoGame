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
                reel.SetSlotsData(GetRandomSlotsData(reel.Slots.Length));
                
                reels.Add(reel);
            }
        }
    }

    public void Spin()
    {
        StartCoroutine(SpinCoroutine());
    }

    private IEnumerator SpinCoroutine()
    {
        DisplayReels();

        yield return new WaitForSeconds(1f);
        
        CheckMatches();
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
        SlotData[] result = new SlotData[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = GetWeightedRandomSlot();
        }

        return result;
    }

    private SlotData GetWeightedRandomSlot()
    {
        if (slotsData == null || slotsData.Length == 0)
        {
            Debug.LogError("No SlotData assets found in Resources/SlotsData.");
            return null;
        }

        int totalWeight = 0;
        foreach (var slot in slotsData)
            totalWeight += Mathf.Max(1, slot.RareAmount);

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var slot in slotsData)
        {
            cumulative += slot.RareAmount;
            if (roll < cumulative)
                return slot;
        }
        
        return slotsData[Random.Range(0, slotsData.Length)];
    }

    public void CheckMatches()
    {
        if (reels.Count == 0)
        {
            Debug.LogWarning("No reels to check!");
            return;
        }

        int reelsCount = reels.Count;
        int rows = reels[0].Slots.Length;

        // создаём двумерный массив для быстрого доступа
        Slot[,] grid = new Slot[reelsCount, rows];

        for (int x = 0; x < reelsCount; x++)
        {
            Slot[] reelSlots = reels[x].Slots;
            for (int y = 0; y < rows; y++)
            {
                grid[x, y] = reelSlots[y];
            }
        }
        
        PrintGrid(grid);

        // хэшсет для хранения всех выигравших слотов
        HashSet<Slot> winningSlots = new HashSet<Slot>();
        int totalWin = 0;

        // проверяем совпадения в каждой строке
        for (int y = 0; y < rows; y++)
        {
            SlotData currentSymbol = grid[0, y].Data;
            int consecutive = 1;

            for (int x = 1; x < reelsCount; x++)
            {
                if (grid[x, y].Data == currentSymbol)
                {
                    consecutive++;
                }
                else
                {
                    // проверяем серию до обрыва
                    if (consecutive >= 3)
                    {
                        int payout = currentSymbol.GetPayout(consecutive);
                        totalWin += payout;

                        for (int back = x - consecutive; back < x; back++)
                            winningSlots.Add(grid[back, y]);

                        Debug.Log($"WIN: {currentSymbol.SlotType} x{consecutive} → +{payout}");
                    }

                    currentSymbol = grid[x, y].Data;
                    consecutive = 1;
                }
            }

            // последняя серия в строке
            if (consecutive >= 3)
            {
                int payout = currentSymbol.GetPayout(consecutive);
                totalWin += payout;

                for (int back = reelsCount - consecutive; back < reelsCount; back++)
                    winningSlots.Add(grid[back, y]);

                Debug.Log($"WIN: {currentSymbol.SlotType} x{consecutive} → +{payout}");
            }
        }

        // проигрываем анимации выигравших слотов
        if (totalWin > 0)
        {
            Debug.Log($"💰 Total Win: {totalWin}");
            foreach (var slot in winningSlots)
                slot.PlayWinAnimation();
        }
        else
        {
            Debug.Log("No win this time!");
        }
    }

    private void PrintGrid(Slot[,] grid)
    {
        if (grid == null)
        {
            Debug.LogError("Grid is null — nothing to print!");
            return;
        }

        int reelsCount = grid.GetLength(0);
        int rows = grid.GetLength(1);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== SLOT GRID ===");

        for (int y = 0; y < rows; y++)
        {
            sb.Append($"Row {y}: ");

            for (int x = 0; x < reelsCount; x++)
            {
                var slot = grid[x, y];
                if (slot != null && slot.Data != null)
                    sb.Append($"{slot.Data.SlotType.ToString().PadRight(50)} ");
                else
                    sb.Append("[NULL] ".PadRight(10));
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
