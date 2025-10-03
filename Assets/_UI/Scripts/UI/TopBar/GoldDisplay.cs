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
        [SerializeField] Image m_iconGold;
        [SerializeField] string where;
        [SerializeField] public bool isUpadate = true;

        public bool Sync
        {
            get => isUpadate;
            set => isUpadate = value;
        }

        public Image ImgCoinIcon => m_iconGold;

        private void Start()
        {
            if (btn_Gold != null)
            {
                btn_Gold.onClick.AddListener(showPopupMiniShop);
            }
        }

        private void OnEnable()
        {
            UserDataManager.OnUpdateGold += UpdateTextGold;
        }

        public void SetText(int quatity)
        {
            txt_Gold.DOKill();
            txt_Gold.text = quatity.ToString();
        }

        private void showPopupMiniShop()
        {
            UIManager.Instance.ShowPopup<PopupShop>(null);
        }

        public void UpdateTextGold(int goldCurrent, int goldUpdate, float timeDelay) 
        {
            if(isUpadate == false)
            {
                return;
            }
            txt_Gold.DOCounter(goldCurrent, goldUpdate, 0.75f, addThousandsSeparator: false).SetDelay(timeDelay).SetId(this).OnComplete(() =>
            {
                txt_Gold.text = UIManager.FormatString(goldUpdate);
            });
        }

        private void OnDisable()
        {
            UserDataManager.OnUpdateGold -= UpdateTextGold;
            DOTween.Kill(this);
        }
    }
}