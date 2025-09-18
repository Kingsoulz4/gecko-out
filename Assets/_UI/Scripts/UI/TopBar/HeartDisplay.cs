using DG.Tweening;
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

        private void OnEnable()
        {
            UserDataManager.OnAddHeart += UpdateQuantity;
        }

        private void OnDisable()
        {
            UserDataManager.OnAddHeart -= UpdateQuantity;
        }

        private void Update()
        {
            m_textCountDown.text = HeartManager.Instance.GetTimeRemaningText();
        }

        private void UpdateQuantity(int oldVal, int newVal, bool hasAnim)
        {
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
