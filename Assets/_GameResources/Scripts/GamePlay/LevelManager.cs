using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class LevelManager : SingletonDontDestroyMono<LevelManager>
    {
        [SerializeField] private LevelController m_levelGameOriginal;
        [SerializeField] private int levelTest;

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
            GameManager.Instance.SetGameState(GameState.Playing);
        }

        public void OnWinGame(int level)
        {
            UIManager.Instance.ShowPopup<PopupWin>(() =>
            {
                NextLevel();
            });
        }

        public void NextLevel()
        {
            CurrentLevel++;
            StartCurrentLevel();
        }
        #endregion
    }
}
