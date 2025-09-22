using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterConfirmUI : MonoBehaviour
{
    [Space, Header("UI")]

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
        gameObject.SetActive(false);
        var booster = BoosterManager.Instance.GetBoosterByType(BoosterType);
        booster.CancelBooster();
    }


    public void SetUIData(BoosterItemData boosterItemData, Image img_Booster)
    {
        BoosterType = boosterItemData.boosterType;
        iconImg.sprite = boosterItemData.icon;
        descriptionTxt.text = boosterItemData.description;

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
