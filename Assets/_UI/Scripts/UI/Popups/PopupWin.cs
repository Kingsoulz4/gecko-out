using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupWin : PopupUI
    {
        [SerializeField] private Button m_buttonClaim;
        [SerializeField] private Button m_buttonClaimX2;

        private void Awake()
        {
            m_buttonClaim.onClick.AddListener(OnClickClaim);
            m_buttonClaimX2.onClick.AddListener(OnClickClaimX2);
        }

        private void OnClickClaimX2()
        {
            Hide();
        }

        private void OnClickClaim()
        {
            Hide();
        }
    }
}
