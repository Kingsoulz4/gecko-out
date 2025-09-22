using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class ButtonSettingMain : MonoBehaviour
    {
        [SerializeField] Button btn_Setting;
        private void Awake()
        {
            btn_Setting.onClick.AddListener(ShowPopupSetting);
        }

        private void ShowPopupSetting()
        {
            PopupSetting popupSetting = UIManager.Instance.ShowPopup<PopupSetting>(null);
            popupSetting.SetType(PopupSettingType.HOME);
        }
    }
}