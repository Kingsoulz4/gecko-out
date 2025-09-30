using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupFullLives : PopupUI
    {
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Button m_buttonGotIt;

        private void Awake()
        {
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonGotIt.onClick.AddListener(OnClickGotIt);

        }

        private void OnClickGotIt()
        {
            Hide();
        }

        private void OnClickClose()
        {
            Hide();
        }
    }
}
