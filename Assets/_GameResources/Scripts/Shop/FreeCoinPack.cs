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
        [SerializeField] private Button m_buttonDisable;
        [SerializeField] private Text m_textCoinQuantity;
        [SerializeField] private Text m_textCoolDown;

        public Action<int> OnGotCoin { get; set; }

        private int coinQuantity = 60;

        private int intervalReceiveFreeCoin = 900;

        private int maxTimeReceiveFreeCoin = 2;

        private double timeCoolDown;

        private DateTime LastTimeReceiveFreeCoin
        {
            get
            {
                return MyUlti.TimeStamp2DateTime(long.Parse(PlayerPrefs.GetString("LastTimeReceiveFreeCoin", "0")));
            }
            set
            {
                PlayerPrefs.SetString("LastTimeReceiveFreeCoin", MyUlti.ToTimestamp(value).ToString());
            }
        }

        private int CurrentTimeReceiveFreeCoin
        {
            get => PlayerPrefs.GetInt("CurrentTimeReceiveFreeCoin", 0);
            set => PlayerPrefs.SetInt("CurrentTimeReceiveFreeCoin", value);
        }    

        private void Awake()
        {
            m_buttonGet.onClick.AddListener(OnClickGet);
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        private void OnClickGet()
        {
            UserDataManager.AddGold(coinQuantity, "free_coin");
            OnGotCoin?.Invoke(coinQuantity);
            LastTimeReceiveFreeCoin = DateTime.Now;
            CurrentTimeReceiveFreeCoin++;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (LastTimeReceiveFreeCoin.Date != DateTime.Now.Date && CurrentTimeReceiveFreeCoin >= maxTimeReceiveFreeCoin)
            {
                CurrentTimeReceiveFreeCoin = 0;
            }

            if (CurrentTimeReceiveFreeCoin < maxTimeReceiveFreeCoin)
            {
                timeCoolDown = (LastTimeReceiveFreeCoin.AddSeconds(intervalReceiveFreeCoin) - DateTime.Now).TotalSeconds;
            }
            else
            {
                timeCoolDown = (DateTime.Now.Date.AddDays(1) - LastTimeReceiveFreeCoin).TotalSeconds;
            }
            m_buttonGet.gameObject.SetActive(timeCoolDown <= 0 && CurrentTimeReceiveFreeCoin < maxTimeReceiveFreeCoin);
            m_buttonDisable.gameObject.SetActive(!m_buttonGet.gameObject.activeInHierarchy);
            StartCoolDownTime();
        }

        private void StartCoolDownTime()
        {
            if (timeCoolDown > 0)
            {
                StartCoroutine(IEStartCoolDown());
            }
        }

        private IEnumerator IEStartCoolDown()
        {
            while (timeCoolDown > 0)
            {
                timeCoolDown -= Time.deltaTime;
                m_textCoolDown.text = $"{MyUlti.Int2TimeString((int)TimeSpan.FromSeconds(timeCoolDown).TotalSeconds)}";

                if(LastTimeReceiveFreeCoin.Date != DateTime.Now.Date)
                {
                    UpdateUI();
                }    

                yield return null;
            }

        }
    }
}
