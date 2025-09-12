using Geckout.Data;
using TMPro;
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
            m_textCount.text = Data.difusionCount.ToString();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            m_textCount.transform.localPosition = Vector3.forward * -0.4f;
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
