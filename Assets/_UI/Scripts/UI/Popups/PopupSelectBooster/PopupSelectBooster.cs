using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupSelectBooster : PopupUI
    {
        [SerializeField] private Button m_buttonPlay;
        [SerializeField] private Button m_buttonClose;
        [SerializeField] private Text m_textLevel;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterTime;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterScissor;
        [SerializeField] private GameObject m_notiLock;

        [Header("Tutorial")]
        [SerializeField] private GameObject m_tutorialObject;
        [SerializeField] private GameObject m_handObject;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterTimeTut;
        [SerializeField] private ButtonSelectBooster m_buttonSelectBoosterScissorTut;
        [SerializeField] private Text m_textDesBooster;

        private void Awake()
        {
            m_buttonPlay.onClick.AddListener(OnClickPlay);
            m_buttonClose.onClick.AddListener(OnClickClose);
            m_buttonSelectBoosterTime.OnClick = OnClickSelectBoosterTime;
            m_buttonSelectBoosterScissor.OnClick = OnClickSelectBoosterScissor;
            m_buttonSelectBoosterScissorTut.OnClick = m_buttonSelectBoosterScissor.OnClickButton;
            m_buttonSelectBoosterTimeTut.OnClick = m_buttonSelectBoosterTime.OnClickButton;
            HideTut();
        }

        private void OnClickSelectBoosterScissor()
        {
            HideTut();
            if(UserDataManager.CissorBooster <= 0)
            {
                var popupBuyBooster = UIManager.Instance.ShowPopup<PopupBuyBooster>(null);
                popupBuyBooster.Show(BoosterType.CISSOR);
                popupBuyBooster.OnBought = UpdateUI;
            }
        }

        private void OnClickSelectBoosterTime()
        {
            HideTut();
            if (UserDataManager.TimePreBooster <= 0)
            {
                var popupBuyBooster = UIManager.Instance.ShowPopup<PopupBuyBooster>(null);
                popupBuyBooster.Show(BoosterType.TIME_PRE);
                popupBuyBooster.OnBought = UpdateUI;
            }
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            var boosterTimeData = BoosterManager.Instance.BoosterData.GetBoosterItemData(BoosterType.TIME_PRE);
            var boosterScissorData = BoosterManager.Instance.BoosterData.GetBoosterItemData(BoosterType.CISSOR);

            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
            m_buttonSelectBoosterTime.Init(boosterTimeData.levelUnlock, UserDataManager.TimePreBooster);
            m_buttonSelectBoosterScissor.Init(boosterScissorData.levelUnlock, UserDataManager.CissorBooster);

            CheckShowTut();
            
        }

        private void HideTut()
        {
            m_tutorialObject.SetActive(false);
            DOTween.Kill(m_handObject.transform);
        }

        private void CheckShowTut()
        {
            var boosterTimeData = BoosterManager.Instance.BoosterData.GetBoosterItemData(BoosterType.TIME_PRE);
            var boosterScissorData = BoosterManager.Instance.BoosterData.GetBoosterItemData(BoosterType.CISSOR);

            m_buttonSelectBoosterTimeTut.gameObject.SetActive(LevelManager.Instance.CurrentLevel == boosterTimeData.levelUnlock);
            m_buttonSelectBoosterScissorTut.gameObject.SetActive(LevelManager.Instance.CurrentLevel == boosterScissorData.levelUnlock);
            m_tutorialObject.SetActive(LevelManager.Instance.CurrentLevel == boosterTimeData.levelUnlock || LevelManager.Instance.CurrentLevel == boosterScissorData.levelUnlock);

            m_buttonSelectBoosterTimeTut.Init(boosterTimeData.levelUnlock, UserDataManager.TimePreBooster);
            m_buttonSelectBoosterScissorTut.Init(boosterScissorData.levelUnlock, UserDataManager.CissorBooster);
            if (LevelManager.Instance.CurrentLevel == boosterTimeData.levelUnlock)
            {
                
                m_textDesBooster.text = boosterTimeData.description;
                m_handObject.transform.position = m_buttonSelectBoosterTimeTut.transform.position;
            }
            else if(LevelManager.Instance.CurrentLevel == boosterScissorData.levelUnlock)
            {
                m_textDesBooster.text = boosterScissorData.description;
                m_handObject.transform.position = m_buttonSelectBoosterScissorTut.transform.position;
            }

            m_handObject.transform.DOLocalMoveY(m_handObject.transform.localPosition.y + 10f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
            

        private void OnClickClose()
        {
            Hide();
        }

        private void OnClickPlay()
        {
            Hide();
            ((TimePreBooster)BoosterManager.Instance.TimePreBooster).IsSelectedToUse = m_buttonSelectBoosterTime.IsSelected;
            ((ScissorBooster)BoosterManager.Instance.ScissorBooster).IsSelectedToUse = m_buttonSelectBoosterScissor.IsSelected;
            LevelManager.Instance.StartCurrentLevel();
            UIManager.Instance.ShowScreen<InGameScreenUI>();
        }
    }
}
