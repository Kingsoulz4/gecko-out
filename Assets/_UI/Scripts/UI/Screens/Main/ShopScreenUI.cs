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
        [SerializeField] private ShopData m_listBundlePack;
        [SerializeField] private ShopData m_listCoinPacks;

        [Header("Containers")]
        [SerializeField] private Transform m_listShopCoinPackContainer;
        [SerializeField] private Transform m_listPackContainer;

        public void Show()
        {

        }    
    }
}
