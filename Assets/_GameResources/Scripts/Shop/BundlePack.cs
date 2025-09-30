using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class BundlePack : MonoBehaviour
    {
        [SerializeField] private ItemRewardUI m_itemRewardPrefab;
        [SerializeField] private Image m_iconCoin;
        [SerializeField] private Transform m_listRewardContainer;
        [SerializeField] private Button m_buttonBuy;
        [SerializeField] private Text m_textPrice;
        [SerializeField] private Text m_textCoinQuantity;
        [SerializeField] private Text m_textPackName;
        [SerializeField] private GameObject m_tagHighlight;
        [SerializeField] private string packIDHighlight;

        private ShopPack shopPack;
        public Action OnPurchased { get; set; }

        private void Awake()
        {
            m_buttonBuy.onClick.AddListener(OnClickBuy);
        }

        private void OnClickBuy()
        {
            //Add Logic IAP Here

            IAPManager.Instance.BuyProductID(shopPack.id, (success) =>
            {
                if(success)
                {
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
                else
                {
                    UIManager.Instance.ShowPopup<PopupNoti>(null).ShowPurchaseFail();
                }
            });

            
        }

        public void SetData(ShopPack packData)
        {
            this.shopPack = packData;
            m_textCoinQuantity.text = packData.listReward.Find(x => x.type == ItemType.GOLD).quantity + "";
            m_iconCoin.sprite = packData.icon;
            m_textPackName.text = packData.title;
            m_tagHighlight.gameObject.SetActive(packData.id == packIDHighlight);
            MyUlti.RemoveAllChilds(m_listRewardContainer);
            foreach(var item in packData.listReward.Where(x => x.type != ItemType.GOLD))
            {
                var newItem = Instantiate(m_itemRewardPrefab, m_listRewardContainer);
                newItem.gameObject.SetActive(true);
                newItem.SetData(item);
            }

        }
    }
}
