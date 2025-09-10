using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BoxMove
    {
        [HideInInspector]
        [Header("Tool")]
        [SerializeField] private Outline m_outlineSelected;
        private Outline OutlineSelected
        {
            get
            {
                if (m_outlineSelected == null)
                {
                    m_outlineSelected = gameObject.AddComponent<Outline>();
                    m_outlineSelected.OutlineColor = Color.red;
                    m_outlineSelected.OutlineWidth = 8;

                }
                //m_outlineSelected.gameObject.SetActive(true);
                return m_outlineSelected;
            }
        }

        #region Tool

        public void SetSelected(bool selected)
        {
            OutlineSelected.enabled = selected;
        }

        public void MoveByOffset(Vector2Int offset)
        {
            foreach (var tileMove in listMovableBoxTile)
            {
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var oldTile);
                //oldTile.IsOccupied = false;
            }

            foreach (var tileMove in listMovableBoxTile)
            {
                var newCoordinate = new Vector2Int(tileMove.Coordinate.x + offset.x, tileMove.Coordinate.y + offset.y);
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var oldTile);
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(newCoordinate, out var tile);
                //oldTile.IsOccupied = false;
                tileMove.transform.position = tile.transform.position;
                tileMove.Coordinate = newCoordinate;
            }
            MovableBoxData.rootCoordinate += offset;
        }

        public void PlaceMoveBox()
        {
            foreach (var tileMove in listMovableBoxTile)
            {
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var tile);
                if (tile.IsOccupied)
                {
                    Debug.LogError("Cannot place tile");
                    return;
                }

            }

            foreach (var tileMove in listMovableBoxTile)
            {
                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var tile);
                tile.IsOccupied = true;
            }

            Debug.Log("Move Success");
            LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Add(MovableBoxData);
        }

        

        #endregion
    }
}
