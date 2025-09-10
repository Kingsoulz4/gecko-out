using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Geckout
{
    

    public partial class BoxMove : MonoBehaviour
    {
        [SerializeField] private LayerMask moveBoxLayer;
        [SerializeField] private GameObject m_tileCornerPrefab;
        [SerializeField] private GameObject m_tileEdgePrefab;
        [SerializeField] private GameObject m_tileCenterPrefab;
        [SerializeField] private GameObject m_tileCorner3EdgePrefab;
        [SerializeField] private GameObject m_tile2EdgePrefab;
        [SerializeField] private GameObject m_tile4EdgePrefab;

        [SerializeField] private GameObject m_arrowHorizontal;
        [SerializeField] private GameObject m_arrowVertical;

        private WayDirection wayDirection = WayDirection.Horizontal;
        public MovableBoxData MovableBoxData { get; private set; } = new();
        private List<MovableBoxTile> listMovableBoxTile = new();

        public void Init(MovableBoxData movableBoxData)
        {
            this.MovableBoxData = movableBoxData;
            
            SpawnTiles();

            UpdateVisual();
        }

        void SpawnTiles()
        {
            var boxSize = MovableBoxData.boxSize;
            bool needSetOccupied = false;
            if(LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Contains(MovableBoxData)) needSetOccupied = true;

            if (MovableBoxData.boxSize.x == 1 && MovableBoxData.boxSize.y == 1)
            {
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(MovableBoxData.rootCoordinate.x, MovableBoxData.rootCoordinate.y), out var tile);

                GameObject obj;

                obj = Instantiate(m_tile4EdgePrefab, transform);

                obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                obj.transform.localScale = Vector3.one * 1;
                obj.name = $"Tile";
                obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
                if(needSetOccupied) tile.IsOccupied = true;
            }
            else if (MovableBoxData.boxSize.x == 1)
            {
                for (int x = 0; x < boxSize.x; x++)
                {
                    for (int y = 0; y < boxSize.y; y++)
                    {
                        Vector2Int c = new Vector2Int(x, y);
                        var prefab = m_tile2EdgePrefab;
                        if (y == 0 || y == boxSize.y - 1)
                        {
                            prefab = m_tileCorner3EdgePrefab;
                        }

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(MovableBoxData.rootCoordinate.x + c.x, MovableBoxData.rootCoordinate.y + c.y), out var tile);
                        GameObject obj;

                        obj = Instantiate(prefab, transform);

                        obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        //obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
                        if(y == 0)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(180, 90, -90));
                        }
                        else if(y == boxSize.y - 1)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(180, -90, 90));
                        }
                        else
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));
                        }

                        var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                        movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                        listMovableBoxTile.Add(movableBoxTile);
                        if (needSetOccupied) tile.IsOccupied = true;
                    }
                }
            }
            else if (MovableBoxData.boxSize.y == 1)
            {

                for (int x = 0; x < boxSize.x; x++)
                {
                    for (int y = 0; y < boxSize.y; y++)
                    {
                        Vector2Int c = new Vector2Int(x, y);
                        var prefab = m_tile2EdgePrefab;
                        if (x == 0 || x == boxSize.x - 1)
                        {
                            prefab = m_tileCorner3EdgePrefab;
                        }

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(MovableBoxData.rootCoordinate.x + c.x, MovableBoxData.rootCoordinate.y + c.y), out var tile);

                        GameObject obj;

                        obj = Instantiate(prefab, transform);

                        obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        if (x == 0)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, 90, -90));
                        }
                        else if (x == boxSize.x - 1)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, -90, 90));
                        }
                        else
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, 90, -90));
                        }

                        var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                        movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                        listMovableBoxTile.Add(movableBoxTile);
                        if (needSetOccupied) tile.IsOccupied = true;
                    }
                }
            }
            else
            {
                for (int x = 0; x < boxSize.x; x++)
                {
                    for (int y = 0; y < boxSize.y; y++)
                    {
                        Vector2Int c = new Vector2Int(x, y);

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(MovableBoxData.rootCoordinate.x + c.x, MovableBoxData.rootCoordinate.y + c.y), out var tile);

                        GameObject obj = null;

                        if (x == 0 && y == 0)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerBottomLeft = new Vector3(0, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerBottomLeft);
                        }
                        else if (x == 0 && y == boxSize.y -1)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerTopLeft = new Vector3(90, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerTopLeft);
                        }
                        else if(x == boxSize.x -1 && y == 0)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerBottomRight = new Vector3(-90, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerBottomRight);
                        }
                        else if(x== boxSize.x - 1 && y == boxSize.y -1)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerTopRight = new Vector3(0, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerTopRight);
                        }
                        else if(x == 0 && (y != 0 && y != boxSize.y -1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleLeftEdge = new Vector3(90, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleLeftEdge);
                        }
                        else if (x == boxSize.x -1 && (y != 0 && y != boxSize.y - 1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleRightEdge = new Vector3(-90, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleRightEdge);
                        }
                        else if (y == 0 && (x != 0 && x != boxSize.x - 1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleBottomEdge = new Vector3(-180, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleBottomEdge);
                        }
                        else if (y == boxSize.y -1 && (x != 0 && x != boxSize.x - 1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleTopEdge = new Vector3(-180, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleTopEdge);
                        }
                        else
                        {
                            obj = Instantiate(m_tileCenterPrefab, transform);
                            obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
                        }

                        obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                        movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                        listMovableBoxTile.Add(movableBoxTile);
                        if (needSetOccupied) tile.IsOccupied = true;
                    }
                }
            }
        }

        private void Move()
        {
            // check way direction
            switch (wayDirection)
            {
                case WayDirection.Horizontal:
                    // move left or right
                    break;
                case WayDirection.Vertical:
                    // move up or down
                    break;
                case WayDirection.All:
                    // move in any direction
                    break;
                default:
                    break;
            }
        }

        public void UpdateVisual()
        {
            SetCenterPos();
            m_arrowHorizontal.transform.localPosition = Vector3.forward * -0.4f;
            m_arrowVertical.transform.localPosition = Vector3.forward * -0.4f;
            m_arrowHorizontal.SetActive(MovableBoxData.wayDirection == WayDirection.Horizontal || MovableBoxData.wayDirection == WayDirection.All);
            m_arrowVertical.SetActive(MovableBoxData.wayDirection == WayDirection.Vertical || MovableBoxData.wayDirection == WayDirection.All);
        }

        public void SetCenterPos()
        {
            SetPivotAndPosition(GetCenterWorldPos());
        }

        public Vector3 GetCenterWorldPos()
        {
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(MovableBoxData.rootCoordinate, out var tileLeft);
            var rightCoord = new Vector2Int(MovableBoxData.rootCoordinate.x + MovableBoxData.boxSize.x - 1, MovableBoxData.rootCoordinate.y);
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(rightCoord, out var tileRight);
            var topCoord = new Vector2Int(MovableBoxData.rootCoordinate.x, MovableBoxData.rootCoordinate.y + MovableBoxData.boxSize.y - 1);
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(topCoord, out var tileTop);
            var centerPos = new Vector3((tileLeft.transform.position.x + tileRight.transform.position.x) / 2, (tileLeft.transform.position.y + tileTop.transform.position.y) / 2, tileLeft.transform.position.z);
            return centerPos;

        }

        public void SetPivotAndPosition(Vector3 newPivotWorldPos)
        {
            // Calculate how much the pivot moves in world space
            Vector3 pivotDelta = transform.position - newPivotWorldPos;

            // Move each child so its world position stays the same
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.position += pivotDelta;
            }

            // Finally, move parent pivot
            transform.position = newPivotWorldPos;
        }
    }
}
