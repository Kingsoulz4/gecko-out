using Geckout.Data;
using Geckout.PathFinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Geckout
{
    public partial class LevelController : MonoBehaviour
    {
        [SerializeField] protected GameLevelData m_gameLevelData;
        [SerializeField] protected GameMap m_gameMap;
        [SerializeField] protected BodyController m_bodyPrefab;
        [SerializeField] protected Transform m_bodyParent;

        [Header("Booster")]
        [SerializeField] private BeginingBoosterManager m_beginingBoosterManager;

        [Header("Tutorial")]
        [SerializeField] private TutorialHandGuide m_tutorialHandGuide;
       

        private float currentTimeRemaining = 0;
        private Coroutine countDownCoroutine;

        private List<BodyController> listBody = new();

        public GameLevelData GameLevelData => m_gameLevelData;

        public GameMap GameMap => m_gameMap;

        public List<BodyController> ListBody { get => listBody; }

        public float CurrentTimeLevelRemaining => currentTimeRemaining;

        public bool IsFirstClick { get; set; }

        #region Boosters
        private bool IsFreezingTime { get; set; }
        public bool CanCountdownTime => !IsFreezingTime && IsFirstClick && GameManager.GameState == GameState.Playing;
        private Coroutine freezeTimeCoroutine { get; set; }

        #endregion

        private void Awake()
        {
            
        }

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
            IsFirstClick = false;
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
            MyUlti.RemoveAllChilds(m_bodyParent);
            Camera.main.fieldOfView = gameLevelData.fieldOfView;
            int i=0;
            ListBody.Clear();
            foreach (var bodyData in gameLevelData.listDogData)
            {
                var body = SpawnBody(bodyData);
                body.name = $"Body_{i}";
                i++;
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
            HideAllTuts();

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
            while (currentTimeRemaining > 0)
            {
                if (CanCountdownTime)
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
            ListBody.Add(newBody);
            return newBody;
        }

        public void UpdateAllBodyState()
        {
            listBody.ForEach(x => x.OccupiedTileController.UpdateAllSegmentPositions(isChangeTileColor:false,forceUpdate: true));
        }    

        #region Boosters

        public void AddTime(float time)
        {
            currentTimeRemaining += time;
        }

        public void FreezeTime(float timeFreeze)
        {
            if (freezeTimeCoroutine != null)
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
            while (currentTime <= timeFreeze)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }
            IsFreezingTime = false;

        }
        #endregion


        #region Tutorial

        public void HideAllTuts()
        {
            m_tutorialHandGuide.gameObject.SetActive(false);
        }    

        public void ShowTurialHandGuide(List<Vector3> path)
        {
            m_tutorialHandGuide.ShowGuidePath(path);
        }

        public void ActiveTutLevel1()
        {
            StartCoroutine(IEActiveTutLevel1());
        }

        private IEnumerator IEActiveTutLevel1()
        {
            m_tutorialHandGuide.gameObject.SetActive(true);
            var bodyGuide = ListBody.First();
            var startCoord = bodyGuide.Segments.First().Coordinate;

            var endCoord = GameMap.ListPortal.Find(x => x.PortalData.listColor.First() == bodyGuide.BodyData.listColor.First()).PortalData.Coordinate;
            GameMap.TryGetTileAtCoord(endCoord, out var tilePortal);
            tilePortal.SetOccupied(false);

            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            var pathfinder = new ASPathFinding(grid);

            var pathMove = new List<Vector3>();
            pathMove.Add(bodyGuide.Segments.First().transform.position);
            pathfinder.Reset();
            pathfinder.FindPath(startCoord, endCoord, (path) =>
            {
                path.ForEach(x =>
                {
                    GameMap.TryGetTileAtCoord(x.Position, out var tile);
                    pathMove.Add(tile.transform.position);
                });
            });
            tilePortal.SetOccupied(true);

            while (bodyGuide.gameObject.activeInHierarchy)
            {
                ShowTurialHandGuide(pathMove);
                yield return new WaitForSeconds(2f);
            }

            m_tutorialHandGuide.gameObject.SetActive(false);
        }

        #endregion

    }
}
