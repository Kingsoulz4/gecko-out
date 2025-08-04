using UnityEngine;

namespace Geckout.Data
{
    public class GameLevelData : ScriptableObject
    {
        [SerializeField] private Vector2Int mapSize;
        public Vector2Int MapSize => mapSize;
    }
}