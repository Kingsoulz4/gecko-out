using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeIngameBooster : BoosterBase
{
    [SerializeField] private float maxTime = 15;

    protected override int CurrentCount { get => UserDataManager.TimeIngameBooster; set => UserDataManager.TimeIngameBooster = value; }

    private float currentTime;
    private TimeBoosterTopUI timeBoosterTopUI;
    public override void Init()
    {
        base.Init();
        CurrentCount = UserDataManager.TimeIngameBooster;
    }
    protected override void ShowBooster()
    {
        base.ShowBooster();
    }

    private void Update()
    {
        if (!InProgress || GameManager.GameState == GameState.Paused)
        {
            return;
        }
        if (currentTime > 0 && timeBoosterTopUI != null)
        {
            currentTime -= Time.deltaTime;
            float value = currentTime / maxTime;
            timeBoosterTopUI.UpdateFill(value, InGameScreenUI.GetTimeValueToString(currentTime));
        }
        else
        {
            Done();
        }
    }

    public override void ActiveBooster()
    {
        base.ActiveBooster();

        timeBoosterTopUI = UIManager.Instance.GetScreenActive<InGameScreenUI>().TimeBoosterTopUI;
        currentTime = maxTime;
        UserDataManager.TimeIngameBooster = CurrentCount;
        LevelManager.Instance.LevelGame.FreezeTime(currentTime);

        OnStartUseBooster?.Invoke(this, CurrentCount);
    }

    protected override void Done()
    {
        base.Done();
        currentTime = 0;
    }
}
