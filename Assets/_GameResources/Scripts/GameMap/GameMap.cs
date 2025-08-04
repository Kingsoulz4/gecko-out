using System;
using UnityEngine;
using Geckout.Data;
using UnityEngine.Serialization;

namespace Geckout
{
    public class GameMap : MonoBehaviour
    {
        private static GameMap _instance;
        [SerializeField] GameLevelData levelData;
        [SerializeField] private GameTile tilePrefab;
        private Transform _tilesContainer;
        private Transform _entitiesContainer;
        private GameTile[] tiles;
        private Vector2Int _mapSize;
        public static Vector2Int MapSize => _instance._mapSize;

        private void Awake()
        {
            _instance = this;
            Initialize(levelData);
        }
        void CreateContainers()
        {
            DestroyContainers();
            _tilesContainer = CreateChild("TilesContainer");
            _entitiesContainer = CreateChild("EntitiesContainer");
        }

        void DestroyContainers()
        {
            if (_tilesContainer != null) Destroy(_tilesContainer.gameObject);
            if (_entitiesContainer != null) Destroy(_entitiesContainer.gameObject);
        }

        private Transform CreateChild(string childName)
        {
            var child = new GameObject(childName).transform;
            child.SetParent(transform);
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return child;
        }

        public void Initialize(GameLevelData levelData)
        {
            // Initialize the map based on levelData

            CreateContainers();

            SpawnAllTiles(levelData);
        }

        public static bool TryGetTileAt(int x, int y, out GameTile result)
        {
            if (x < 0 || y < 0 || x >= _instance._mapSize.x || y >= _instance._mapSize.y)
            {
                result = null;
                return false;
            }

            var index = x + y * _instance._mapSize.x;
            result = _instance.tiles[index];
            return true;
        }
        
        public static bool TryGetTileAt(Vector2Int coordinate, out GameTile result)
        {
            var x= coordinate.x;
            var y = coordinate.y;
            if (x < 0 || y < 0 || x >= _instance._mapSize.x || y >= _instance._mapSize.y)
            {
                result = null;
                return false;
            }

            var index = x + y * _instance._mapSize.x;
            result = _instance.tiles[index];
            return true;
        }

        void SpawnAllTiles(GameLevelData levelData)
        {
            _mapSize = levelData.MapSize;
            tiles = new GameTile[_mapSize.x * _mapSize.y];
            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    Vector3 offSet = new Vector3(_mapSize.x - 1, _mapSize.y - 1, 0) * 0.5f;
                    GameTile tile = Instantiate(tilePrefab, new Vector3(x, y, 0) - offSet, Quaternion.identity);
                    tile.name = $"Tile {x}, {y}";
                    tiles[x + y * _mapSize.x] = tile;

                    tile.SetCoordinate(x, y);
                    tile.transform.SetParent(_tilesContainer);
                    // tile.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}