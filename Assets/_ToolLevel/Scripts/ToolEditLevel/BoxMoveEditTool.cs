using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public override bool PlaceMoveBox()
        {
            if (!base.PlaceMoveBox()) return false;
            Debug.Log("Move Success");
            if (!LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Contains(Data))
            {
                LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Add(base.Data);
            }
            return true;
        }

        #endregion
    }
}
