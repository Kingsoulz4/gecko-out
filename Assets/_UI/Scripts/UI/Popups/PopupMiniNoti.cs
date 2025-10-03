using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupMiniNoti : PopupUI
    {
        [SerializeField] private Text m_textContent;
        [SerializeField] private GameObject m_mainContent;
        [SerializeField] private Transform m_spawnPoint;
        [SerializeField] private Transform m_endPoint;

        Tween tweenShowNoti;

        public override void Show(Action onClose)
        {
            base.Show(onClose);
            StartCoroutine(IEAnimateAppear());
        }

        private IEnumerator IEAnimateAppear()
        {
            m_mainContent.transform.position = m_spawnPoint.position;
            m_mainContent.transform.DOMove(m_endPoint.position, 0.5f);
            yield return new WaitForSeconds(0.5f);
            Hide();
        }

        public void ShowNotiLock()
        {
            m_textContent.text = "Lock!";
        }

        public void Show(string message)
        {
            m_textContent.text = message;
        }
    }
}
