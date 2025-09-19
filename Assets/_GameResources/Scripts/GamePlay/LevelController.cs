using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Geckout
{
    public partial class LevelController : MonoBehaviour
    {
        [SerializeField] protected GameLevelData m_gameLevelData;
        [SerializeField] protected GameMap m_gameMap;
        [SerializeField] protected BodyController m_bodyPrefab;
        [SerializeField] protected Transform m_bodyParent;

        private float currentTimeRemaining = 0;
        private Coroutine countDownCoroutine;

        private List<BodyController> listBody = new();

        public GameLevelData GameLevelData => m_gameLevelData;

        public GameMap GameMap => m_gameMap;

        public List<BodyController> ListBody { get => listBody;}

        public float CurrentTimeLevelRemaining => currentTimeRemaining;

        #region Boosters
        private bool IsFreezingTime { get; set; }
        private Coroutine freezeTimeCoroutine { get; set; }

        #endregion

        private void OnEnable()
        {
            LevelEvent.OnMoveToPortalDone += OnBodyMoveToPortal;
        }

        private void OnDisable()
        {
            LevelEvent.OnMoveToPortalDone -= OnBodyMoveToPortal;
        }

        public void SetLevelData(GameLevelData gameLevelData)
        {
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
            Utils.RemoveAllChilds(m_bodyParent);
            Camera.main.fieldOfView = gameLevelData.fieldOfView;
            int i=0;
            ListBody.Clear();
            foreach (var bodyData in gameLevelData.listDogData)
            {
                var body = SpawnBody(bodyData);
                body.name = $"Body_{i}";
                i++;
                ListBody.Add(body);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);   
#endif
        }

        public void StartLevel()
        {
            StartCountDownTime(GameLevelData.time);
        }

        private void WinLevel()
        {
            LevelEvent.OnWin?.Invoke(m_gameLevelData.levelNum);
        }

        private void LoseLevel()
        {
            LevelEvent.OnLose?.Invoke(m_gameLevelData.levelNum);
        }

        private void ReviveLevel()
        {
            StartCountDownTime(20);
        }

        private void OnBodyMoveToPortal(BodyController body, Portal portal)
        {
            if (listBody.Contains(body))
            {
                listBody.Remove(body);
            }

            if (listBody.Count == 0)
            {
                WinLevel();
            }
        }

        private void StartCountDownTime(float time)
        {
            StopCountDownTime();
            countDownCoroutine = StartCoroutine(IECountDownTime(time));
        }

        private void StopCountDownTime()
        {
            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
                countDownCoroutine = null;
            }
        }

        private IEnumerator IECountDownTime(float time)
        {
            currentTimeRemaining = time;
            while(currentTimeRemaining > 0)
            {
                if(!IsFreezingTime)
                {
                    currentTimeRemaining -= Time.deltaTime;
                }
                yield return null;
            }
            TimeOut();
        }

        private void TimeOut()
        {
            var popupTimeIsUp = UIManager.Instance.ShowPopup<PopupTimeIsUp>(() => { });
            popupTimeIsUp.OnClose = LoseLevel;
            popupTimeIsUp.OnKeepPlaying = ReviveLevel;
            //LoseLevel();
        }

        public BodyController SpawnBody(BodyData bodyData)
        {
            var bodyPrefab = m_bodyPrefab;

#if UNITY_EDITOR
            var newBody = (BodyController)PrefabUtility.InstantiatePrefab(m_bodyPrefab, m_bodyParent);
#else
            var newBody = Instantiate(m_bodyPrefab, m_bodyParent);
#endif
            newBody.Initialize(bodyData);
            return newBody;
        }

        #region Boosters

        public void AddTime(float time)
        {
            currentTimeRemaining += time;
        }

        public void FreezeTime(float timeFreeze)
        {
            if(freezeTimeCoroutine != null)
            {
                StopCoroutine(freezeTimeCoroutine);
                freezeTimeCoroutine = null;
            }    
            freezeTimeCoroutine = StartCoroutine(IEFreezeTime(timeFreeze));
        }

        private IEnumerator IEFreezeTime(float timeFreeze)
        {
            var currentTime = 0f;
            IsFreezingTime = true;
            while(currentTime <= timeFreeze)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }
            IsFreezingTime = false;

        }
        #endregion

    }
}
