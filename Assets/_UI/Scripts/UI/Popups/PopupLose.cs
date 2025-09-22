using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupLose : PopupUI
    {
        [SerializeField] private Button m_buttonRetry;
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Text m_textLevel;

        public Action OnRetry { get; set; }

        public Action OnClose { get; set; }

        private void OnEnable()
        {
            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
        }

        private void Awake()
        {
            m_buttonRetry.onClick.AddListener(OnClickRetry);
            m_buttonClose.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            Hide();
            OnClose?.Invoke();
        }

        private void OnClickRetry()
        {
            Hide();
            OnRetry?.Invoke();
        }
    }
}
