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
        private static GameState gameState = GameState.MainMenu;
        Action<GameState> OnGameStateChange;
        public static GameState GameState { get => gameState; }

        private void Start()
        {
            HeartManager.CF_EnableHeart = 1;
            HeartManager.CF_RecoverTimeHeart = 60 * 20;
            SetGameState(GameState.MainMenu);
            var loading = UIManager.Instance.ShowScreen<LoadingScreen>();
            loading.Show(() =>
            {
                UIManager.Instance.ShowScreen<MainScreenUI>();
            });
        }

        public void SetGameState(GameState newState)
        {
            if (gameState == newState) return;
            gameState = newState;
            OnGameStateChange?.Invoke(gameState);
        }
    }
}
