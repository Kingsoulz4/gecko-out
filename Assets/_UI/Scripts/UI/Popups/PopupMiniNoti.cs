using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupMiniNoti : PopupUI
    {
        [SerializeField] private Text m_textContent;

        Tween tweenShowNoti;

        public void ShowNotiLock()
        {
            m_textContent.text = "Lock!";
            if (tweenShowNoti != null)
            {
                tweenShowNoti.Kill();
                tweenShowNoti = null;
            }
            tweenShowNoti = DOVirtual.DelayedCall(2f, () =>
            {
                Hide();
            });
        }
    }
}
