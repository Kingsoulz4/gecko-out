using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class CoinPack : MonoBehaviour
    {
        [SerializeField] private Image m_iconCoin;
        [SerializeField] private Button m_buttonBuy;
        [SerializeField] private Text m_textPrice;
        [SerializeField] private Text m_textCoinQuantity;

        private ShopPack shopPack;

        public Action OnPurchased { get; set; }

        private void Awake()
        {
            m_buttonBuy.onClick.AddListener(OnClickBuy);
        }

        private void OnClickBuy()
        {
            //Add Logic IAP Here
            var popupReceiveReward = UIManager.Instance.ShowPopup<PopupReceiveReward>(() =>
            {
                OnPurchased?.Invoke();
            });
            popupReceiveReward.SetData(shopPack.listReward);

            foreach (var item in shopPack.listReward)
            {
                item.Claim();
            }
        }

        public void SetData(ShopPack packData)
        {
            this.shopPack = packData;
            m_textCoinQuantity.text = packData.listReward.Find(x => x.type == ItemType.GOLD).quantity + "";
            m_iconCoin.sprite = packData.icon;


        }
    }
}
