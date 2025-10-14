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
        if (winSequence != null && winSequence.IsActive())
        {
            winSequence.Kill();
            transform.localScale = Vector3.one;
            image.color = Color.white;
        }

        winSequence = DOTween.Sequence();

        winSequence.Append(transform.DOScale(1.3f, 0.25f).SetEase(Ease.OutBack));
        winSequence.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.InBack));

        winSequence.Join(image.DOColor(Color.yellow, 0.2f).SetLoops(6, LoopType.Yoyo));
        winSequence.Join(transform.DOPunchPosition(Vector3.up * 10f, 0.6f, 8, 1f));

        winSequence.OnComplete(() =>
        {
            image.color = Color.white;
            transform.localScale = Vector3.one;
        });
    }
    
    public void TransformToSymbol(Sprite targetSprite)
    {
        Sequence seq = DOTween.Sequence();

        // Немного "взрываем" Wild перед сменой
        seq.Append(transform.DOScale(1.4f, 0.2f).SetEase(Ease.OutBack));
        seq.Append(image.DOFade(0f, 0.15f));

        // Меняем спрайт, когда Wild "исчез"
        seq.AppendCallback(() =>
        {
            image.sprite = targetSprite;
        });

        // Возвращаем его обратно с новой иконкой
        seq.Append(image.DOFade(1f, 0.15f));
        seq.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.InOutBack));
    }

    private void OnValidate()
    {
        image ??= GetComponent<Image>();
    }
}
