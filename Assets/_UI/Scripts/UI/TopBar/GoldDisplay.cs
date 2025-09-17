using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class GoldDisplay : MonoBehaviour
    {
        [Space, Header("UI")]
        [SerializeField] Text txt_Gold;
        [SerializeField] Button btn_Gold;
        [SerializeField] ParticleSystem vfxCoin;
        [SerializeField] string where;
        public static bool isUpadate = true;

        private void Start()
        {
            setUpVisual();
        }

        private void OnEnable()
        {
        }

        private void setUpVisual()
        {
            if(btn_Gold != null)
            {
                btn_Gold.onClick.AddListener(showPopupMiniShop);
            }
           
        }

        private void showPopupMiniShop()
        {
        }

        public void UpdateTextGold(int goldCurrent, int goldUpdate, float timeDelay) 
        {
            if(isUpadate == false)
            {
                return;
            }
            txt_Gold.DOCounter(goldCurrent, goldUpdate, 0.75f).SetDelay(timeDelay).SetId(this).OnComplete(() =>
            {
                txt_Gold.text = UIManager.FormatString(goldUpdate);
            });
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
        }
    }
}