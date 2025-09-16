using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JellyBlockJam
{
    public class VisualCountBooster : MonoBehaviour
    {
        [Space, Header("UI")]
        [SerializeField] Text txt_CountBooster;
        [SerializeField] GameObject obj_BoosterActive;
        [SerializeField] GameObject obj_BoosterAdd;
        [SerializeField] Button btn_AddBooster;
        private BoosterType boosterType;

        public void Init(int count, BoosterType boosterType)
        {
            this.boosterType = boosterType;
            if (obj_BoosterActive == null || obj_BoosterAdd == null) { return; }
            btn_AddBooster.onClick.AddListener(ShowPopupAddBooster);
            bool isActive = count > 0;
            obj_BoosterActive.SetActive(isActive);
            obj_BoosterAdd.SetActive(!isActive);
            UpdateText(count);
        }

        private void ShowPopupAddBooster()
        {
            //PopupBuyBooster poup = UIManager.Instance.GetPopupActive<PopupBuyBooster>();
            //if (poup == null)
            //{
            //    poup = UIManager.Instance.ShowPopup<PopupBuyBooster>(null);                
            //    poup.VisualBooster(boosterType);
            //}
        }

        public void UpdateTextCountBooster(int count)
        {
            bool isActive = count > 0;
            if(obj_BoosterActive == null || obj_BoosterAdd == null) { return; }
            obj_BoosterActive.SetActive(isActive);
            obj_BoosterAdd.SetActive(!isActive);
            UpdateText(count);
        }

        private void UpdateText(int count)
        {
            string value = $"{count}";
            if (count > 99)
            {
                value = $"99+";
            }
            txt_CountBooster.text = value;
        }
    }
}