using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BoxBase<TTile, TData> 
    {
        [HideInInspector]
        [Header("Tool")]
        [SerializeField] protected Outline m_outlineSelected;
        protected Outline OutlineSelected
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

        public virtual void SetSelected(bool selected)
        {
            OutlineSelected.enabled = selected;
        }

        public virtual void MoveByOffset(Vector2Int offset)
        {
            foreach (var tileMove in spawnedTiles)
            {
                var newCoordinate = new Vector2Int(tileMove.Coordinate.x + offset.x, tileMove.Coordinate.y + offset.y);
                GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var oldTile);
                GameMap.TryGetTileAtCoord(newCoordinate, out var tile);
                tileMove.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tileMove.transform.position.z);
                tileMove.Coordinate = newCoordinate;
            }
            Data.rootCoordinate += offset;
            UpdateVisual();
        }

        public virtual void PlaceMoveBox()
        {
            foreach (var tileMove in spawnedTiles)
            {
                GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var tile);
                if (tile.IsOccupied)
                {
                    Debug.LogError("Cannot place tile");
                    return;
                }

            }

            foreach (var tileMove in spawnedTiles)
            {
                GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var tile);
                tile.IsOccupied = true;
            }

            
        }



        #endregion
    }
}
