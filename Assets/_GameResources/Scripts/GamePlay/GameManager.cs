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

    public class GameManager : SingletonDontDestroyMono<GameManager>
    {
        private static GameState gameState = GameState.MainMenu;
        Action<GameState> OnGameStateChange;
        public static GameState GameState { get => gameState; }

        public void SetGameState(GameState newState)
        {
            if (gameState == newState) return;
            gameState = newState;
            OnGameStateChange?.Invoke(gameState);
        }
    }
}
