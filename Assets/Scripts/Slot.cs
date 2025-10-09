using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class Slot : MonoBehaviour
{
    public SlotData Data { get; private set; }

    [SerializeField] private Image image;
    
    private Sequence winSequence;

    public void SetData(SlotData data)
    {
        Data = data;
        image.sprite = data.IconSprite;
    }
    
    public void PlayWinAnimation()
    {
        // если уже проигрывается — сбрасываем
        if (winSequence != null && winSequence.IsActive())
        {
            winSequence.Kill();
            transform.localScale = Vector3.one;
            image.color = Color.white;
        }

        // создаём новую анимацию
        winSequence = DOTween.Sequence();

        // 1️⃣ — пульсация (scale up / down)
        winSequence.Append(transform.DOScale(1.3f, 0.25f).SetEase(Ease.OutBack));
        winSequence.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.InBack));

        // 2️⃣ — мигание цвета
        winSequence.Join(image.DOColor(Color.yellow, 0.2f).SetLoops(6, LoopType.Yoyo));

        // 3️⃣ — лёгкая вибрация (прыжок)
        winSequence.Join(transform.DOPunchPosition(Vector3.up * 10f, 0.6f, 8, 1f));

        // 4️⃣ — возврат к исходному состоянию
        winSequence.OnComplete(() =>
        {
            image.color = Color.white;
            transform.localScale = Vector3.one;
        });
    }

    private void OnValidate()
    {
        image ??= GetComponent<Image>();
    }
}
