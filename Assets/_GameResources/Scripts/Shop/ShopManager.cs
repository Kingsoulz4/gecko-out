using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class ShopManager : SingletonMono<ShopManager>
    {
        [SerializeField] private ShopData m_listCoinPacks;
        [SerializeField] private ShopData m_listBundlePack;

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
        }
    }
}
