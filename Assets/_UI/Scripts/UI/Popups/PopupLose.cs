using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupLose : PopupUI
    {
        [SerializeField] private Button m_buttonRetry;
        [SerializeField] private Button m_buttonClose;

        public Action OnRetry { get; set; }

        private void Awake()
        {
            m_buttonRetry.onClick.AddListener(OnClickRetry);
            m_buttonClose.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            Hide();
            
        }

        private void OnClickRetry()
        {
            Hide();
            OnRetry?.Invoke();

        }
    }
}
