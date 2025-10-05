using Geckout.Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupGetMoreLives : PopupUI
    {
        [SerializeField] private Button m_buttonClaimByCoin;
        [SerializeField] private Button m_buttonClaimByAds;
        [SerializeField] private Button m_buttonClose;

        [SerializeField] private Text m_textHeartQuantity;
        [SerializeField] private Text m_textCountDown;
        [SerializeField] private Text m_textPriceCoinRefill;

        [SerializeField] private HeartDisplay m_heartBar;

        public Action OnClose { get; set; }

        public Action OnRefilled { get; set; }

        private void Awake()
        {
            m_buttonClaimByAds.onClick.AddListener(OnClickClaimByAds);
            m_buttonClaimByCoin.onClick.AddListener(OnClickClaimByCoin);
            m_buttonClose.onClick.AddListener(OnClickClose);
        }

        private void OnEnable()
        {
            m_textHeartQuantity.text = UserDataManager.Heart.ToString();
            m_textPriceCoinRefill.text = GameManager.Instance.CoinPriceRefillHeart.ToString();
        }

        private void Update()
        {
            m_textCountDown.text = HeartManager.Instance.GetTimeRemaningText();
        }

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickClaimByCoin()
        {
            //Hide();
            if (UserDataManager.Gold >= GameManager.Instance.CoinPriceRefillHeart)
            {
                UserDataManager.AddHeart(5, "Refill Heart", false);
                UIManager.Instance.ShowPopup<PopupReceiveHeart>(() => {
                    Hide();
                }).PlayCollectFx(m_heartBar.transform.position, Vector3.zero, 5);
                OnRefilled?.Invoke();
            }
            else
            {
                OnClose?.Invoke();
                UIManager.Instance.ShowPopup<PopupShop>(null);
            }
        }

        private void OnClickClaimByAds()
        {
            UserDataManager.AddHeart(1, "Refill Heart", false);
            UIManager.Instance.ShowPopup<PopupReceiveHeart>(() =>
            {
                Hide();
            }).PlayCollectFx(m_heartBar.transform.position, Vector3.zero, 1);
        }
    }
}
