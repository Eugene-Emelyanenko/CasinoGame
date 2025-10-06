using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(RectTransform))]
public class Reel : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Slot[] slots;
    public int SlotsCount => slots.Length;

    public void SetSlotsData(SlotData[] slotsData)
    {
        for (int i = 0; i < SlotsCount; i++)
        {
            slots[i].SetData(slotsData[i]);
        }
    }

    private void OnValidate()
    {
        rectTransform ??= GetComponent<RectTransform>();
    }
}
