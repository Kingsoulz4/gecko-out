using Geckout.Data;
using Geckout.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using static DG.Tweening.DOTweenAnimation;

namespace Geckout
{
    public class LevelManager : SingletonDontDestroyMono<LevelManager>
    {
        [SerializeField] private LevelController m_levelGameOriginal;
        [SerializeField] private int levelTest;

        private SortedDictionary<int, IFlowCallback> queueFlowStartGame = new();

        public bool IsEdittingLevel { get; set; } = false;

        private LevelController levelGame;

        private int priceRevive = 900;

        public LevelController LevelGame
        {
            get
            {
                if (levelGame == null)
                {
                    levelGame = Instantiate(m_levelGameOriginal);
                }

                return levelGame;
            }
            set
            {
                levelGame = value;
            }
        }

        public int CurrentLevel
        {
            get
            {
                return UserDataManager.Level;
            }
            set
            {
                UserDataManager.Level = value;
            }
        }

        public int CurrentLevelSetID
        {
            get
            {
                return UserDataManager.LevelSetID;
            }
            set
            {
                UserDataManager.LevelSetID = value;
            }
        }

        private void OnEnable()
        {
            LevelEvent.OnWin += OnWinGame;
            LevelEvent.OnLose += OnLoseGame;
            
        }

        private void OnDisable()
        {
            LevelEvent.OnWin -= OnWinGame;
            LevelEvent.OnLose -= OnLoseGame;
        }

        public void StartCurrentLevel()
        {
            StartLevel(CurrentLevel, CurrentLevelSetID);
        }

        public void StartLevel(int level, int levelSetID = 0)
        {
            var levelData = LoadLevel(level, levelSetID);
//#if !UNITY_EDITOR
            Destroy(LevelGame.gameObject);
            LevelGame = Instantiate(m_levelGameOriginal);
//#endif
            LevelGame.SetLevelData(levelData);
            OnStartGame(CurrentLevel);
        }

        public GameLevelData LoadLevel(int level, int levelSetID)
        {
            var levelData = Resources.Load<GameLevelData>($"Levels/{levelSetID}/Level{level}");
            if (levelData == null)
            {
                levelData = Resources.Load<GameLevelData>($"Levels/0/Level1");
            }

            return new GameLevelData(levelData);   
        }

        public LevelType GetCurrentLevelType()
        {
            var levelData = LoadLevel(CurrentLevel, CurrentLevelSetID);
            return levelData.type;
        }    

        #region GameState

        public void OnLoseGame(int level)
        {
            var popupLose = UIManager.Instance.ShowPopup<PopupLose>(() =>
            {
                //UIManager.Instance.ShowScreen<MainScreenUI>();
            });

            HeartManager.UseHeart(1);

            popupLose.OnClose = () =>
            {
                UIManager.Instance.ShowScreen<MainScreenUI>();
            };
            popupLose.OnRetry = OnRetryGame;
        }

        public void OnRetryGame()
        {
            if(UserDataManager.Heart > 0)
            {
                StartCurrentLevel();
                UIManager.Instance.ShowScreen<InGameScreenUI>();
            }
            else
            {
                var popupGetMoreLives = UIManager.Instance.ShowPopup<PopupGetMoreLives>(null);
                popupGetMoreLives.OnRefilled = () =>
                {
                    StartCurrentLevel();
                };
                popupGetMoreLives.OnClose = () =>
                {
                    UIManager.Instance.ShowScreen<MainScreenUI>();
                };
            }
        }

        public void OnReviveGame()
        {
            if(UserDataManager.Gold >= priceRevive)
            {
                UserDataManager.AddGold(-priceRevive, "Revive");
                
            }
            else
            {

            }
        }

        public void OnPauseGame(int level)
        {
            throw new System.NotImplementedException();
        }
   

        public void OnStartGame(int level)
        {
            LevelGame.StartLevel();

            CheckShowTutorials();

            var popupTutNewFeature = UIManager.Instance.GetPopup<PopupTutorialNewFeature>();
            InjectToFlowStartGame(popupTutNewFeature, 1);
            var popupWarningDifficultLevel = UIManager.Instance.GetPopup<PopupWarningDifficultLevel>();
            InjectToFlowStartGame(popupWarningDifficultLevel, 0);

            ExecuteNextFlowStep();

        }

        private void CheckShowTutorials()
        {
            if(CurrentLevel == 1 || CurrentLevel == 2)
            {
                LevelGame.ActiveTutLevel1();
            }
        }

        public void OnWinGame(int level)
        {
            GameManager.Instance.SetGameState(GameState.Win);
            UIManager.Instance.ShowPopup<PopupWin>(() =>
            {
                NextLevel();
            });
        }

        public void NextLevel()
        {
            CurrentLevel++;
            
            StartCurrentLevel();
            UIManager.Instance.ShowScreen<InGameScreenUI>();
        }
        #endregion

        #region Flow Start Game

        public void InjectToFlowStartGame(IFlowCallback step, int priority = -1)
        {
            if (priority < 0)
            {
                queueFlowStartGame[queueFlowStartGame.Count] = step;
            }
            else
            {
                queueFlowStartGame[priority] = step;
            }
        }

        public void ExecuteNextFlowStep()
        {
            if (queueFlowStartGame.Count <= 0)
            {
                DoneFlowStartGame();
                return;
            }

            var topElement = queueFlowStartGame.First();
            queueFlowStartGame.Remove(topElement.Key);
            topElement.Value.Execute(ExecuteNextFlowStep);
            
        }

        public void DoneFlowStartGame()
        {
            GameManager.Instance.SetGameState(GameState.Playing);

        }

        #endregion
    }
}
