using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupBuyBooster : PopupUI
    {
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Button m_buttonBuy;
        [SerializeField] private Image m_imageBoosterIcon;

        public Action OnClose { get; set; }

        public Action OnBought { get; set; }

        private BoosterType boosterType;

        private void Awake()
        {
            m_buttonBuy.onClick.AddListener(OnClickBuy);
            m_buttonClose.onClick.AddListener(OnClickClose);

        }

        public void Show(BoosterType boosterType)
        {
            this.boosterType = boosterType;
            var iconSprite = Resources.Load<Sprite>($"BoosterIcons/{(int)boosterType}");
            m_imageBoosterIcon.sprite = iconSprite;
        }

        private void OnClickClose()
        {
            Hide();
            OnClose?.Invoke();
        }

        private void OnClickBuy()
        {
            Hide();
            UserDataManager.AddBooster(boosterType, 3);
            OnBought?.Invoke();

            if (UserDataManager.Gold >= 900)
            {
                
            }
            else
            {

            }
        }
    }
}
