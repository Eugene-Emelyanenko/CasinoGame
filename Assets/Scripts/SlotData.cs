using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SlotData", menuName = "Slot Data", order = 0)]
public class SlotData : ScriptableObject
{
    [field: Header("Base Settings")]
    [field: SerializeField] public SlotType SlotType { get; private set; }

    [field: SerializeField] public Sprite IconSprite { get; private set; }

    [field: SerializeField, Range(1, 100), Tooltip("Symbol rarity weight (higher = appears more often)")]
    public int RareAmount { get; private set; } = 25;

    [Header("Payout Settings")] 
    [Tooltip("Reward for 3 consecutive matches")] 
    [SerializeField] private int payout3;
    
    [Tooltip("Reward for 4 consecutive matches")]
    [SerializeField] private int payout4; 
    
    [Tooltip("Reward for 5 consecutive matches")]
    [SerializeField] private int payout5;

    [field: Header("Special Flags")]
    [field: SerializeField] public bool IsWild { get; private set; }
    [field: SerializeField] public bool IsMultiplier { get; private set; }

    [field: SerializeField, Tooltip("Minimum multiplier value (if this symbol is a multiplier)")]
    public int MultiplierMin { get; private set; }

    [field: SerializeField, Tooltip("Maximum multiplier value (if this symbol is a multiplier)")]
    public int MultiplierMax { get; private set; }

    public int GetPayout(int matchCount)
    {
        return matchCount switch
        {
            3 => payout3,
            4 => payout4,
            5 => payout5,
            _ => LogInvalidMatchCount(matchCount)
        };
    }

    private int LogInvalidMatchCount(int matchCount)
    {
        Debug.LogWarning($"[SlotData] Invalid match count: {matchCount}. Expected 3, 4, or 5.");
        return 0;
    }
}