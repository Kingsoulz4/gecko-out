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
            if (ShouldSetOccupied() && selected) SetTilesOccupiedMap(false);
        }
        public virtual void MoveByOffset(Vector2Int offset)
        {
            var mapSize = LevelManager.Instance.LevelGame.GameLevelData.mapSize;
            var boxSize = Data.boxSize;
            var newRootConstraintR = Data.rootCoordinate + boxSize + offset;
            var newRootConstraintL = Data.rootCoordinate + offset;
            if(newRootConstraintL.x <0 || newRootConstraintR.x > mapSize.x || newRootConstraintL.y < 0 || newRootConstraintR.y > mapSize.y)
            {
                return;
            }

            //if(CheckCanPlace())
            //{
            //    SetTilesOccupiedMap(false);
            //}

            foreach (var tileMove in spawnedTiles)
            {
                var newCoordinate = new Vector2Int(tileMove.Coordinate.x + offset.x, tileMove.Coordinate.y + offset.y);
                GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var oldTile);
                GameMap.TryGetTileAtCoord(newCoordinate, out var tile);
                tileMove.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tileMove.transform.position.z);
                tileMove.Coordinate = newCoordinate;
            }
            Data.rootCoordinate += offset;

            //if(CheckCanPlace())
            //{
            //    SetTilesOccupiedMap(true);
            //}

            UpdateVisual();
        }

        public bool CheckCanPlace()
        {
            foreach (var tileMove in spawnedTiles)
            {
                GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var tile);
                if (tile.IsOccupied)
                {
                    Debug.LogError("Cannot place tile");
                    return false;
                }

            }
            return true;
        }

        public void SetTilesOccupiedMap(bool isOccupied)
        {
            foreach (var tileMove in spawnedTiles)
            {
                GameMap.TryGetTileAtCoord(tileMove.Coordinate, out var tile);
                tile.IsOccupied = isOccupied;
            }
        }

        public virtual bool PlaceMoveBox()
        {
            if (!CheckCanPlace()) return false;

            SetTilesOccupiedMap(true);

            return true;
        }



        #endregion
    }
}
