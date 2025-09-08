using System;

namespace Geckout.Generals
{
    public static class GamePlayEventManager
    {
        public static Action<GameTile> OnTileSelected;
    }
}