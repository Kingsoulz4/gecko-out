using DG.Tweening;
using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class InGameScreenUI : ScreenUI
{
    [Space, Header("UI")]
    [SerializeField] Text txt_Time;
    [SerializeField] Text txt_Level;

    [SerializeField] InputField inputField;
    [SerializeField] Button btn_Load;
    [SerializeField] Button btn_pause;

    [SerializeField] Button btn_Replay;
    [SerializeField] Button btn_newLevel;

    [Space, Header("Booster")]
    BoosterDataSO boosterDataSO;
    [SerializeField] BoosterConfirmUI boosterConfirmUI;

    [Header("Booster FreeTime")]
    [SerializeField] VisualCountBooster timeBoosterCountTxt;
    [SerializeField] Button timeBoosterBtn;
    [SerializeField] Image img_BG_Time;
    [SerializeField] Image img_BG_Booster_FreeTime;

    [Header("Booster remove one tile")]
    [SerializeField] Button handMoveBoosterBtn;
    [SerializeField] VisualCountBooster handMoveBoosterCountTxt;

    [Header("Booster remove 1 tail")]
    [SerializeField] Button cissorBoosterBtn;
    [SerializeField] VisualCountBooster cissorBoosterCountTxt;

    [Header("Booster remove 1 tail")]
    [SerializeField] Button suffleBoosterBtn;
    [SerializeField] VisualCountBooster suffleBoosterCountTxt;


    [Space, Header("Top")]

    [SerializeField] RectTransform rect_Top;
    private float currentTopY;
    internal GameObject TimeBooster;

    public override void Initialize(UIManager uiManager)
    {
        base.Initialize(uiManager);

        btn_Load.onClick.AddListener(LoadLevel);
        timeBoosterBtn.onClick.AddListener(OnTimeBoosterClick);
        handMoveBoosterBtn.onClick.AddListener(HammerBoosterClick);
        cissorBoosterBtn.onClick.AddListener(CissorBoosterClick);
        suffleBoosterBtn.onClick.AddListener(SuffleBoosterClick);
        btn_pause.onClick.AddListener(OnPauseClick);
        btn_Replay.onClick.AddListener(OnReplayClick);

        foreach (var booster in BoosterManager.Instance.Boosters)
        {
            booster.OnStartUseBooster += OnStartUseBooster;
            booster.OnChangeBoosterCount += OnChangeBoosterCount;
            booster.OnUseBoosterDone += OnUseBoosterDone;
        }

        boosterDataSO = BoosterManager.Instance.BoosterData;

        timeBoosterCountTxt.Init(UserDataManager.TimeIngameBooster, BoosterType.TIME_INGAME);
        handMoveBoosterCountTxt.Init(UserDataManager.HammerBooster, BoosterType.HAND_MOVE);
        cissorBoosterCountTxt.Init(UserDataManager.CissorBooster, BoosterType.CISSOR);
        suffleBoosterCountTxt.Init(UserDataManager.SuffleBooster, BoosterType.SUFFLE);

        btn_newLevel.onClick.AddListener(() =>
        {
            UserDataManager.AddHeart(1, "test", false);
            LevelManager.Instance.StartLevel(UserDataManager.Level += 1);
        });
    }

    private void SuffleBoosterClick()
    {
        throw new NotImplementedException();
    }

    private void OnChangeBoosterCount(BoosterBase booster, int currentCount)
    {
        switch (booster.BoosterType)
        {
            case BoosterType.TIME_INGAME:
                timeBoosterCountTxt.UpdateTextCountBooster(currentCount);
                break;
            case BoosterType.HAMMER:
                handMoveBoosterCountTxt.UpdateTextCountBooster(currentCount);
                break;
            case BoosterType.CISSOR:
                cissorBoosterCountTxt.UpdateTextCountBooster(currentCount);
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
            case BoosterType.SUFFLE:
                break;
            case BoosterType.CISSOR:
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            case BoosterType.HAND_MOVE:
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        if (BoosterManager.Instance == null) return;
        foreach (var booster in BoosterManager.Instance.Boosters)
        {
            booster.OnStartUseBooster -= OnStartUseBooster;
            booster.OnUseBoosterDone -= OnUseBoosterDone;
        }
    }

    private void OnStartUseBooster(BoosterBase booster, int currentCount)
    {
        switch (booster.BoosterType)
        {
            case BoosterType.TIME_INGAME:
                timeBoosterCountTxt.UpdateTextCountBooster(currentCount);
                break;
            case BoosterType.HAND_MOVE:
                handMoveBoosterCountTxt.UpdateTextCountBooster(currentCount);
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            case BoosterType.CISSOR:
                cissorBoosterCountTxt.UpdateTextCountBooster(currentCount);
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            case BoosterType.SUFFLE:
                suffleBoosterCountTxt.UpdateTextCountBooster(currentCount);
                boosterConfirmUI.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void OnTimeBoosterClick()
    {
        BoosterManager.Instance.TimeIngameBooster.DoShowBooster((sucess) =>
        {
            //if (sucess)
            //{
            //    boosterPanelUI.ShowChooseFreeTime(img_BG_Time, img_BG_Booster_FreeTime, () =>
            //    {
            //        StopTimeGame(true);
            //        BoosterManager.Instance.TimeBooster.ActiveBooster();
            //    }, () =>
            //    {
            //        StopTimeGame(false);
            //    });
            //}
        });
    }

    private void HammerBoosterClick()
    {
        BoosterManager.Instance.TimePreBooster.DoShowBooster((sucess) =>
        {
            if (sucess)
            {
                boosterConfirmUI.gameObject.SetActive(true);
                var boosterData = boosterDataSO.GetBoosterItemData(BoosterType.HAMMER);
                Image img_BG_ClearOneBooster = handMoveBoosterBtn.GetComponent<Image>();
                boosterConfirmUI.SetUIData(boosterData, img_BG_ClearOneBooster);
            }
        });
    }

    private void CissorBoosterClick()
    {
        BoosterManager.Instance.HammerBooster.DoShowBooster((sucess) =>
        {
            if (sucess)
            {
                boosterConfirmUI.gameObject.SetActive(true);
                var boosterData = boosterDataSO.GetBoosterItemData(BoosterType.CISSOR);
                Image img_BG_ClearSameBooster = cissorBoosterBtn.GetComponent<Image>();
                boosterConfirmUI.SetUIData(boosterData, img_BG_ClearSameBooster);
            }
        });
    }

    private void OnPauseClick()
    {
        MoveUp();
        //GameManager.Instance.PauseGame(false);
        //PopupPause popupPause = UIManager.Instance.ShowPopup<PopupPause>(null);
    }

    private void OnReplayClick()
    {
        MoveUp();
        //GameManager.Instance.PauseGame(false);
        //PopupWarningOutLevel popupWarning = UIManager.Instance.ShowPopup<PopupWarningOutLevel>(null);
        //popupWarning.ShowVisual(true, "ingame");
    }

    public override void Active()
    {
        base.Active();
        txt_Level.text = $"{UserDataManager.Level}";
        currentTopY = rect_Top.anchoredPosition.y;
    }

    public void UpdateText(float timeLevel)
    {
        timeLevel = (int)timeLevel;
        txt_Time.text = GetTimeValueToString(timeLevel);
    }

    private void LoadLevel()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }
        UserDataManager.Level = int.Parse(inputField.text);
        LevelManager.Instance.StartCurrentLevel();
    }

    private void StopTimeGame(bool isStop)
    {
        LevelManager.Instance.CanCountTimeLevel = !isStop;
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
}