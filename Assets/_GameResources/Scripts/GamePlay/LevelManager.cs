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

        public bool CanCountTimeLevel { get; internal set; }

        public void StartCurrentLevel()
        {
            StartLevel(CurrentLevel, CurrentLevelSetID);
        }

        public void StartLevel(int level, int levelSetID = 0)
        {
            var levelData = LoadLevel(level, levelSetID);
            LevelGame.SetLevelData(levelData);
            LevelGame.StartLevel();
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
    }
}
