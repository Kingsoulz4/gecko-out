using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class MenuTabButton : ScrollScreenButton
{
    public Button button;
    private MenuTabSystem tabSystem;
    [SerializeField] Image imgSpr;
    [SerializeField] Sprite[] statusSprs;
    [SerializeField] RectTransform nameRect;
    [SerializeField] RectTransform iconRect;
    [SerializeField] RectTransform iconDeSelected;

    [SerializeField] SkeletonGraphic skeleton;

    [SerializeField, SpineAnimation]  string[] anims;

    [SerializeField] bool isLocked;

    [SerializeField] private bool canInActiveIcon;

    public int index;

    public bool IsLocked { get => isLocked;}

    public void Initialize(MenuTabSystem tabSystem)
    {
        this.tabSystem = tabSystem;
        button.onClick.AddListener(OnSelectTab);
    }
    public void Select()
    {
        imgSpr.sprite = statusSprs[1];
        nameRect.gameObject.SetActive(true);
        nameRect.anchoredPosition = Vector2.zero;
        if(canInActiveIcon)
        {
            iconRect.gameObject.SetActive(true);
            iconDeSelected.gameObject.SetActive(false);
        }    
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.DOAnchorPosY(50f, 0.15f).SetEase(Ease.OutQuad);
        iconRect.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
        nameRect.DOAnchorPosY(-84f, 0.15f).SetEase(Ease.OutQuad);
        if (skeleton)
        {
            //skeleton.AnimationState.AddAnimation(0, anims[0], false, 0);
            //skeleton.AnimationState.AddAnimation(0, anims[1], true, 0);
        }
    }
    public void Deselect()
    {
        imgSpr.sprite = statusSprs[0];
        nameRect.gameObject.SetActive(false);
        if (canInActiveIcon)
        {
            iconRect.gameObject.SetActive(false);
            iconDeSelected.gameObject.SetActive(true);
        }
        nameRect.anchoredPosition = new Vector2(55f, 0);
        nameRect.anchoredPosition = Vector2.zero;
        iconRect.DOAnchorPosY(0f, 0.15f).SetEase(Ease.OutQuad);
        iconRect.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
        if (skeleton)
        {
            //skeleton.AnimationState.AddAnimation(0, anims[2], false, 0);
            //skeleton.AnimationState.AddAnimation(0, anims[3], true, 0);
        }
    }
    private void OnSelectTab()
    {
        if (isLocked) return;
        //AudioManager.Instance.PlayOneShot("SFX_UI_Button_Click_Select_1", 0.7f);
        tabSystem.ChangeTab(index);
    }
}
