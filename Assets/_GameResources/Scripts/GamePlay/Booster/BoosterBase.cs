using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoosterType
{
    NONE = -1,
    TIME_INGAME = 0,
    TIME_PRE = 1,
    HAMMER = 2,
    SISSOR = 3,
    HAND_MOVE = 4,
    SUFFLE = 5,
}
public abstract class BoosterBase : MonoBehaviour
{
    [SerializeField] protected int maxCount;
    [SerializeField] BoosterType boosterType;
    private int currentCount;
    private bool canActive => !InProgress;
    private bool inProgress;
    private bool isShowConfirm = false;

    public Action<BoosterBase, int> OnStartUseBooster;
    public Action<BoosterBase, int> OnChangeBoosterCount;
    public Action<BoosterBase, int> OnUseBoosterDone;

    protected virtual int CurrentCount
    {
        get
        {

            return currentCount;
        }
        set
        {

            currentCount = value;
        }
    }

    public bool InProgress { get => inProgress; set => inProgress = value; }
    public BoosterType BoosterType { get => boosterType; }
    protected bool IsShowConfirm { get => isShowConfirm; set => isShowConfirm = value; }

    protected virtual bool CanShowBooster() => CurrentCount > 0 && canActive;

    public virtual void Init()
    {
        InProgress = false;
    }

    public void DoShowBooster(Action<bool> callback = null)
    {
        if (CanShowBooster())
        {
            ShowBooster();
            callback?.Invoke(true);

        }
        else
        {
            if (inProgress)
            {
                UIManager.Instance.NotifyContent("You can't use it right now!");
                return;
            }
            // Show popup buy
            //PopupBuyBooster poup = UIManager.Instance.GetPopupActive<PopupBuyBooster>();
            //if (poup == null)
            //{
            //    poup = UIManager.Instance.ShowPopup<PopupBuyBooster>(() =>
            //    {
            //        OnUseBoosterDone?.Invoke(this, CurrentCount);
            //    });
            //    poup.action = UpdateCountBooster;
            //    poup.VisualBooster(boosterType);
            //}
            callback?.Invoke(false);
        }
    }

    public void UpdateCountBooster()
    {
        CurrentCount += 1;
        UpdateVisualBooster();
    }

    public void UpdateVisualBooster()
    {
        OnChangeBoosterCount?.Invoke(this, CurrentCount);
    }

    public virtual void ActiveBooster()
    {
        CurrentCount--;
    }

    public virtual void CancelBooster()
    {
        IsShowConfirm = false;
        InProgress = false;

    }

    protected virtual void ShowBooster()
    {
        InProgress = true;
    }

    protected virtual void Done()
    {
        InProgress = false;
        OnUseBoosterDone?.Invoke(this, CurrentCount);
    }
}
