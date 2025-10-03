using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public enum GameState
    {
        MainMenu = 0,
        Playing = 1,
        Paused = 2,
        Win = 3,
        Lose = 4,
    }

    public class GameManager : SingletonMono<GameManager>
    {
        [SerializeField] UIManager uiManager;

        private static GameState gameState = GameState.MainMenu;
        Action<GameState> OnGameStateChange;
        public static GameState GameState { get => gameState; }

        public int CoinRewardWinGame
        {
            get => PlayerPrefs.GetInt("CoinRewardWinGame", 40);
            set => PlayerPrefs.SetInt("CoinRewardWinGame", value);
        }

        public int CoinPriceRefillHeart
        {
            get => PlayerPrefs.GetInt("CoinPriceRefillHeart", 1200);
            set => PlayerPrefs.SetInt("CoinPriceRefillHeart", value);
        }

        private void Start()
        {
            InitAllManager();
            Init();
        }

        private void Init()
        {
            HeartManager.CF_EnableHeart = 1;
            HeartManager.CF_RecoverTimeHeart = 60 * 30;
            SetGameState(GameState.MainMenu);

            var loading = UIManager.Instance.ShowScreen<LoadingScreen>();
            loading.Show(() =>
            {
                UIManager.Instance.ShowScreen<MainScreenUI>();
            });
        }

        private void InitAllManager()
        {
            uiManager.Initialize();
        }

        public void SetGameState(GameState newState)
        {
            if (gameState == newState) return;
            gameState = newState;
            OnGameStateChange?.Invoke(gameState);
        }
    }
}
