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
        

        private WayDirection wayDirection = WayDirection.Horizontal;
        private MovableBoxData movableBoxData = new();
        private List<MovableBoxTile> listMovableBoxTile = new();

        public void Init(MovableBoxData movableBoxData)
        {
            this.movableBoxData = movableBoxData;
            SpawnTiles();
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(movableBoxData.boxSize.x, movableBoxData.boxSize.y, 1);
        }

        void SpawnTiles()
        {
            var boxSize = movableBoxData.boxSize;
            float cellSize = 1;

            if (movableBoxData.boxSize.x == 1 && movableBoxData.boxSize.y == 1)
            {
                GameObject obj;

                obj = Instantiate(m_tile4EdgePrefab, transform);

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one * 1;
                obj.name = $"Tile";
                obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
            }
            else if (movableBoxData.boxSize.x == 1)
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

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(movableBoxData.rootCoordinate.x + c.x, movableBoxData.rootCoordinate.y + c.y), out var tile);
                        GameObject obj;

                        obj = Instantiate(prefab, transform);

                        obj.transform.position = tile.transform.position;
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        //obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
                        if(y == 0)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));
                        }
                        else if(y == boxSize.y - 1)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, -90, 90));
                        }
                        else
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));
                        }

                        var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                        movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                        listMovableBoxTile.Add(movableBoxTile);
                    }
                }
            }
            else if (movableBoxData.boxSize.y == 1)
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

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(movableBoxData.rootCoordinate.x + c.x, movableBoxData.rootCoordinate.y + c.y), out var tile);

                        // matrix coordinate → world position

                        GameObject obj;

                        obj = Instantiate(prefab, transform);

                        obj.transform.position = tile.transform.position;
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        if (x == 0)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, 90, -90));
                        }
                        else if (x == boxSize.x - 1)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, -90, 90));
                        }
                        else
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, 90, -90));
                        }

                        var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                        movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                        listMovableBoxTile.Add(movableBoxTile);
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

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(movableBoxData.rootCoordinate.x + c.x, movableBoxData.rootCoordinate.y + c.y), out var tile);

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

                        //obj.transform.localPosition = pos;
                        obj.transform.position = tile.transform.position;
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                        movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                        listMovableBoxTile.Add(movableBoxTile);

                    }
                }

            }

        }

        public void MoveByOffset(Vector2Int offset)
        {
            foreach(var tileMove in listMovableBoxTile)
            {
                var newCoordinate = new Vector2Int(tileMove.Coordinate.x + offset.x, tileMove.Coordinate.y + offset.y);
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(newCoordinate, out var tile);
                tileMove.transform.position = tile.transform.position;
                tileMove.Coordinate = newCoordinate;
            }
            movableBoxData.rootCoordinate += offset;
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
    }
}
