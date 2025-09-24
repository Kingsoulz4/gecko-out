using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupLoseHaveSelectBooster : PopupUI
    {
        [SerializeField] private Button m_buttonRetry;
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Text m_textLevel;

        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterTime;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterScissor;

        public Action OnRetry { get; set; }

        public Action OnClose { get; set; }

        private void OnEnable()
        {
            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
            UpdateUI();
        }

        private void Awake()
        {
            m_buttonRetry.onClick.AddListener(OnClickRetry);
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonSelectBoosterTime.OnClick = OnClickSelectBoosterTime;
            m_buttonSelectBoosterScissor.OnClick = OnClickSelectBoosterScissor;
        }

        private void OnClickSelectBoosterScissor()
        {
            if (UserDataManager.CissorBooster <= 0)
            {
                var popupBuyBooster = UIManager.Instance.ShowPopup<PopupBuyBooster>(null);
                popupBuyBooster.Show(BoosterType.CISSOR);
                popupBuyBooster.OnBought = UpdateUI;
            }
        }

        private void OnClickSelectBoosterTime()
        {
            if (UserDataManager.TimePreBooster <= 0)
            {
                var popupBuyBooster = UIManager.Instance.ShowPopup<PopupBuyBooster>(null);
                popupBuyBooster.Show(BoosterType.TIME_PRE);
                popupBuyBooster.OnBought = UpdateUI;
            }
        }

        private void UpdateUI()
        {
            var boosterTimeData = BoosterManager.Instance.BoosterData.GetBoosterItemData(BoosterType.TIME_PRE);
            var boosterScissorData = BoosterManager.Instance.BoosterData.GetBoosterItemData(BoosterType.CISSOR);

            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
            m_buttonSelectBoosterTime.Init(boosterTimeData.levelUnlock, UserDataManager.TimePreBooster);
            m_buttonSelectBoosterScissor.Init(boosterScissorData.levelUnlock, UserDataManager.CissorBooster);

        }

        private void OnClickClose()
        {
            Hide();
            OnClose?.Invoke();
        }

        private void OnClickRetry()
        {
            Hide();
            ((TimePreBooster)BoosterManager.Instance.TimePreBooster).IsSelectedToUse = m_buttonSelectBoosterTime.IsSelected;
            ((ScissorBooster)BoosterManager.Instance.ScissorBooster).IsSelectedToUse = m_buttonSelectBoosterScissor.IsSelected;
            OnRetry?.Invoke();
        }
    }
}
