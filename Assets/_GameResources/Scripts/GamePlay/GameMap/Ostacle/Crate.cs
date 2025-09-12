using Geckout.Data;
using UnityEngine;
using TMPro;
namespace Geckout
{
    public partial class Crate : BoxBase<CrateTile, CrateData>
    {

        [SerializeField] private TextMeshPro m_textCount;
        protected override Vector2Int RootCoordinate => Data.rootCoordinate;
        protected override Vector2Int BoxSize => Data.boxSize;

        public override void Init(CrateData data)
        {
            base.Init(data);

        }




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
