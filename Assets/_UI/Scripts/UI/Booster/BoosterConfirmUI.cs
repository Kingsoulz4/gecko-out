using JellyBlockJam;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterConfirmUI : MonoBehaviour
{
    [Space, Header("UI")]
    [SerializeField] ThemeChooseBooster objHammerPanel;
    [SerializeField] ThemeChooseBooster objMagnetPanel;

    [SerializeField] private Text titleTxt;
    [SerializeField] private Text descriptionTxt;
    [SerializeField] private Image iconImg;
    [SerializeField] private Button closeBtn;
    public BoosterType BoosterType = BoosterType.NONE;

    private void Start()
    {
        closeBtn.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick()
    {
        if(BoosterType == BoosterType.HAMMER)
        {
        }
        else if(BoosterType == BoosterType.SISSOR)
        {
        }
        gameObject.SetActive(false);
        var booster = BoosterManager.Instance.GetBoosterByType(BoosterType);
        booster.CancelBooster();
    }

    public void SetUIData(BoosterItemData boosterItemData, Image img_Booster)
    {
        BoosterType = boosterItemData.boosterType;
        var pos = UIManager.Instance.UICamera.WorldToScreenPoint(img_Booster.rectTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(closeBtn.transform.parent.GetComponent<RectTransform>(), pos, UIManager.Instance.UICamera, out Vector2 localPos);
        RectTransform rectTransformButtonCancel = closeBtn.GetComponent<RectTransform>();
        rectTransformButtonCancel.anchoredPosition = localPos;
        if (BoosterType == BoosterType.HAMMER)
        {
            objHammerPanel.gameObject.SetActive(true);
            objMagnetPanel.gameObject.SetActive(false);
            objHammerPanel.SetUpVisual(boosterItemData);
        }
        else
        {
            objHammerPanel.gameObject.SetActive(false);
            objMagnetPanel.gameObject.SetActive(true);
            objMagnetPanel.SetUpVisual(boosterItemData);
        }
        if (titleTxt != null)
        {
            titleTxt.text = boosterItemData.title;
        }

        if (descriptionTxt != null)
        {
            descriptionTxt.text = boosterItemData.description;
        }

        if (iconImg != null)
        {
            iconImg.sprite = boosterItemData.icon;
        }
    }

}
