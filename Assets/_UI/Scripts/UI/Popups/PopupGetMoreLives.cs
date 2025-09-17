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

        public Action OnClose { get; set; }

        public Action OnRefilled { get; set; }

        private void Awake()
        {
            m_buttonClaimByAds.onClick.AddListener(OnClickClaimByAds);
            m_buttonClaimByCoin.onClick.AddListener(OnClickClaimByCoin);
            m_buttonClose.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickClaimByCoin()
        {
            Hide();
            UserDataManager.AddHeart(5, "Refill Heart", false);
        }

        private void OnClickClaimByAds()
        {
            Hide();
            UserDataManager.AddHeart(5, "Refill Heart", false);
        }
    }
}
