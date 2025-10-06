using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class Slot : MonoBehaviour
{
    public SlotData Data { get; private set; }

    [SerializeField] private Image image;

    public void SetData(SlotData data)
    {
        Data = data;
        image.sprite = data.IconSprite;
    }

    private void OnValidate()
    {
        image ??= GetComponent<Image>();
    }
}
