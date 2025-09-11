using Geckout.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    //public abstract class BoxBase: MonoBehaviour
    //{
    //    public abstract void SetSelected(bool selected);
    //}

    public partial class BoxBase<TTile, TData> : MonoBehaviour /*BoxBase*/
        where TTile : BoxBaseTile
        where TData : BoxBaseData
    {
        [SerializeField] protected GameObject m_tileCornerPrefab;
        [SerializeField] protected GameObject m_tileEdgePrefab;
        [SerializeField] protected GameObject m_tileCenterPrefab;
        [SerializeField] protected GameObject m_tileCorner3EdgePrefab;
        [SerializeField] protected GameObject m_tile2EdgePrefab;
        [SerializeField] protected GameObject m_tile4EdgePrefab;

        protected readonly List<TTile> spawnedTiles = new();

        public TData Data { get; private set; }

        public virtual void Init(TData data)
        {
            Data = data;
            SpawnTiles();
            UpdateVisual();
        }

        protected virtual Vector2Int RootCoordinate { get; }
        protected virtual Vector2Int BoxSize { get; }
        protected virtual bool ShouldSetOccupied() { return false; }
        protected virtual void AddTileComponent(GameObject obj, Vector2Int coord) { }

        public virtual void UpdateVisual()
        {
            SetCenterPos();
        }

        protected void SetCenterPos()
        {
            SetPivotAndPosition(GetCenterWorldPos());
        }

        protected Vector3 GetCenterWorldPos()
        {
            var root = RootCoordinate;
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(root, out var tileLeft);

            var rightCoord = new Vector2Int(root.x + BoxSize.x - 1, root.y);
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(rightCoord, out var tileRight);

            var topCoord = new Vector2Int(root.x, root.y + BoxSize.y - 1);
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(topCoord, out var tileTop);

            return new Vector3(
                (tileLeft.transform.position.x + tileRight.transform.position.x) / 2,
                (tileLeft.transform.position.y + tileTop.transform.position.y) / 2,
                tileLeft.transform.position.z
            );
        }

        protected void SetPivotAndPosition(Vector3 newPivotWorldPos)
        {
            Vector3 pivotDelta = transform.position - newPivotWorldPos;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.position += pivotDelta;
            }

            transform.position = newPivotWorldPos;
        }

        protected void SpawnTiles()
        {
            bool needSetOccupied = false;
            if (ShouldSetOccupied()) needSetOccupied = true;

            if (BoxSize.x == 1 && BoxSize.y == 1)
            {
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(RootCoordinate.x, RootCoordinate.y), out var tile);

                GameObject obj;

                obj = Instantiate(m_tile4EdgePrefab, transform);

                obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                obj.transform.localScale = Vector3.one * 1;
                obj.name = $"Tile";
                obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
                AddTileComponent(obj, tile.MapTileData.coordinate);
                if (needSetOccupied) tile.IsOccupied = true;
            }
            else if (BoxSize.x == 1)
            {
                for (int x = 0; x < BoxSize.x; x++)
                {
                    for (int y = 0; y < BoxSize.y; y++)
                    {
                        Vector2Int c = new Vector2Int(x, y);
                        var prefab = m_tile2EdgePrefab;
                        if (y == 0 || y == BoxSize.y - 1)
                        {
                            prefab = m_tileCorner3EdgePrefab;
                        }

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(RootCoordinate.x + c.x, RootCoordinate.y + c.y), out var tile);
                        GameObject obj;

                        obj = Instantiate(prefab, transform);

                        obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        //obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
                        if (y == 0)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(180, 90, -90));
                        }
                        else if (y == BoxSize.y - 1)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(180, -90, 90));
                        }
                        else
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));
                        }

                        AddTileComponent(obj, tile.MapTileData.coordinate);
                        if (needSetOccupied) tile.IsOccupied = true;
                    }
                }
            }
            else if (BoxSize.y == 1)
            {

                for (int x = 0; x < BoxSize.x; x++)
                {
                    for (int y = 0; y < BoxSize.y; y++)
                    {
                        Vector2Int c = new Vector2Int(x, y);
                        var prefab = m_tile2EdgePrefab;
                        if (x == 0 || x == BoxSize.x - 1)
                        {
                            prefab = m_tileCorner3EdgePrefab;
                        }

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(RootCoordinate.x + c.x, RootCoordinate.y + c.y), out var tile);

                        GameObject obj;

                        obj = Instantiate(prefab, transform);

                        obj.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, obj.transform.position.z);
                        obj.transform.localScale = Vector3.one * 1;
                        obj.name = $"Tile_{c.x}_{c.y}";
                        if (x == 0)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, 90, -90));
                        }
                        else if (x == BoxSize.x - 1)
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, -90, 90));
                        }
                        else
                        {
                            obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, 90, -90));
                        }

                        AddTileComponent(obj, tile.MapTileData.coordinate);
                        if (needSetOccupied) tile.IsOccupied = true;
                    }
                }
            }
            else
            {
                for (int x = 0; x < BoxSize.x; x++)
                {
                    for (int y = 0; y < BoxSize.y; y++)
                    {
                        Vector2Int c = new Vector2Int(x, y);

                        LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(new Vector2Int(RootCoordinate.x + c.x, RootCoordinate.y + c.y), out var tile);

                        GameObject obj = null;

                        if (x == 0 && y == 0)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerBottomLeft = new Vector3(0, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerBottomLeft);
                        }
                        else if (x == 0 && y == BoxSize.y - 1)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerTopLeft = new Vector3(90, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerTopLeft);
                        }
                        else if (x == BoxSize.x - 1 && y == 0)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerBottomRight = new Vector3(-90, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerBottomRight);
                        }
                        else if (x == BoxSize.x - 1 && y == BoxSize.y - 1)
                        {
                            obj = Instantiate(m_tileCornerPrefab, transform);
                            var angleCornerTopRight = new Vector3(0, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleCornerTopRight);
                        }
                        else if (x == 0 && (y != 0 && y != BoxSize.y - 1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleLeftEdge = new Vector3(90, 90, -90);
                            obj.transform.localRotation = Quaternion.Euler(angleLeftEdge);
                        }
                        else if (x == BoxSize.x - 1 && (y != 0 && y != BoxSize.y - 1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleRightEdge = new Vector3(-90, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleRightEdge);
                        }
                        else if (y == 0 && (x != 0 && x != BoxSize.x - 1))
                        {
                            obj = Instantiate(m_tileEdgePrefab, transform);
                            var angleBottomEdge = new Vector3(-180, -90, 90);
                            obj.transform.localRotation = Quaternion.Euler(angleBottomEdge);
                        }
                        else if (y == BoxSize.y - 1 && (x != 0 && x != BoxSize.x - 1))
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

                        AddTileComponent(obj, tile.MapTileData.coordinate);

                        if (needSetOccupied) tile.IsOccupied = true;
                    }
                }
            }
        }
    }
}
