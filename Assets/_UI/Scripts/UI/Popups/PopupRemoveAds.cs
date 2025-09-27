using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupRemoveAds : PopupUI
    {
        [SerializeField] private ShopData m_listRemoveAdsPacks;

        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Button m_buttonPurchaseRemoveInterPack;
        [SerializeField] private Button m_buttonPurchaseRemoveBannerPack;
        [SerializeField] private Button m_buttonPurchaseNoAdsPack;

        [SerializeField] private GameObject m_removeInterPack;
        [SerializeField] private GameObject m_removeBannerPack;
        [SerializeField] private GameObject m_removeAdsPack;

        private void Awake()
        {
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonPurchaseNoAdsPack.onClick.AddListener(OnClickPurchaseNoAdsPack);
            m_buttonPurchaseRemoveBannerPack.onClick.AddListener(OnClickPurchaseRemoveBannerPack);
            m_buttonPurchaseRemoveInterPack.onClick.AddListener(OnClickPurchaseInterPack);
        }

        public void UpdateUI()
        {
            m_removeInterPack.gameObject.SetActive(!ShopManager.Instance.HasPurchasedRemoveInterAds);
            m_removeBannerPack.gameObject.SetActive(!ShopManager.Instance.HasPurchasedRemoveAds);
            m_removeAdsPack.gameObject.SetActive(!ShopManager.Instance.HasPurchasedNoAdsPack);
        }

        private void OnClickPurchaseInterPack()
        {
            var packInter = m_listRemoveAdsPacks.listShopPack.Find(
                x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_INTER_ADS) != null);
            if (packInter == null) return;

            var popupReceiveRewards = UIManager.Instance.ShowPopup<PopupReceiveReward>(null);
            popupReceiveRewards.SetData(packInter.listReward);
            ShopManager.Instance.HasPurchasedRemoveInterAds = true;

            UpdateUI();
        }

        private void OnClickPurchaseRemoveBannerPack()
        {
            var packBanner = m_listRemoveAdsPacks.listShopPack.Find(
                x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_BANNER_ADS) != null);
            if (packBanner == null) return;

            var popupReceiveRewards = UIManager.Instance.ShowPopup<PopupReceiveReward>(null);
            popupReceiveRewards.SetData(packBanner.listReward);
            ShopManager.Instance.HasPurchasedRemoveAds = true;

            UpdateUI();
        }

        private void OnClickPurchaseNoAdsPack()
        {
            var packNoAds = m_listRemoveAdsPacks.listShopPack.Find(
               x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_ADS) != null);
            if (packNoAds == null) return;

            var popupReceiveRewards = UIManager.Instance.ShowPopup<PopupReceiveReward>(null);
            popupReceiveRewards.SetData(packNoAds.listReward);
            ShopManager.Instance.HasPurchasedNoAdsPack = true;

            Hide();
        }

        private void OnClickClose()
        {
            Hide();
        }
    }
}
