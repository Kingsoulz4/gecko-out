using DG.Tweening;
using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupNewFeature : PopupUI
{
    public NewFeatureItemData popupData;
    public Image iconImage;
    public Text titleText;
    public Text usingText;
    public Button okBtn;

    public void SetData()
    {
        popupData = NewFeatureManager.Instance.NewFeatureSO.GetNewFeatureItemData(LevelManager.Instance.CurrentLevel);
        if (popupData != null)
        {
            if (iconImage != null) iconImage.sprite = popupData.icon;
            if (titleText != null) titleText.text = popupData.title;
            if (usingText != null) usingText.text = popupData.des;
            iconImage.SetNativeSize();
        }
        else
        {
            Debug.LogWarning("PopupSimpleData is not assigned!");
        }
    }


    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        okBtn.onClick.AddListener(OnOkClick);
    }

    public override void Show(Action onClose)
    {
        SetData();
        base.Show(onClose);
    }

    private void OnOkClick()
    {
        Hide();
    }



}
