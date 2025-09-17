using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollScreenReferences))]
public class ScrollScreenHorizontal : ScrollRect
{
    private ScrollScreenReferences reference;
    Sequence tween;
    private Vector2 lastPosition;
    private int previousPanel;
    public int currentPanel;
    private float sizeXCanvas;
    private float sizeButton;
    public event Action OnInitialized;
    public ScrollScreenReferences Reference => reference;
    protected override void Awake()
    {
        base.Awake();
        reference = GetComponent<ScrollScreenReferences>();
        horizontal = true;
        vertical = false;
    }
    protected override void Start()
    {
        base.Start();
        DOVirtual.DelayedCall(0.05f, () =>
        {
            tween = DOTween.Sequence();
            tween.SetId(this);
            sizeXCanvas = UIManager.Instance.GetCanvasSize().x;
            sizeButton = sizeXCanvas / (reference.component.Length - 1 + reference.ButtonScaleUpPercent);
            for (int i = 0; i < reference.component.Length; i++)
            {
                var component = reference.component[i];
                component.Panel.Index = i;
                var RectTransform = component.Panel.GetComponent<RectTransform>();
                RectTransform.sizeDelta = new Vector2(sizeXCanvas, RectTransform.sizeDelta.y);
                RectTransform.anchoredPosition = new Vector2(sizeXCanvas * i, 0);

                var RectTransformButton = component.Button.GetComponent<RectTransform>();
                if (i == reference.StartIndex)
                {
                    RectTransformButton.sizeDelta = new Vector2(sizeButton * reference.ButtonScaleUpPercent, RectTransformButton.sizeDelta.y);
                    reference.ImageSlider.sizeDelta = RectTransformButton.sizeDelta;
                }
                else
                {
                    RectTransformButton.sizeDelta = new Vector2(sizeButton, RectTransformButton.sizeDelta.y);
                }
            }
            var startPanel = reference.component[reference.StartIndex].Panel.GetComponent<RectTransform>();
            int countPanel = 0;
            foreach(var panel in reference.component)
            {
                if (panel.Panel.gameObject.activeSelf)
                {
                    countPanel++;
                }
            }
            content.sizeDelta = new Vector2(sizeXCanvas * countPanel, content.sizeDelta.y);
            content.anchoredPosition = new Vector2(-startPanel.anchoredPosition.x, content.anchoredPosition.y);
            currentPanel = reference.StartIndex;
            previousPanel = currentPanel;
            LayoutRebuilder.ForceRebuildLayoutImmediate(reference.ImageSlider.transform.parent.GetComponent<RectTransform>());
            var currentButton = reference.component[currentPanel].Button.GetComponent<RectTransform>();
            reference.ImageSlider.anchoredPosition = currentButton.anchoredPosition;
            OnInitialized?.Invoke();
        }).SetId(this);
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        lastPosition = content.anchoredPosition;
        previousPanel = currentPanel;
        tween?.Kill();
        StopMovement();
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        var del = lastPosition.x - content.anchoredPosition.x;
        var delMax = reference.DeltaMaxToNextPanel;
        var isNext = Mathf.Abs(velocity.x) > reference.VelocityMaxToNextPanel;

        int targetPanel = currentPanel;

        if (currentPanel + 1 < reference.component.Length && (del > delMax || (isNext && del > 0)))
        {
            targetPanel = currentPanel + 1;
        }
        else if (currentPanel > 0 && currentPanel < reference.component.Length && (del < -delMax || (isNext && del < 0)))
        {
            targetPanel = currentPanel - 1;
        }

        // Check if target panel is locked
        if (targetPanel >= 0 && targetPanel < reference.component.Length)
        {
            var targetButton = reference.component[targetPanel].Button;
            if (targetButton is MenuTabButton tabButton && tabButton.IsLocked)
            {
                targetPanel = currentPanel;
            }
        }

        currentPanel = targetPanel;
        UpdatePanel();
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        var currentButton = reference.component[currentPanel].Button.GetComponent<RectTransform>();
        float percent = -(currentPanel * sizeXCanvas + content.anchoredPosition.x) / sizeXCanvas;
        reference.ImageSlider.anchoredPosition = currentButton.anchoredPosition + new Vector2(percent * reference.ImageSlider.sizeDelta.x, 0);
    }

    protected override void OnDestroy()
    {
        tween?.Kill();
        this.DOKill();
    }
    public void ChangePanel(int idx)
    {
        previousPanel = currentPanel;
        currentPanel = idx;

        UpdatePanel();
    }
    private void UpdatePanel()
    {
        if (previousPanel != currentPanel)
        {
            ScrollScreenReferences.OnChangePanel?.Invoke(currentPanel);
        }
        var x = content.GetChild(currentPanel).GetComponent<RectTransform>().anchoredPosition.x;
        tween?.Kill();
        tween = DOTween.Sequence();
        StopMovement();
        tween.Insert(0, content.DOAnchorPosX(-x, reference.DurationAutoScroll).SetEase(Ease.OutQuad).OnUpdate(() =>
        {
            lastPosition = content.anchoredPosition;
        }));
        /// Button
        var sizeX = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x;
        var RectTransform1 = reference.component[currentPanel].Button.GetComponent<RectTransform>();
        tween.Insert(0, reference.ImageSlider.DOAnchorPosX(sizeButton * currentPanel, reference.DurationAutoScroll).SetEase(Ease.OutQuad));
        if (previousPanel != currentPanel)
        {
            var RectTransform2 = reference.component[previousPanel].Button.GetComponent<RectTransform>();
            //RectTransform2.DOSizeDelta(new Vector2(sizeButton, RectTransform2.sizeDelta.y), 0.3f).SetEase(Ease.OutQuad);
            //RectTransform1.DOSizeDelta(new Vector2(sizeButton * reference.ButtonScaleUpPercent, RectTransform2.sizeDelta.y), reference.DurationAutoScroll).SetEase(Ease.OutQuad);
        }
    }
}

