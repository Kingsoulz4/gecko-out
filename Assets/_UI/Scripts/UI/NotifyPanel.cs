using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class NotifyPanel : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Text text;
    private void Start()
    {
        rect.gameObject.SetActive(false);
    }
    public void ShowNotify(string content, string key = "", float number = 0)
    {
        DOTween.Kill(this);
        if (string.IsNullOrEmpty(key)) {
            text.text = content;
        }
        
        rect.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        rect.anchoredPosition = new Vector2(0, -200f);
        rect.DOAnchorPosY(400, 4f).SetDelay(1f).SetEase(Ease.OutQuad).SetId(this).OnComplete(()=>
        {
            rect.gameObject.SetActive(false);
        });
        canvasGroup.DOFade(0, 2f).SetDelay(2f).SetEase(Ease.OutQuad).SetId(this);
    }
}
