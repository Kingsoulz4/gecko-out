using Geckout.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Geckout
{
    public partial class GameMap : SingletonMono<GameMap>
    {
        private static GameMap _instance;
        [SerializeField] GameLevelData levelData;
        [SerializeField] private GameTile tilePrefab;
        [SerializeField] private bool isDebug = false;
        [SerializeField] private float offsetFactor = 1f;

        [Header("Tiles And Blocks")]
        [SerializeField] private GameTile m_tileWallEdge;
        [SerializeField] private GameTile m_tileWallCorner;
        [SerializeField] private Portal m_portalPrefab;
        [SerializeField] private BoxMove m_boxMovePrefab;
        [SerializeField] private Crate m_cratePrefab;

        [Header("Containers")]
        [SerializeField] private Transform _tilesContainer;
        [SerializeField] private Transform m_wallContainer;
        [SerializeField] private Transform m_portalsContainer;
        [SerializeField] private Transform m_movableBoxContainer;
        [SerializeField] private Transform m_crateContainer;

        [Header("Material Management")]
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material moveMaterial;
        private Transform _entitiesContainer;
        private GameTile[] tiles;
        private Vector2Int _mapSize;
        private List<Portal> listPortal = new();
        private List<BoxMove> listMovableBox = new();
        private List<Crate> listCrate = new();
        private List<GameTile> tileInGame = new();

        private static Vector2 gridOffset;

        public static Vector2Int MapSize => _instance._mapSize;

        public List<Portal> ListPortal { get => listPortal; }

        public bool IsDebug { get => isDebug;}
        public List<GameTile> TilesWall { get => tileInGame; set => tileInGame = value; }

        private void Awake()
        {
            _instance = this;
            //Initialize(levelData);
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
            //CreateContainers();
            //SpawnAllTiles(levelData);
            //GetAllTilesTest(levelData);
        }

        public static Vector3 GetTileWorldPosition(Vector2Int coord)
        {
            if (TryGetTileAtCoord(coord, out var tile))
            {
                return tile.transform.position;
            }
            else
            {
                Debug.LogWarning($"No tile found at {coord}");
                return Vector3.zero;
            }
        }

        private static bool TryGetTileAt(int x, int y, out GameTile result)
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

        public static bool TryGetTileAtCoord(Vector2Int coordinate, out GameTile result)
        {
            var x = coordinate.x;
            var y = coordinate.y;
            if (x < 0 || y < 0 || x >= _instance.levelData.mapSize.x || y >= _instance.levelData.mapSize.y)
            {
                result = null;
                return false;
            }

            //var index = x + y * levelData.mapSize.x;
            //result = tiles[index];

            result = _instance.tiles.First(x => x.Coordinate == coordinate);

            return  result != null;
        }

        public bool TryGetPortalAtCoord(Vector2Int coordinate, out Portal result)
        {
            var x = coordinate.x;
            var y = coordinate.y;
            if (x < 0 || y < 0 || x >= levelData.mapSize.x || y >= levelData.mapSize.y)
            {
                result = null;
                return false;
            }

            result = listPortal.Find(x => x.PortalData.Coordinate == coordinate);
            return result != null;
        }

        void GetAllTilesTest(GameLevelData levelData)
        {
            _mapSize = levelData.mapSize;
            _entitiesContainer = CreateChild("EntitiesContainer");
            if (_tilesContainer == null)
            {
                _tilesContainer = GameObject.Find("TilesContainer").transform;
            }    
            tiles = new GameTile[_mapSize.x * _mapSize.y];
            List<GameTile> listTile = _tilesContainer.GetComponentsInChildren<GameTile>().ToList();
            int i = 0;
            for (int y = 0; y < _mapSize.y; y++)
            {
                for (int x = 0; x < _mapSize.x; x++)
                {
                    var tile = listTile[i];
                    tiles[x + y * _mapSize.x] = tile;
                    tile.SetCoordinate(x, y);
                    tile.InitializeMaterials(defaultMaterial, moveMaterial);
                    i++;
                }
            }

            gridOffset = new Vector2(MapSize.x - 1, MapSize.y - 1) * 0.5f;
        }

        public void SetLevelData(GameLevelData levelData)
        {
            this.levelData = levelData;
            if(levelData.mapSize.x * levelData.mapSize.y != levelData.mapTileDatas.Count)
            {
                levelData.mapTileDatas.Clear();
            }   
            
            if (levelData.mapTileDatas != null && levelData.mapTileDatas.Count > 0)
            {
                SpawnAllTiles(levelData.mapTileDatas);
            }
            else
            {
                SpawnAllTiles();
            }

            GetAllTilesTest(levelData);

            SpawnPortals();

            SpawnMovableBoxes();

            SpawnCrates();
        }

        [ContextMenu("Test SpawnTiles")]
        void TestSpawnTiles()
        {
            SpawnAllTiles(levelData.mapTileDatas);

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }    

        public void SpawnPortal(PortalData portalData)
        {
            var portal = Instantiate(m_portalPrefab, m_portalsContainer);
            TryGetTileAtCoord(portalData.Coordinate, out var tile);
            portal.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, portal.transform.position.z);
            portal.Initialize(portalData);
            listPortal.Add(portal);
        }

        public Crate SpawnCrate(CrateData crateData)
        {
            var newCrate = Instantiate(m_cratePrefab, m_movableBoxContainer);
            newCrate.Init(crateData);
            return newCrate;
        }

        public BoxMove SpawnBoxMove(MovableBoxData movableBoxData)
        {
            var newMovableBox = Instantiate(m_boxMovePrefab, m_movableBoxContainer);
            newMovableBox.Init(movableBoxData);
            return newMovableBox;
        }

        void SpawnMovableBoxes()
        {
            Utils.RemoveAllChilds(m_movableBoxContainer);
            listMovableBox.Clear();
            foreach (var movableBox in levelData.listMovableBoxData)
            {
                listMovableBox.Add(SpawnBoxMove(movableBox));
            }

        }

        void SpawnPortals()
        {
            Utils.RemoveAllChilds(m_portalsContainer);
            listPortal.Clear();
            foreach(var portal in levelData.listPortalData)
            {
                SpawnPortal(portal);
            }
        }

        void SpawnCrates()
        {
            Utils.RemoveAllChilds(m_crateContainer);
            listCrate.Clear();
            foreach (var crateData in levelData.listCrateData)
            {
                listCrate.Add(SpawnCrate(crateData));
            }
        }

        void SpawnAllTiles()
        {
            TilesWall.Clear();
            if (_tilesContainer.transform.childCount > 0)
            {
                Utils.RemoveAllChilds(_tilesContainer);
            }

            if (m_wallContainer.transform.childCount > 0)
            {
                Utils.RemoveAllChilds(m_wallContainer);
            }

            var cubeSize = 1f;
            var spacing = 0f;
            var gridSize = levelData.mapSize;
            float cellSize = cubeSize + spacing;
            GameTile prefab = tilePrefab;

            // calculate offset so grid is centered at (0,0)
            Vector3 centerOffset = new Vector3(
                (gridSize.x - 1) * cellSize * 0.5f,
                (gridSize.y - 1) * cellSize * 0.5f,
                0

            );

            // spawn based on coordinates
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int c = new Vector2Int(x, y);
                    MapTileData tile = new();
                    tile.coordinate = c;
                    tile.type = MapTileType.Normal;
                    levelData.mapTileDatas.Add(tile);

                    // matrix coordinate → world position
                    Vector3 pos = new Vector3(c.x * cellSize, c.y * cellSize, 0);
                    pos -= centerOffset; // center grid

                    GameTile obj;
#if UNITY_EDITOR
                    obj = ((GameTile)PrefabUtility.InstantiatePrefab(prefab, _tilesContainer));
                    
#else
                    obj = Instantiate(prefab, _tilesContainer);
#endif
                    obj.SetCoordinate(c.x, c.y);
                    obj.transform.localPosition = pos;
                    obj.transform.localScale = Vector3.one * cubeSize;
                    obj.name = $"Tile_{c.x}_{c.y}_{tile.type}";
                    obj.Initialize(tile);
                    if (tile.type != MapTileType.Portal && tile.type != MapTileType.Normal)
                    {
                        TilesWall.Add(obj);
                    }

                }
            }

            SpawnWalls(gridSize, cellSize, cubeSize, centerOffset);

            //SpawnPortals();
        }

        void SpawnAllTiles(List<MapTileData> mapTileData)
        {
            if(_tilesContainer.transform.childCount > 0)
            {
                Utils.RemoveAllChilds(_tilesContainer);
            }

            if(m_wallContainer.transform.childCount > 0)
            {
                Utils.RemoveAllChilds(m_wallContainer);
            }

            var cubeSize = 1f;
            var spacing = 0f;
            var gridSize = levelData.mapSize;
            float cellSize = cubeSize + spacing;
            GameTile prefab = tilePrefab;

            // calculate offset so grid is centered at (0,0)
            Vector3 centerOffset = new Vector3(
                (gridSize.x - 1) * cellSize * 0.5f,
                (gridSize.y - 1) * cellSize * 0.5f,
                0
                
            );

            // spawn based on coordinates
            foreach (var tile in mapTileData)
            {
                Vector2Int c = tile.coordinate;

                // matrix coordinate → world position
                Vector3 pos = new Vector3(c.x * cellSize, c.y * cellSize, 0);
                pos -= centerOffset; // center grid

                GameTile obj;
#if UNITY_EDITOR
                obj = ((GameTile)PrefabUtility.InstantiatePrefab(prefab, _tilesContainer));
                
                obj.name = $"Tile_{c.x}_{c.y}_{tile.type}";
#else
            obj = Instantiate(prefab, _tilesContainer);
#endif
                obj.SetCoordinate(c.x, c.y);
                obj.transform.localPosition = pos;
                obj.transform.localScale = Vector3.one * cubeSize;
                obj.Initialize(tile);
                if (tile.type == MapTileType.Portal)
                {
                    //var portal = obj.gameObject.AddComponent<Portal>();
                    //portal.Initialize(levelData.listPortalData.Find(x => x.Coordinate == tile.coordinate));
                    //SpawnPortal(levelData.listPortalData.Find(x => x.Coordinate == tile.coordinate));
                }

            }

            SpawnWalls(gridSize, cellSize, cubeSize, centerOffset);

            //SpawnPortals();
        }
        public static Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            float gridX = worldPos.x + gridOffset.x;
            float gridY = worldPos.y + gridOffset.y;

            Vector2Int result = new Vector2Int(
                Mathf.RoundToInt(gridX),
                Mathf.RoundToInt(gridY)
            );

            return result;
        }
        private void SpawnWalls(Vector2Int gridSize, float cellSize, float cubeSize, Vector3 centerOffset)
        {
            // Corners
            var wallCornerPrefab = m_tileWallCorner;
            var wallEdgePrefab = m_tileWallEdge;

            var angleCornerBottomLeft = new Vector3(270, -90, 90);
            PlaceWall(wallCornerPrefab, new Vector2Int(-1, -1), cellSize, cubeSize, centerOffset, "Corner_BottomLeft", angleCornerBottomLeft);
            var angleCornerBottomRight = new Vector3(0, -90, 90);
            PlaceWall(wallCornerPrefab, new Vector2Int(gridSize.x, -1), cellSize, cubeSize, centerOffset, "Corner_BottomRight", angleCornerBottomRight);
            var angleCornerTopLeft = new Vector3(0, 90, -90);
            PlaceWall(wallCornerPrefab, new Vector2Int(-1, gridSize.y), cellSize, cubeSize, centerOffset, "Corner_TopLeft", angleCornerTopLeft);
            var angleCornerTopRight = new Vector3(90, -90, 90);
            PlaceWall(wallCornerPrefab, new Vector2Int(gridSize.x, gridSize.y), cellSize, cubeSize, centerOffset, "Corner_TopRight", angleCornerTopRight);

            // Bottom edge
            var angleBottomEdge = new Vector3(180, 90, -90);
            for (int x = 0; x < gridSize.x; x++)
                PlaceWall(wallEdgePrefab, new Vector2Int(x, -1), cellSize, cubeSize, centerOffset, $"Wall_Bottom_{x}", angleBottomEdge);

            // Top edge
            var angleTopEdge = new Vector3(0, 90, -90);
            for (int x = 0; x < gridSize.x; x++)
                PlaceWall(wallEdgePrefab, new Vector2Int(x, gridSize.y), cellSize, cubeSize, centerOffset, $"Wall_Top_{x}", angleTopEdge);

            // Left edge
            var angleLeftEdge = new Vector3(270, -90, 90);
            for (int y = 0; y < gridSize.y; y++)
                PlaceWall(wallEdgePrefab, new Vector2Int(-1, y), cellSize, cubeSize, centerOffset, $"Wall_Left_{y}", angleLeftEdge);

            // Right edge
            var angleRightEdge = new Vector3(90, -90, 90);
            for (int y = 0; y < gridSize.y; y++)
                PlaceWall(wallEdgePrefab, new Vector2Int(gridSize.x, y), cellSize, cubeSize, centerOffset, $"Wall_Right_{y}", angleRightEdge);
        }

        private void PlaceWall(GameTile prefab, Vector2Int c, float cellSize, float cubeSize, Vector3 centerOffset, string name, Vector3 localRotation)
        {
            if (prefab == null) return;

            Vector3 pos = new Vector3(c.x * cellSize, c.y * cellSize, 0) - centerOffset;

            GameObject obj;
#if UNITY_EDITOR
            obj = ((GameTile)PrefabUtility.InstantiatePrefab(prefab, m_wallContainer)).gameObject;
#else
            obj = Instantiate(prefab.gameObject, m_wallContainer);
#endif
            obj.name = name;
            obj.transform.localPosition = pos;
            obj.transform.localScale = Vector3.one * cubeSize;
            obj.transform.localRotation = Quaternion.Euler(localRotation);
        }

        void SpawnAllTiles(GameLevelData levelData)
        {
            _mapSize = levelData.mapSize;
            tiles = new GameTile[_mapSize.x * _mapSize.y];
            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    Vector3 offSet = new Vector3(_mapSize.x - 1, _mapSize.y - 1, 0) * 0.5f;
                    GameTile tile = Instantiate(tilePrefab, (new Vector3(x, y, 0) - offSet) * offsetFactor, Quaternion.identity);
                    tile.name = $"Tile {x}, {y}";
                    tiles[x + y * _mapSize.x] = tile;

                    tile.SetCoordinate(x, y);
                    tile.transform.SetParent(_tilesContainer);
                    // tile.transform.localRotation = Quaternion.identity;
                }
            }
        }

        public static Vector2Int WorldToGridPositionForward(Vector3 worldPos,BodyController bodyController)
        {
            float gridX = worldPos.x + gridOffset.x;
            float gridY = worldPos.y + gridOffset.y;

            Vector2Int currentTile = new Vector2Int(Mathf.RoundToInt(gridX), Mathf.RoundToInt(gridY));
            var movementDirection = bodyController.GridClamper.CurrentDirection;
            if (movementDirection == Vector2Int.zero)
                return currentTile;

            Vector3 currentTileCenter = new Vector3(
                currentTile.x - gridOffset.x,
                currentTile.y - gridOffset.y,
                0
            );

            bool crossedCenter = false;

            if (movementDirection.x > 0)
            {
                crossedCenter = worldPos.x > currentTileCenter.x;
            }
            else if (movementDirection.x < 0)
            {
                crossedCenter = worldPos.x < currentTileCenter.x;
            }
            else if (movementDirection.y > 0)
            {
                crossedCenter = worldPos.y > currentTileCenter.y;
            }
            else if (movementDirection.y < 0)
            {
                crossedCenter = worldPos.y < currentTileCenter.y;
            }

            if (crossedCenter)
            {
                return currentTile + movementDirection;
            }

            return currentTile;
        }
        public static bool[] GetCurrentMapState()
        {
            bool[] mapState = new bool[_instance.tiles.Length];
            for(int x = 0; x < _instance._mapSize.x; x++)
            {
                for (int y = 0; y < _instance._mapSize.y; y++)
                {
                    if (TryGetTileAt(x, y, out GameTile tile))
                    {
                        mapState[x + y * _instance._mapSize.x] = !tile.IsOccupied;
                    }
                }
            }
            return mapState;
        }

        public static void SetTilesUnoccupied(List<Vector2Int> coordinates)
        {
            if (coordinates == null || coordinates.Count == 0)
            {
                Debug.LogWarning("[GameMap] SetTilesUnoccupied: coordinates list is null or empty");
                return;
            }

            foreach (var coord in coordinates)
            {
                if (TryGetTileAtCoord(coord, out GameTile tile))
                {
                    tile.IsOccupied = false;
                }
                else
                {
                    Debug.LogWarning($"[GameMap] SetTilesUnoccupied: No tile found at coordinate {coord}");
                }
            }
        }

        public static void ApplyFuncToAllTiles(Action<GameTile> action)
        {
            foreach (var tile in _instance.tiles)
            {
                action(tile);
            }
        }
    }
}