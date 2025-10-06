using DG.Tweening;
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

        [SerializeField] private Text m_textPriceInterPack;
        [SerializeField] private Text m_textPriceRemoveAdsPack;
        [SerializeField] private Text m_textPriceNoAdsPack;



        private void Awake()
        {
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonPurchaseNoAdsPack.onClick.AddListener(OnClickPurchaseNoAdsPack);
            m_buttonPurchaseRemoveBannerPack.onClick.AddListener(OnClickPurchaseRemoveBannerPack);
            m_buttonPurchaseRemoveInterPack.onClick.AddListener(OnClickPurchaseInterPack);
            UpdateUI();
        }

        public void UpdateUI()
        {
            m_removeInterPack.gameObject.SetActive(!ShopManager.Instance.HasPurchasedRemoveInterAds && !ShopManager.Instance.HasPurchasedRemoveAds);
            m_removeBannerPack.gameObject.SetActive(!ShopManager.Instance.HasPurchasedRemoveAds);
            m_removeAdsPack.gameObject.SetActive(!ShopManager.Instance.HasPurchasedNoAdsPack);

            var packInter = m_listRemoveAdsPacks.listShopPack.Find(
                x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_INTER_ADS) != null);
            var packBanner = m_listRemoveAdsPacks.listShopPack.Find(
                x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_ADS) != null);
            var packNoAds = m_listRemoveAdsPacks.listShopPack.Find(
               x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_ADS) != null && x.listReward.Count > 1);

            m_textPriceInterPack.text = IAPManager.Instance.GetLocalizedPriceString(packInter.id);
            m_textPriceRemoveAdsPack.text = IAPManager.Instance.GetLocalizedPriceString(packBanner.id);
            m_textPriceNoAdsPack.text = IAPManager.Instance.GetLocalizedPriceString(packNoAds.id);
        }

        public override void Show(Action onClose)
        {
            base.Show(onClose);
            StartCoroutine(IEAnimateAppear());
        }


        private IEnumerator IEAnimateAppear()
        {
            m_buttonClose.transform.localScale = Vector3.zero;
            yield return new WaitForSeconds(0.8f);
            m_buttonClose.transform.localScale = Vector3.one * 0.7f;
            m_buttonClose.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }
            

        private void OnClickPurchaseInterPack()
        {
            var packInter = m_listRemoveAdsPacks.listShopPack.Find(
                x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_INTER_ADS) != null);
            if (packInter == null) return;

            IAPManager.Instance.BuyProductID(packInter.id, (success) =>
            {
                if (success)
                {
                    UIManager.Instance.ShowPopup<PopupNoti>(null).ShowSuccess();
                    ShopManager.Instance.HasPurchasedRemoveInterAds = true;
                    UpdateUI();
                }
                else
                {
                    UIManager.Instance.ShowPopup<PopupNoti>(null).ShowPurchaseFail();
                }
            });
            
        }

        private void OnClickPurchaseRemoveBannerPack()
        {
            var packBanner = m_listRemoveAdsPacks.listShopPack.Find(
                x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_ADS) != null);
            if (packBanner == null) return;

            IAPManager.Instance.BuyProductID(packBanner.id, (success) =>
            {
                if (success)
                {
                    UIManager.Instance.ShowPopup<PopupNoti>(null).ShowSuccess();
                    ShopManager.Instance.HasPurchasedRemoveAds = true;
                    UpdateUI();
                }
                else
                {
                    UIManager.Instance.ShowPopup<PopupNoti>(null).ShowPurchaseFail();
                }
            });

        }

        private void OnClickPurchaseNoAdsPack()
        {
            var packNoAds = m_listRemoveAdsPacks.listShopPack.Find(
               x => x.listReward.FirstOrDefault(y => y.type == ItemType.REMOVE_ADS) != null && x.listReward.Count > 1);
            if (packNoAds == null) return;

            IAPManager.Instance.BuyProductID(packNoAds.id, (success) =>
            {
                if (success)
                {
                    var popupReceiveRewards = UIManager.Instance.ShowPopup<PopupReceiveReward>(null);
                    popupReceiveRewards.SetData(packNoAds.listReward);
                    ShopManager.Instance.HasPurchasedNoAdsPack = true;
                    UIManager.Instance.OnRefeshBannerAndAds?.Invoke();
                    Hide();
                }
                else
                {
                    UIManager.Instance.ShowPopup<PopupNoti>(null).ShowPurchaseFail();
                }
            });
        }

        private void OnClickClose()
        {
            Hide();
        }
    }
}
