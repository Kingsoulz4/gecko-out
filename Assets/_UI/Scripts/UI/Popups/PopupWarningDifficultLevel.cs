using DG.Tweening;
using Geckout.Data;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class PopupWarningDifficultLevel : PopupUI, IFlowCallback
    {
        [SerializeField] private GameObject m_hardLevelObject;
        [SerializeField] private GameObject m_superHardLevelObject;
        [SerializeField] private SkeletonGraphic m_hardAnim;
        [SerializeField] private SkeletonGraphic m_superHardAnim;

        private LevelController LevelGame => LevelManager.Instance.LevelGame;

        public void Execute(Action callback)
        {
            if(LevelGame.GameLevelData.type != LevelType.HARD && LevelGame.GameLevelData.type != LevelType.SUPER_HARD)
            {
                Hide();
                callback?.Invoke();
                return;
            }    
            else
            {
                Show(LevelGame.GameLevelData.type, callback);
            }    
        }

        public void Show(LevelType levelType, Action callback)
        {
            base.Show(null);
            StartCoroutine(IEAnimate(levelType, callback));
        }

        private IEnumerator IEAnimate(LevelType levelType, Action callback)
        {
            m_hardLevelObject.SetActive(false);
            m_superHardLevelObject.SetActive(false);
            var objectShow = levelType == LevelType.HARD ? m_hardLevelObject : m_superHardLevelObject;
            objectShow.gameObject.SetActive(true);
            objectShow.transform.localScale = Vector3.zero;
            objectShow.transform.DOScale(1, 0.5f);

            m_hardAnim.AnimationState.SetAnimation(0, "animation", false);
            m_superHardAnim.AnimationState.SetAnimation(0, "animation hard", false);

            yield return new WaitForSeconds(3.4f);
            Hide();
            callback?.Invoke();
        }
    }
}
