using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class HandMoveBooster : BoosterBase
    {
        protected override int CurrentCount { get => UserDataManager.HandMoveBooster; set => UserDataManager.HandMoveBooster = value; }

        public override void Init()
        {
            base.Init();
            CurrentCount = UserDataManager.HandMoveBooster;
        }

        public override void CancelBooster()
        {
            base.CancelBooster();
            TouchInputHandler.Instance.CanClick = true;
            IsShowConfirm = false;
        }

        public override void ActiveBooster()
        {
            base.ActiveBooster();
            IsShowConfirm = false;
            OnStartUseBooster?.Invoke(this, CurrentCount);
        }

        protected override void ShowBooster()
        {
            base.ShowBooster();
            TouchInputHandler.Instance.CanClick = false;
            IsShowConfirm = true;
        }

        protected override void Done()
        {
            base.Done();
            TouchInputHandler.Instance.CanClick = true;
        }
    }
}
