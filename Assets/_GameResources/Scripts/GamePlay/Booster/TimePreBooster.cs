using DG.Tweening;
using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimePreBooster : BoosterBase
{
    public float bonusTime = 15;
    public float timePlayAnim = 2;

    protected override int CurrentCount { get => UserDataManager.TimeIngameBooster; set => UserDataManager.TimeIngameBooster = value; }

    public bool IsSelectedToUse { get; set; }

    private float currentTime;

    public override void Init()
    {
        base.Init();
        CurrentCount = UserDataManager.TimeIngameBooster;
    }
    protected override void ShowBooster()
    {
        //base.ShowBooster();
    }

    private void Update()
    {
        if (!InProgress || GameManager.GameState == GameState.Paused)
        {
            return;
        }
        
    }

    public override void ActiveBooster()
    {
        if (!IsSelectedToUse) return;

        IsSelectedToUse = true;

        base.ActiveBooster();
        UserDataManager.TimeIngameBooster = CurrentCount;

        OnStartUseBooster?.Invoke(this, CurrentCount);
        InProgress = true;

        DOVirtual.DelayedCall(timePlayAnim, Done);

    }

    protected override void Done()
    {
        base.Done();
        currentTime = 0;

    }
}
