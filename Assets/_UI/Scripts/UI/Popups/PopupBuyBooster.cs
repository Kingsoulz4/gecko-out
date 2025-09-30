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
        [SerializeField] private Text m_textPrice;
        [SerializeField] private Text m_textTitle;
        [SerializeField] private Text m_textDes;

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
            var boosterData = BoosterManager.Instance.BoosterData.GetBoosterItemData(boosterType);
            var iconSprite = Resources.Load<Sprite>($"BoosterIcons/{(int)boosterType}");
            m_imageBoosterIcon.sprite = iconSprite;
            m_textPrice.text = boosterData.price + "";
            m_textTitle.text = boosterData.title;
        }

        private void OnClickClose()
        {
            Hide();
            OnClose?.Invoke();
        }

        private void OnClickBuy()
        {
            Hide();
            
            if (UserDataManager.Gold >= BoosterManager.Instance.BoosterData.GetBoosterItemData(boosterType).price)
            {
                UserDataManager.AddBooster(boosterType, 3);
                OnBought?.Invoke();
            }
            else
            {
                OnClose?.Invoke();
                UIManager.Instance.ShowPopup<PopupShop>(() =>
                {
                    GameManager.Instance.SetGameState(GameState.Playing);
                });
                GameManager.Instance.SetGameState(GameState.Paused);
            }
        }
    }
}
