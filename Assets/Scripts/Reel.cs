using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(RectTransform))]
public class Reel : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [field: SerializeField] public Slot[] Slots { get; private set; }

    private void OnValidate()
    {
        rectTransform ??= GetComponent<RectTransform>();
    }
}
