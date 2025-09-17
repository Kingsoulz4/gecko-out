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

        private void Update()
        {
            m_textQuantity.text = UserDataManager.Heart.ToString();
            //m_textCountDown.text = HeartManager.
        }
    }
}
