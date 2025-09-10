using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public interface IGameState
    {
        void OnStartGame();
        void OnPauseGame();
        void OnPlayingGame();
        void OnEndGame();
    }
}
