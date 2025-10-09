using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(RectTransform))]
public class Reel : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [field: SerializeField] public Slot[] Slots { get; private set; }

    public void SetSlotsData(SlotData[] slotsData)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].SetData(slotsData[i]);
        }
    }

    private void OnValidate()
    {
        rectTransform ??= GetComponent<RectTransform>();
    }
}
