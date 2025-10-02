using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupTimeIsUp : PopupUI
    {
        [SerializeField] private Button m_buttonKeepPlaying;
        [SerializeField] private Button m_buttonClose;

        public Action OnKeepPlaying { get; set; }

        public Action OnClose { get; set; }

        private void Awake()
        {
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonKeepPlaying.onClick.AddListener(OnClickKeepPlaying);
        }

        public override void Show(Action onClose)
        {
            base.Show(onClose);
            AudioManager.Instance.PlayOneShot(AudioClipNames.FAIL_GAME.ToString(), 1f);
        }

        private void OnClickKeepPlaying()
        {
            Hide();
            OnKeepPlaying?.Invoke();
        }

        private void OnClickClose()
        {
            Hide();
            OnClose?.Invoke();
        }

        
    }
}
