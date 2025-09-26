using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class ShopScreenUI : MonoBehaviour
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

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            MyUlti.RemoveAllChilds(m_listBundlePackContainer);
            foreach(var pack in m_listBundlePacks.listShopPack)
            {
                var newPack = Instantiate(m_bundlePackPrefab, m_listBundlePackContainer);
                newPack.SetData(pack);    
            }
            MyUlti.RemoveAllChilds(m_listShopCoinPackContainer);
            foreach(var pack in m_listCoinPacks.listShopPack)
            {
                var newCoinPack = Instantiate(m_coinPackPrefab, m_listShopCoinPackContainer);
                newCoinPack.SetData(pack);
            }
        }
    }
}
