using Geckout.Data;
using UnityEngine;

namespace Geckout
{
    public partial class Crate : BoxBase<CrateTile, CrateData>
    {
        protected override Vector2Int RootCoordinate => Data.rootCoordinate;
        protected override Vector2Int BoxSize => Data.boxSize;

        protected override bool ShouldSetOccupied()
        {
            return LevelManager.Instance.LevelGame.GameLevelData.listCrateData.Contains(Data);
        }

        protected override void AddTileComponent(GameObject obj, Vector2Int coord)
        {
            var tile = obj.AddComponent<CrateTile>();
            tile.Coordinate = coord;
            spawnedTiles.Add(tile);
        }
    }
}
