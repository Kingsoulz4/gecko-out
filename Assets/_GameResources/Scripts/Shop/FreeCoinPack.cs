using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class FreeCoinPack : MonoBehaviour
    {
        [SerializeField] private Button m_buttonGet;
        [SerializeField] private Text m_textCoinQuantity;

        public Action<int> OnGotCoin { get; set; }

        private int coinQuantity = 60;

        private void Awake()
        {
            m_buttonGet.onClick.AddListener(OnClickGet);
        }

        private void OnClickGet()
        {
            UserDataManager.AddGold(coinQuantity, "free_coin");
            OnGotCoin?.Invoke(coinQuantity);
        }
    }
}
