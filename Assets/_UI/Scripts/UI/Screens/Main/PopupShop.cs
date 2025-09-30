using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupShop : PopupUI
    {
        [Header("Prefabs")]
        [SerializeField] private BundlePack m_bundlePackPrefab;
        [SerializeField] private CoinPack m_coinPackPrefab;

        [Header("Data")]
        [SerializeField] private ShopData m_listBundlePacks;
        [SerializeField] private ShopData m_listCoinPacks;

        [Header("Containers")]
        [SerializeField] private Transform m_listShopCoinPackContainer;
        [SerializeField] private Transform m_listPackContainer;
        [SerializeField] private Transform m_listBundlePackContainer;

        [Header("Others Packs")]
        [SerializeField] private Button m_buttonRemoveAdsPacks;
        [SerializeField] private GameObject m_removeAdsPacks;
        [SerializeField] private FreeCoinPack m_freeCoinPack;

        [Header("UI")]
        [SerializeField] private GoldDisplay m_goldBar;
        [SerializeField] private Button m_buttonClose;

        private void Awake()
        {
            m_buttonRemoveAdsPacks.onClick.AddListener(OnClickRemoveAdPacks);
            m_buttonClose.onClick.AddListener(Hide);

            Init();
        }

        private void OnClickRemoveAdPacks()
        {
            UIManager.Instance.ShowPopup<PopupRemoveAds>(null);
        }

        private void OnEnable()
        {
            UIManager.Instance.OnRefeshBannerAndAds += UpdateUI;
            UpdateUI();
        }

        private void OnDisable()
        {
            UIManager.Instance.OnRefeshBannerAndAds -= UpdateUI;
        }

        private void UpdateUI()
        {
            m_removeAdsPacks.gameObject.SetActive(!ShopManager.Instance.HasPurchasedNoAdsPack);
        }

        public void Init()
        {
            MyUlti.RemoveAllChilds(m_listBundlePackContainer);
            foreach(var pack in m_listBundlePacks.listShopPack)
            {
                if (ShopManager.Instance.ListPurchasedPacks.Contains(pack.id)) continue;

                var newPack = Instantiate(m_bundlePackPrefab, m_listBundlePackContainer);
                newPack.SetData(pack);
                newPack.OnPurchased = () =>
                {
                    var popupReceiveCoin = UIManager.Instance.ShowPopup<PopupReceiveCoin>(null);
                    var goldQuantity = pack.listReward.Find(x => x.type == ItemType.GOLD).quantity;
                    popupReceiveCoin.PlayCoinFX(m_goldBar.transform.position, Vector3.zero, goldQuantity, () =>
                    {
                        m_goldBar.SetText(UserDataManager.Gold);
                    });
                };
            }
            MyUlti.RemoveAllChilds(m_listShopCoinPackContainer);
            foreach(var pack in m_listCoinPacks.listShopPack)
            {
                var newCoinPack = Instantiate(m_coinPackPrefab, m_listShopCoinPackContainer);
                newCoinPack.SetData(pack);
                newCoinPack.OnPurchased = () =>
                {
                    var popupReceiveCoin = UIManager.Instance.ShowPopup<PopupReceiveCoin>(null);
                    var goldQuantity = pack.listReward.Find(x => x.type == ItemType.GOLD).quantity;
                    popupReceiveCoin.PlayCoinFX(m_goldBar.transform.position, Vector3.zero, goldQuantity, () =>
                    {
                        m_goldBar.SetText(UserDataManager.Gold);
                    });
                };
            }
            m_freeCoinPack.OnGotCoin = (val) => {
                var popupReceiveCoin = UIManager.Instance.ShowPopup<PopupReceiveCoin>(null);
                popupReceiveCoin.PlayCoinFX(m_goldBar.transform.position, Vector3.zero, val, () =>
                {
                    m_goldBar.SetText(UserDataManager.Gold);
                });
            };

        }
    }
}
