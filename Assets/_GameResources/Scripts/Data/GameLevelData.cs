using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Geckout.Data
{
    public enum LevelType
    {
        EASY,
        NORMAL,
        HARD,
        SUPER_HARD
    }

    public class GameLevelData : ScriptableObject
    {
        public int levelNum;
        public int levelIndex;
        public Vector2Int mapSize = new(8, 12);
        public int time;
        public LevelType type;
        public List<MapTileData> mapTileDatas = new();
        public List<BodyData> listDogData = new();
        public List<PortalData> listPortalData = new();
        public List<MovableBoxData> listMovableBoxData = new();
        public ColorAndMaterialData colorAndMaterialData;

        public void GenerateDefaultMap()
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
                    data.type = MapTileType.Normal;

                    mapTileDatas.Add(data);
                }
            }

            Debug.Log($"Generated {mapTileDatas.Count} MapTileData entries for grid {gridSize.x}x{gridSize.y}");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // mark as dirty so data is saved
#endif
        }

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

        private void OnValidate()
        {
            if (colorAndMaterialData == null)
            {
                colorAndMaterialData = Resources.Load<ColorAndMaterialData>("ColorsAndMaterials/ColorAndMaterialData");
            }
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

    public enum ColorType
    {
        Red = 0,
        Orange = 1,
        Yellow = 2,
        Green = 3,
        Blue = 4,
        Violet = 5,
        Pink = 6,
        Brown = 7,
        White = 8,
        Black = 9,
        Cyan = 10,
        Grey = 11,
        Purple = 12,
        BabyPink = 13,
        RedWine = 14
    }

    public enum MapTileType
    {
        Normal,
        Wall4Side,
        WallCornerInside,
        Wall1Side,
        Wall2Side,
        Wall3Side,
        Portal
    }

    public enum BodyType
    {
        Normal,
        DoubleColor,
        TripleColor
    }

    public class MovableBoxData
    {
        
    }

    public enum PortalType
    {
        Normal = 0,
        Ice = 1,
    }

    public enum WayDirection
    {
        Horizontal = 0,
        Vertical = 1,
        All = 2,
    }

    [Serializable]
    public class PortalData
    {
        public PortalType portalType;
        public Vector2Int Coordinate;
        public List<ColorType> listColor = new() { ColorType.Red };
    }

    [Serializable]
    public class BodyData
    {
        public BodyType bodyType;
        public List<ColorType> listColor = new() { ColorType.Red };
        public List<Vector2Int> listCoordinate = new();

        public BodyData()
        { }

        public BodyData(BodyData bodyData)
        {
            this.bodyType = bodyData.bodyType;
            this.listColor = new List<ColorType>(bodyData.listColor);
            this.listCoordinate = new List<Vector2Int>(bodyData.listCoordinate);
        }
    }

    [Serializable]
    public class MapTileData
    {
        public Vector2Int coordinate;
        public MapTileType type;
        public Vector3Int rotation = new Vector3Int(0, 90, -90);
    }
}