using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Geckout.Data
{
    public class GameLevelData : ScriptableObject
    {
        [SerializeField] private Vector2Int mapSize;
        public Vector2Int MapSize => mapSize;

        public List<MapTileData> mapTileDatas;
        public List<DogData> listDogData;

        [ContextMenu("Generate Test Data")]
        public void GenerateTestData()
        {
            mapTileDatas.Clear();
            var gridSize = mapSize;

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    MapTileData data = new MapTileData();
                    data.coordinate = new Vector2Int(x, y);

                    // pick random type
                    data.type = (MapTileType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(MapTileType)).Length);

                    mapTileDatas.Add(data);
                }
            }

            Debug.Log($"Generated {mapTileDatas.Count} MapTileData entries for grid {gridSize.x}x{gridSize.y}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // mark as dirty so data is saved
#endif
        }

    }


    [Serializable]
    public class Vector3IntSerial
    {
        public int x, y, z;
        public Vector3IntSerial(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public enum MapTileType
    {
        Normal,
        Wall,
        WallCorner,
        Wall1Side,
        Wall2Side,
        Wall2Side2Corner,
    }

    public enum DogType
    {
        Normal,
        DoubleColor,

    }

    public enum DogColor
    {
        Red,
        Green,

    }

    public class MovableBoxData
    {

    }

    [Serializable]
    public class DogData
    {
        public DogType dogType;
        public List<DogColor> listColor;
        public List<Vector2Int> listCoordinate;

    }

    [Serializable]
    public class MapTileData
    {
        public Vector2Int coordinate;
        public MapTileType type;
    }


    
    
}