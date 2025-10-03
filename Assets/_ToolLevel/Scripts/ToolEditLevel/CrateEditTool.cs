using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class Crate 
    {
        #region Tool

        public override void SetSelected(bool selected)
        {
            base.SetSelected(selected);
        }

        public override void MoveByOffset(Vector2Int offset)
        {
            base.MoveByOffset(offset);
        }

        public override bool PlaceMoveBox()
        {
            if(!base.PlaceMoveBox()) return false;
            Debug.Log("Move Success");
            if (!LevelManager.Instance.LevelGame.GameLevelData.listCrateData.Contains(Data))
            {
                LevelManager.Instance.LevelGame.GameLevelData.listCrateData.Add(Data);
            }
            return true;
        }

        public override bool CheckCanPlace()
        {
            foreach (var tileMove in spawnedTiles)
            {
                //GameMap.TryGetCrateAtCoord(tileMove.Coordinate, out var crate);
                if (GameMap.TryGetCratesAtCoord(tileMove.Coordinate, out var crates) && crates.Count > 1)
                {
                    Debug.LogError("Cannot place tile");
                    return false;
                }

            }
            return true;
        }

        #endregion
    }
}
