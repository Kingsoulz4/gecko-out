using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class LevelManager : SingletonDontDestroyMono<LevelManager>, IGameState
    {
        [SerializeField] private LevelController m_levelGameOriginal;
        [SerializeField] private int levelTest;

        public bool IsEdittingLevel { get; set; } = false;

        private LevelController levelGame;

        public LevelController LevelGame { 
            get
            {
                if(levelGame == null)
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

        public int CurrentLevelNum
        {
            get
            {
                return PlayerPrefs.GetInt("CurrentLevelNum", levelTest);
            }
            set
            {
                PlayerPrefs.SetInt("CurrentLevelNum", value);
            }
        }

        public int CurrentLevelIndex
        {
            get
            {
                return PlayerPrefs.GetInt("CurrentLevelIndex", 0);
            }
            set
            {
                PlayerPrefs.SetInt("CurrentLevelIndex", value);
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
            StartLevel(CurrentLevelNum, CurrentLevelIndex);
        }

        public void StartLevel(int levelNum, int levelIndex)
        {
            var levelData = LoadLevel(CurrentLevelNum, CurrentLevelIndex);
            LevelGame.SetLevelData(levelData);
            OnStartGame(CurrentLevelNum);
        }

        public GameLevelData LoadLevel(int levelNum, int levelIndex)
        {
            var levelData = Resources.Load<GameLevelData>($"Levels/{levelIndex}/Level{levelNum}");
            if(levelData == null)
            {
                levelData = Resources.Load<GameLevelData>($"Levels/0/Level1");
            }

            return new GameLevelData(levelData);   
        }

        #region GameState
        public void OnLoseGame(int level)
        {
            UIManager.Instance.ShowPopup<PopupLose>(() =>
            {
                UIManager.Instance.ShowScreen<MainScreenUI>();
            });
        }

        public void OnPauseGame(int level)
        {
            throw new System.NotImplementedException();
        }

        public void OnPlayingGame(int level)
        {
            throw new System.NotImplementedException();
        }

        public void OnStartGame(int level)
        {
            LevelGame.StartLevel();
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
            CurrentLevelNum++;
            StartCurrentLevel();
        }
        #endregion
    }
}
