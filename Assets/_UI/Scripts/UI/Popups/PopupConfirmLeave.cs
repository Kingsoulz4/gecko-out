using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupConfirmLeave : PopupUI
    {
        [SerializeField] private Button m_buttonConfirm;
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Text m_textButtonConfirm;

        public Action OnConfirm { get; set; }
        public Action OnClose { get; set; }

        private void Awake()
        {
            m_buttonConfirm.onClick.AddListener(OnClickConfirm);
            m_buttonClose.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            Hide();
            OnClose?.Invoke();
        }

        private void OnClickConfirm()
        {
            Hide();
            OnConfirm?.Invoke();
        }

        public void SetTextButtonConfirm(string text)
        {
            m_textButtonConfirm.text = text;
        }
    }
}
