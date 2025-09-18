using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PopupSettingType
{
    IN_GAME,
    HOME
}

public class PopupSetting : PopupUI
{
    [SerializeField] Button btn_Close;
    [SerializeField] Button btn_Restore;
    [SerializeField] Button m_buttonExitGame;
    [SerializeField] Button m_buttonRemoveAds;

    private void Awake()
    {
        m_buttonExitGame.onClick.AddListener(OnClickExitGame);
    }

    private void OnClickExitGame()
    {
        Hide();
        var popupConfirmLeave = UIManager.Instance.ShowPopup<PopupConfirmLeave>(null);
        popupConfirmLeave.OnConfirm = () =>
        {
            UIManager.Instance.ShowScreen<MainScreenUI>();
        };
    }

    public void SetType(PopupSettingType type)
    {
        m_buttonExitGame.gameObject.SetActive(type == PopupSettingType.IN_GAME);
        m_buttonRemoveAds.gameObject.SetActive(type == PopupSettingType.HOME);
    }

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btn_Close.onClick.AddListener(Hide);
        btn_Restore.onClick.AddListener(Resrote);
    }

    public void Resrote()
    {
        UIManager.Instance.CheckRestore();
        Hide();
    }
}
