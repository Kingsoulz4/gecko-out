using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupSelectBooster : PopupUI
    {
        [SerializeField] private Button m_buttonPlay;
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Text m_textLevel;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterTime;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterScissor;

        private void Awake()
        {
            m_buttonPlay.onClick.AddListener(OnClickPlay);
            m_buttonClose.onClick.AddListener(OnClickClose);
            
        }

        private void OnEnable()
        {
            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
            m_buttonSelectBoosterTime.Init(5);
            m_buttonSelectBoosterScissor.Init(10);
        }

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickPlay()
        {
            Hide();
            UIManager.Instance.ShowScreen<InGameScreenUI>();
            ((TimePreBooster)BoosterManager.Instance.TimePreBooster).IsSelectedToUse = m_buttonSelectBoosterTime.IsSelected;
            ((ScissorBooster)BoosterManager.Instance.ScissorBooster).IsSelectedToUse = m_buttonSelectBoosterScissor.IsSelected;
            LevelManager.Instance.StartCurrentLevel();

        }
    }
}
