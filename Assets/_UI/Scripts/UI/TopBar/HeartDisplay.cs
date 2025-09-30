using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class HeartDisplay : MonoBehaviour
    {
        [SerializeField] private Text m_textQuantity;
        [SerializeField] private Text m_textCountDown;
        [SerializeField] private Button m_button;
        [SerializeField] private GameObject m_normalHeart;
        [SerializeField] private GameObject m_infinityHeart;

        private void Awake()
        {
            m_button.onClick.AddListener(OnClickHeartBar);
        }

        private void OnClickHeartBar()
        {
            if(UserDataManager.Heart < HeartManager.MAX_HEART)
            {
                UIManager.Instance.ShowPopup<PopupGetMoreLives>(null);
            }
            else
            {
                UIManager.Instance.ShowPopup<PopupFullLives>(null);
            }
        }

        private void OnEnable()
        {
            UserDataManager.OnAddHeart += UpdateQuantity;
            UpdateUI();
        }

        private void OnDisable()
        {
            UserDataManager.OnAddHeart -= UpdateQuantity;
        }

        private void Update()
        {
            m_textCountDown.text = HeartManager.Instance.GetTimeRemaningText();
        }

        private void UpdateUI()
        {
            m_textQuantity.text = UserDataManager.Heart.ToString();
            m_normalHeart.gameObject.SetActive(!HeartManager.IsInfinityEndTime);
            m_infinityHeart.gameObject.SetActive(HeartManager.IsInfinityEndTime);

        }

        private void UpdateQuantity(int oldVal, int newVal, bool hasAnim)
        {
            UpdateUI();
            if(hasAnim)
            {
                DOTween.To(() => oldVal, (val) => { m_textQuantity.text = (int)val + ""; }, newVal, 0.5f);
            }
            else
            {
                m_textQuantity.text = UserDataManager.Heart.ToString();
            }
        }
    }
}
