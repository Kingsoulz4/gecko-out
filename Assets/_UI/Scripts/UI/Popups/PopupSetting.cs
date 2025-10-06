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
    [SerializeField] private GameObject m_iconMinusHeart;

    private void Awake()
    {
        m_buttonExitGame.onClick.AddListener(OnClickExitGame);
        m_buttonRemoveAds.onClick.AddListener(OnClickRemoveAds);
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(LevelManager.Instance.LevelGame != null)
        {
            m_iconMinusHeart.gameObject.SetActive(LevelManager.Instance.LevelGame.IsFirstClick);
        }
        btn_Restore.gameObject.SetActive(ShopManager.Instance.ListPurchasedPacks.Count > 0);
    }

    private void OnClickRemoveAds()
    {
        Hide();
        UIManager.Instance.ShowPopup<PopupRemoveAds>(null);
    }

    private void OnClickExitGame()
    {
        
        if (LevelManager.Instance.LevelGame.IsFirstClick)
        {
            var popupConfirmLeave = UIManager.Instance.ShowPopup<PopupConfirmLeave>(null);
            popupConfirmLeave.OnConfirm = () =>
            {
                Hide();
                HeartManager.UseHeart(1);
                UIManager.Instance.ShowScreen<MainScreenUI>();
            };
        }
        else
        {
            Hide();
            UIManager.Instance.ShowScreen<MainScreenUI>();
        }    
    }

    public void SetType(PopupSettingType type)
    {
        m_buttonExitGame.gameObject.SetActive(type == PopupSettingType.IN_GAME);
        m_buttonRemoveAds.gameObject.SetActive(type == PopupSettingType.HOME && !ShopManager.Instance.HasPurchasedNoAdsPack);
        UpdateUI();
    }

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btn_Close.onClick.AddListener(OnCloseClick);
        btn_Restore.onClick.AddListener(Resrote);
    }

    private void OnCloseClick()
    {
        if(GameManager.GameState == GameState.Paused)
        {
            GameManager.Instance.SetGameState(GameState.Playing);
        }
        Hide();
    }

    public void Resrote()
    {
        UIManager.Instance.CheckRestore();
        Hide();
    }
}
