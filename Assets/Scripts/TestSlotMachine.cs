using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TestSlotMachine : MonoBehaviour
{
    [SerializeField] private GameObject reelPrefab;
    [SerializeField] private Transform reelsContainer;

    [SerializeField] private HorizontalLayoutGroup reelsLayoutGroup;
    [SerializeField] private ReelGroupData[] reelGroupsData;
    private int currentReelGroupDataIndex = 0;

    private Slot[,] slots;

    private List<Reel> reels = new List<Reel>();

    private SlotData[] slotsData;
    private const string SLOTS_DATA_PATH = "SlotsData";

    private Coroutine spinCoroutine;

    private void Start()
    {
        slotsData = Resources.LoadAll<SlotData>(SLOTS_DATA_PATH);
        UpdateReels();
        SetRandomSlotsData();
    }

    private void UpdateReels()
    {
        reels.Clear();

        if (reelsContainer.childCount > 0)
        {
            foreach (Transform child in reelsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        int reelCount = reelGroupsData[currentReelGroupDataIndex].Count;
        for (int i = 0; i < reelCount; i++)
        {
            GameObject reelObject = Instantiate(reelPrefab, reelsContainer);

            if (reelObject.TryGetComponent(out Reel reel))
            {
                reels.Add(reel);
            }
        }

        UpdateSlots();
    }

    private void UpdateSlots()
    {
        int columns = reels.Count;
        int rows = reels[0].Slots.Length;

        slots = new Slot[columns, rows];

        for (int x = 0; x < columns; x++)
        {
            Slot[] reelSlots = reels[x].Slots;

            for (int y = 0; y < rows; y++)
            {
                slots[x, y] = reelSlots[y];
            }
        }

        Debug.Log($"Created {columns} reels × {rows} slots grid successfully!");
    }

    private void SetRandomSlotsData()
    {
        int columns = slots.GetLength(0);
        int rows = slots.GetLength(1);

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                slots[i, j].SetData(GetRandomSlotData());
            }
        }

        PrintGrid(slots);
    }

    public void Spin()
    {
        if (spinCoroutine == null)
        {
            spinCoroutine = StartCoroutine(SpinCoroutine());
        }
    }

    private IEnumerator SpinCoroutine()
    {
        SetRandomSlotsData();

        yield return new WaitForSeconds(1f);

        CheckMatches();

        spinCoroutine = null;
    }

    private SlotData GetRandomSlotData()
    {
        int totalWeight = 0;
        foreach (var s in slotsData)
            totalWeight += Mathf.Max(1, s.RareAmount);

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var s in slotsData)
        {
            cumulative += Mathf.Max(1, s.RareAmount);
            if (roll < cumulative)
                return s;
        }

        return slotsData[slotsData.Length - 1];
    }


    private void CheckMatches()
    {
        int columns = slots.GetLength(0);
        int rows = slots.GetLength(1);

        int totalWin = 0;

        // Перебираем все строки
        for (int y = 0; y < rows; y++)
        {
            List<Slot> currentLine = new List<Slot>();

            // Берем строку целиком
            for (int x = 0; x < columns; x++)
            {
                currentLine.Add(slots[x, y]);
            }

            // Проверяем совпадения в этой строке
            int win = CheckLineMatches(currentLine);
            totalWin += win;
        }

        if (totalWin > 0)
        {
            Debug.Log($"💰 TOTAL WIN: {totalWin}");
        }
        else
        {
            Debug.Log("No win this time!");
        }
    }

    private int CheckLineMatches(List<Slot> line)
    {
        int totalWin = 0;
        List<Slot> currentMatch = new List<Slot>();

        SlotType? baseType = null;

        for (int i = 0; i < line.Count; i++)
        {
            Slot slot = line[i];
            SlotData data = slot.Data;

            if (currentMatch.Count == 0)
            {
                currentMatch.Add(slot);
                baseType = data.IsWild ? null : data.SlotType;
            }
            else
            {
                bool isMatch =
                    data.IsWild ||
                    baseType == null ||
                    data.SlotType == baseType;

                if (isMatch)
                {
                    currentMatch.Add(slot);
                    if (baseType == null && !data.IsWild)
                        baseType = data.SlotType;
                }
                else
                {
                    totalWin += ProcessMatch(currentMatch);
                    currentMatch.Clear();
                    currentMatch.Add(slot);
                    baseType = data.IsWild ? null : data.SlotType;
                }
            }
        }

        totalWin += ProcessMatch(currentMatch);

        return totalWin;
    }

    private int ProcessMatch(List<Slot> match)
    {
        if (match.Count < 3)
            return 0;

        // Проверяем, все ли Wild
        bool allWild = match.All(s => s.Data.IsWild);

        SlotData baseData;

        if (allWild)
        {
            // 🔹 Выбираем случайный не-Wild символ из всех доступных
            var nonWildSymbols = slotsData.Where(s => !s.IsWild && !s.IsMultiplier).ToArray();
            baseData = nonWildSymbols[Random.Range(0, nonWildSymbols.Length)];

            Debug.Log($"✨ All Wilds turned into: {baseData.SlotType}");

            // Анимация превращения всех Wild в выбранный символ
            foreach (Slot slot in match)
            {
                slot.TransformToSymbol(baseData.IconSprite);
                slot.SetData(baseData); // обновляем данные, чтобы считался как выбранный тип
            }

            // Можно добавить небольшую паузу перед анимацией выигрыша
            DOVirtual.DelayedCall(0.6f, () =>
            {
                foreach (Slot slot in match)
                    slot.PlayWinAnimation();
            });

            int payout = baseData.GetPayout(match.Count);
            Debug.Log($"💎 All Wilds became {baseData.SlotType}! Win: {payout}");
            return payout;
        }

        // 🔹 Обычная логика, если не все Wild
        baseData = match.FirstOrDefault(s => !s.Data.IsWild)?.Data ?? match[0].Data;
        int matchCount = match.Count;
        int payoutNormal = baseData.GetPayout(matchCount);

        bool hasWilds = false;
        foreach (Slot slot in match)
        {
            if (slot.Data.IsWild)
            {
                hasWilds = true;
                slot.TransformToSymbol(baseData.IconSprite);
            }
        }

        float delayBeforeWinAnim = hasWilds ? 0.6f : 0f;

        DOVirtual.DelayedCall(delayBeforeWinAnim, () =>
        {
            foreach (Slot slot in match)
                slot.PlayWinAnimation();
        });

        Debug.Log($"🔥 Match x{matchCount} of {baseData.SlotType}: +{payoutNormal}");
        return payoutNormal;
    }


    private void PrintGrid(Slot[,] grid)
    {
        int columns = grid.GetLength(0);
        int rows = grid.GetLength(1);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== SLOT GRID ===");

        for (int y = 0; y < rows; y++)
        {
            sb.Append($"Row {y}: ");

            for (int x = 0; x < columns; x++)
            {
                Slot slot = grid[x, y];
                sb.Append($"{slot.Data.SlotType.ToString().PadRight(50)} ");
            }

            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}