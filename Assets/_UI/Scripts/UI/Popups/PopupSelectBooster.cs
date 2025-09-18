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

        private void Awake()
        {
            m_buttonPlay.onClick.AddListener(OnClickPlay);
            m_buttonClose.onClick.AddListener(OnClickClose);
        }

        private void OnEnable()
        {
            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
        }

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickPlay()
        {
            Hide();
            UIManager.Instance.ShowScreen<InGameScreenUI>();
            LevelManager.Instance.StartCurrentLevel();
        }
    }
}
