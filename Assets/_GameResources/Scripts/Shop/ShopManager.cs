using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class ShopManager : SingletonMono<ShopManager>
    {
        [SerializeField] private ShopData m_listCoinPacks;
        [SerializeField] private ShopData m_listBundlePack;
        [SerializeField] private ShopData m_listRemoveAdsPacks;

        public bool HasPurchasedRemoveInterAds
        {
            get => PlayerPrefs.GetInt("HasPurchasedRemoveInterAds", 0) > 0;
            set => PlayerPrefs.SetInt("HasPurchasedRemoveInterAds", value? 1: 0);
        }

        public bool HasPurchasedRemoveAds
        {
            get => PlayerPrefs.GetInt("HasPurchasedRemoveBannerAds", 0) > 0;
            set => PlayerPrefs.SetInt("HasPurchasedRemoveBannerAds", value ? 1 : 0);
        }

        public bool HasPurchasedNoAdsPack
        {
            get => PlayerPrefs.GetInt("HasPurchasedNoAdsPack", 0) > 0;
            set => PlayerPrefs.SetInt("HasPurchasedNoAdsPack", value ? 1 : 0);
        }

        private void Awake()
        {
            Init();
        }

        void Init()
        {
            foreach(var item in m_listCoinPacks.listShopPack)
            {
                IAPManager.Instance.AddProductConsume(item.id, item.googleID, item.appleID, null);
            }

            foreach (var item in m_listBundlePack.listShopPack)
            {
                IAPManager.Instance.AddProductConsume(item.id, item.googleID, item.appleID, null);
            }

            foreach (var item in m_listRemoveAdsPacks.listShopPack)
            {
                IAPManager.Instance.AddProductConsume(item.id, item.googleID, item.appleID, null);
            }
        }
    }
}
