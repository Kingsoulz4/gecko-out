using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class BeginingBoosterManager : MonoBehaviour, IFlowCallback
    {
        private LevelController LevelGame => LevelManager.Instance.LevelGame;

        private void Awake()
        {
            LevelManager.Instance.InjectToFlowStartGame(this, 2);
        }

        public void Execute(Action callback)
        {
            ActiveBeginingBoosters(callback);
        }

        private void ActiveBeginingBoosters(Action callback)
        {
            StartCoroutine(IEActiveBeginingBoosters(callback));
        }

        private IEnumerator IEActiveBeginingBoosters(Action callback)
        {
            yield return new WaitForEndOfFrame();

            var timeBeginBooster = (TimePreBooster)BoosterManager.Instance.TimePreBooster;
            var scissorBooster = (ScissorBooster)BoosterManager.Instance.ScissorBooster;
            if (timeBeginBooster.IsSelectedToUse || scissorBooster.IsSelectedToUse)
            {
                if (timeBeginBooster.IsSelectedToUse)
                {
                    timeBeginBooster.ActiveBooster();
                    LevelGame.AddTime(timeBeginBooster.bonusTime);

                }

                if (scissorBooster.IsSelectedToUse)
                {
                    scissorBooster.ActiveBooster(LevelGame.ListBody.Find(x => x.Length > 3 && x.BodyData.freezeTimeCount <= 0));
                }

                if (timeBeginBooster.IsSelectedToUse)
                {
                    yield return new WaitUntil(() => !timeBeginBooster.InProgress);
                    UIManager.Instance.GetScreenActive<InGameScreenUI>().ShowAddTimeAnim((int)timeBeginBooster.bonusTime);
                }
                if (scissorBooster.IsSelectedToUse)
                {
                    yield return new WaitUntil(() => !scissorBooster.InProgress);
                }

                timeBeginBooster.IsSelectedToUse = false;
                scissorBooster.IsSelectedToUse = false;
            }

            callback?.Invoke();

        }

    }
}
