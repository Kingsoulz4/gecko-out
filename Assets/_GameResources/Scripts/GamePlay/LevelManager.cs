using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class LevelManager : SingletonDontDestroyMono<LevelManager>, IGameState
    {
        [SerializeField] private LevelController m_levelGameOriginal;

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
                return PlayerPrefs.GetInt("CurrentLevelNum", 2);
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

        public void StartCurrentLevel()
        {
            StartLevel(CurrentLevelNum, CurrentLevelIndex);
        }

        public void StartLevel(int levelNum, int levelIndex)
        {
            var levelData = LoadLevel(CurrentLevelNum, CurrentLevelIndex);
            LevelGame.SetLevelData(levelData);
        }

        public GameLevelData LoadLevel(int levelNum, int levelIndex)
        {
            var levelData = Resources.Load<GameLevelData>($"Levels/{levelIndex}/Level{levelNum}");
            if(levelData == null)
            {
                levelData = Resources.Load<GameLevelData>($"Levels/0/Level1");
            }

            return levelData;   
        }    

        #region GameState
        public void OnEndGame()
        {
            throw new System.NotImplementedException();
        }

        public void OnPauseGame()
        {
            throw new System.NotImplementedException();
        }

        public void OnPlayingGame()
        {
            throw new System.NotImplementedException();
        }

        public void OnStartGame()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
