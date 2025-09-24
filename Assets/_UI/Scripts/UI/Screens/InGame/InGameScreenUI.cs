using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Geckout;
using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameScreenUI : ScreenUI
{
    [Space, Header("UI")]
    [SerializeField] Text txt_Time;
    [SerializeField] Text txt_Level;

    [SerializeField] Button btn_pause;
    [SerializeField] Button btn_Replay;

    [SerializeField] SerializedDictionary<LevelType, GameObject> m_listTimeBarBackground;

    [Space, Header("Booster")]
    BoosterDataSO boosterDataSO;
    [SerializeField] BoosterConfirmUI boosterConfirmUI;

    [Header("Booster FreeTime")]
    [SerializeField] VisualCountBooster timeBoosterCount;
    [SerializeField] Button timeBoosterBtn;
    [SerializeField] Image img_BG_Time;
    [SerializeField] TimeBoosterTopUI timeBoosterTopUI;

    [Header("Booster hand")]
    [SerializeField] Button handMoveBoosterBtn;
    [SerializeField] VisualCountBooster handMoveBoosterCount;

    [Header("Booster hammer")]
    [SerializeField] Button hammerBtn;
    [SerializeField] VisualCountBooster hammerBoosterCount;

    [Header("Booster Time")]
    [SerializeField] private GameObject m_iconClock;

    //[Header("Booster remove 1 tail")]
    //[SerializeField] Button suffleBoosterBtn;
    //[SerializeField] VisualCountBooster suffleBoosterCount;


    [Space, Header("Top")]

    [SerializeField] RectTransform rect_Top;
    private float currentTopY;

    public TimeBoosterTopUI TimeBoosterTopUI { get => timeBoosterTopUI;}

    private LevelController LevelController => LevelManager.Instance.LevelGame;

    private void Update()
    {
        if (LevelController.CanCountdownTime)
        {
            UpdateTimeText(LevelController.CurrentTimeLevelRemaining);
        }
    }

    public override void Initialize(UIManager uiManager)
    {
        base.Initialize(uiManager);

        timeBoosterBtn.onClick.AddListener(OnTimeBoosterClick);
        handMoveBoosterBtn.onClick.AddListener(HandMoveboosterClick);
        hammerBtn.onClick.AddListener(HammerBoosterClick);
        //suffleBoosterBtn.onClick.AddListener(SuffleBoosterClick);
        btn_pause.onClick.AddListener(OnPauseClick);
        btn_Replay.onClick.AddListener(OnReplayClick);

        foreach (var booster in BoosterManager.Instance.Boosters)
        {
            booster.OnStartUseBooster += OnStartUseBooster;
            booster.OnChangeBoosterCount += OnChangeBoosterCount;
            booster.OnUseBoosterDone += OnUseBoosterDone;
        }

        boosterDataSO = BoosterManager.Instance.BoosterData;

        timeBoosterCount.Init(UserDataManager.TimeIngameBooster, BoosterType.TIME_INGAME);
        hammerBoosterCount.Init(UserDataManager.HammerBooster, BoosterType.HAMMER);
        handMoveBoosterCount.Init(UserDataManager.HandMoveBooster, BoosterType.HAND_MOVE);
        //suffleBoosterCount.Init(UserDataManager.SuffleBooster, BoosterType.SUFFLE);
    }

    private void OnDisable()
    {
        HideAllTuts();
    }

    private void OnDestroy()
    {
        HideAllTuts();
        if (BoosterManager.Instance == null) return;
        foreach (var booster in BoosterManager.Instance.Boosters)
        {
            booster.OnStartUseBooster -= OnStartUseBooster;
            booster.OnUseBoosterDone -= OnUseBoosterDone;
            booster.OnChangeBoosterCount -= OnChangeBoosterCount;
        }
    }

    private void OnChangeBoosterCount(BoosterBase booster, int currentCount)
    {
        switch (booster.BoosterType)
        {
            case BoosterType.TIME_INGAME:
                timeBoosterCount.UpdateTextCountBooster(currentCount);
                break;
            case BoosterType.HAND_MOVE:
                handMoveBoosterCount.UpdateTextCountBooster(currentCount);
                break;
            case BoosterType.HAMMER:
                hammerBoosterCount.UpdateTextCountBooster(currentCount);
                break;
            default:
                break;
        }
    }

    private void OnUseBoosterDone(BoosterBase booster, int currentCount)
    {
        switch (booster.BoosterType)
        {
            case BoosterType.TIME_INGAME:
                timeBoosterTopUI.SetActive(false);
                break;
            default:
                break;
        }
    }


    private void OnStartUseBooster(BoosterBase booster, int currentCount)
    {
        switch (booster.BoosterType)
        {
            case BoosterType.TIME_INGAME:
                timeBoosterTopUI.SetActive(true);
                timeBoosterCount.UpdateTextCountBooster(currentCount);
                break;
            case BoosterType.HAND_MOVE:
                handMoveBoosterCount.UpdateTextCountBooster(currentCount);
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            case BoosterType.HAMMER:
                hammerBoosterCount.UpdateTextCountBooster(currentCount);
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void HandMoveboosterClick()
    {
        BoosterManager.Instance.HandMoveBooster.DoShowBooster((sucess) =>
        {
            if (sucess)
            {
                boosterConfirmUI.gameObject.SetActive(true);
                var boosterData = boosterDataSO.GetBoosterItemData(BoosterType.HAND_MOVE);
                Image handMoveImg = handMoveBoosterBtn.GetComponent<VisualCountBooster>().Icon;
                boosterConfirmUI.SetUIData(boosterData, handMoveImg);
            }
        });
    }

    private void SuffleBoosterClick()
    {
        
    }

    private void OnTimeBoosterClick()
    {
        BoosterManager.Instance.TimeIngameBooster.DoShowBooster((sucess) =>
        {
            if (sucess)
            {
                BoosterManager.Instance.TimeIngameBooster.ActiveBooster();
            }
        });
    }

    private void HammerBoosterClick()
    {

        BoosterManager.Instance.HammerBooster.DoShowBooster((sucess) =>
        {
            if (sucess)
            {
                boosterConfirmUI.gameObject.SetActive(true);
                var boosterData = boosterDataSO.GetBoosterItemData(BoosterType.HAMMER);
                Image hammerImg = hammerBtn.GetComponent<VisualCountBooster>().Icon;
                boosterConfirmUI.SetUIData(boosterData, hammerImg);
            }
        });
    }

    private void OnPauseClick()
    {
        var popupSetting = UIManager.Instance.ShowPopup<PopupSetting>(() =>
        {
            GameManager.Instance.SetGameState(GameState.Playing);
        });
        popupSetting.SetType(PopupSettingType.IN_GAME);
        GameManager.Instance.SetGameState(GameState.Paused);
    }

    public void OnReplayClick()
    {
        ShowConfirmLeave();
        GameManager.Instance.SetGameState(GameState.Playing);
    }

    public override void Active()
    {
        base.Active();
        currentTopY = rect_Top.anchoredPosition.y;
        UpdateUI();
        PoupNewFeature();
    }

    public void UpdateUI()
    {
        txt_Level.text = $"{LevelManager.Instance.CurrentLevel}";
        UpdateTimeText(LevelController.CurrentTimeLevelRemaining);
        UpdateTimeBar();
    }

    private void UpdateTimeBar()
    {
        foreach(var item in m_listTimeBarBackground)
        {
            item.Value.SetActive(false);
        }
        m_listTimeBarBackground[LevelController.GameLevelData.type].SetActive(true);
    }

    private void PoupNewFeature()
    {
        //return;
        var feature = NewFeatureManager.Instance.GetNewFeatureInProgress();
        if (feature != null && LevelManager.Instance.CurrentLevel == feature.level && UserDataManager.LastFeatureCount < feature.level)
        {
            if (feature.displayType == NewFeatureTutDisplayType.POPOP_VID)
            {
                var pop = UIManager.Instance.ShowPopup<PopupTutorialNewFeature>(null);
                pop.SetData(feature.title, feature.desInTutorial, feature.icon);
                UserDataManager.LastFeatureCount = feature.level;
            }
            else
            {
                var pop = UIManager.Instance.ShowPopup<PopupTutorialTextNewFeature>(null);
                pop.SetData(feature);
            }
        }
    }

    private void HideAllTuts()
    {
        var popupTut = UIManager.Instance.GetPopupActive<PopupTutorialTextNewFeature>();
        if(popupTut != null)
        {
            popupTut.Hide();
        }    
    }
        

    public void UpdateTimeText(float timeLevel)
    {
        timeLevel = (int)timeLevel;
        txt_Time.text = GetTimeValueToString(timeLevel);
    }

  

    public static string GetTimeValueToString(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        if (seconds <= 0 && minutes <= 0)
        {
            return "Finish";
        }
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    public void MoveUp()
    {
        rect_Top.DOAnchorPosY(currentTopY + 1000, 0.5f).SetId(this);
    }

    public void MoveDown()
    {
        rect_Top.DOAnchorPosY(currentTopY, 0.5f).SetId(this);
    }

    private void ShowConfirmLeave()
    {
        var popupConfirmLeave = UIManager.Instance.ShowPopup<PopupConfirmLeave>(null);
        popupConfirmLeave.SetTextButtonConfirm("Retry");
        popupConfirmLeave.OnConfirm = () =>
        {
            HeartManager.UseHeart(1);
            LevelManager.Instance.OnRetryGame();
        };
        popupConfirmLeave.OnClose = () =>
        {

        };
    }
    #region Booster Add Time

    public void ShowAddTimeAnim(int valAdd)
    {
        StartCoroutine(IEAnimateAddTime(valAdd));
    }
        
    private IEnumerator IEAnimateAddTime(int valAdd)
    {
        yield return null;
        m_iconClock.transform.DOScale(1.1f, 0.25f);
        txt_Time.transform.DOScale(1.1f, 0.25f);
        yield return new WaitForSeconds(0.3f);
        var currentVal = LevelController.CurrentTimeLevelRemaining - valAdd;
        DOTween.To(() => currentVal, (val) =>
        {
            txt_Time.text = GetTimeValueToString(val);
        }, LevelController.CurrentTimeLevelRemaining, 0.5f);

        yield return new WaitForSeconds(0.5f);
        m_iconClock.transform.DOScale(1f, 0.25f);
        txt_Time.transform.DOScale(1f, 0.25f);
    }

    public Vector3 GetClockIconPosition()
    {
        return m_iconClock.transform.position;
    }
    #endregion

}