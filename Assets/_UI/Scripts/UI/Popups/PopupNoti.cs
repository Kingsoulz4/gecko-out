using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    

    public class PopupNoti : PopupUI
    {
        public enum NotiType
        {
            Error,
            Warning,
            Success
        }

        [SerializeField] private Text m_textContent;
        [SerializeField] private SerializedDictionary<NotiType, Sprite> m_listSpriteNotiType;
        [SerializeField] private Image m_image;

        public override void Show(Action onClose)
        {
            base.Show(onClose);
            DOVirtual.DelayedCall(1f, () =>
            {
                Hide();
            });
        }

        public void ShowLocked()
        {
            m_image.sprite = m_listSpriteNotiType[NotiType.Warning];
            m_textContent.text = "Locked";
        }

        public void ShowProcessing()
        {
            m_image.sprite = m_listSpriteNotiType[NotiType.Warning];
            m_textContent.text = "Processing...";
        }

        public void ShowPurchaseFail()
        {
            m_image.sprite = m_listSpriteNotiType[NotiType.Error];
            m_textContent.text = "Purchase Failed!";
        }

        public void ShowSuccess()
        {
            m_image.sprite = m_listSpriteNotiType[NotiType.Success];
            m_textContent.text = "Success!";
        }
    }
}
