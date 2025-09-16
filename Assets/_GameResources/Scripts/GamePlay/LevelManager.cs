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

        public bool CanCountTimeLevel { get; internal set; }

        public void StartCurrentLevel()
        {
            StartLevel(CurrentLevelNum, CurrentLevelIndex);
        }

        public void StartLevel(int levelNum, int levelIndex = 0)
        {
            var levelData = LoadLevel(CurrentLevelNum, CurrentLevelIndex);
            LevelGame.SetLevelData(levelData);
            LevelGame.StartLevel();
        }

        public GameLevelData LoadLevel(int levelNum, int levelIndex)
        {
            var levelData = Resources.Load<GameLevelData>($"Levels/{levelIndex}/Level{levelNum}");
            if (levelData == null)
            {
                levelData = Resources.Load<GameLevelData>($"Levels/0/Level1");
            }

            return new GameLevelData(levelData);
        }
    }
}
