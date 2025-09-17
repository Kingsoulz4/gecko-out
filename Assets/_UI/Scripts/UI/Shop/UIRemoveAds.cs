using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Geckout;
using UnityEngine;
using UnityEngine.UI;

public class UIRemoveAds : PopupUI
{
    [SerializeField] private int id = -1;

    [SerializeField] private Text iapPriceText;
    [SerializeField] private Text iapPriceNoSaleText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;

    public Action OnBuySS;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        buyButton.onClick.AddListener(OnClickBuy);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Initialized()
    {
        
    }

    private void OnClickBuy()
    {
    }

    private void OnBuySuccess()
    {
        
    }
}
