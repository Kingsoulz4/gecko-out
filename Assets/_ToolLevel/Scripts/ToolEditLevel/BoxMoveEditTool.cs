using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BoxMove
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

        public override void PlaceMoveBox()
        {
            base.PlaceMoveBox();
            Debug.Log("Move Success");
            LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Add(Data);
        }

        #endregion
    }
}
