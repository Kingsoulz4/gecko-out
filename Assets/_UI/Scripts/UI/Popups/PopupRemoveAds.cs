using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupRemoveAds : PopupUI
    {
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Button m_buttonPurchaseRemoveInterPack;
        [SerializeField] private Button m_buttonPurchaseRemoveBannerPack;
        [SerializeField] private Button m_buttonPurchaseNoAdsPack;

        private void Awake()
        {
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonPurchaseNoAdsPack.onClick.AddListener(OnClickPurchaseNoAdsPack);
            m_buttonPurchaseRemoveBannerPack.onClick.AddListener(OnClickPurchaseRemoveBannerPack);
            m_buttonPurchaseRemoveInterPack.onClick.AddListener(OnClickPurchaseInterPack);
        }

        private void OnClickPurchaseInterPack()
        {
            Hide();
        }

        private void OnClickPurchaseRemoveBannerPack()
        {
            Hide();
        }

        private void OnClickPurchaseNoAdsPack()
        {
            Hide();
        }

        private void OnClickClose()
        {
            Hide();
        }
    }
}
