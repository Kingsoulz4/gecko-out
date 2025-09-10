using Geckout.Data;
using UnityEngine;

namespace Geckout
{
    public partial class BoxMove : BoxBase<MovableBoxTile, MovableBoxData>
    {
        [SerializeField] private GameObject m_arrowHorizontal;
        [SerializeField] private GameObject m_arrowVertical;

        protected override Vector2Int RootCoordinate => Data.rootCoordinate;
        protected override Vector2Int BoxSize => Data.boxSize;

        protected override bool ShouldSetOccupied()
        {
            return LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Contains(Data);
        }

        protected override void AddTileComponent(GameObject obj, Vector2Int coord)
        {
            var tile = obj.AddComponent<MovableBoxTile>();
            tile.Coordinate = coord;
            spawnedTiles.Add(tile);
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            m_arrowHorizontal.transform.localPosition = Vector3.forward * -0.4f;
            m_arrowVertical.transform.localPosition = Vector3.forward * -0.4f;
            m_arrowHorizontal.SetActive(Data.wayDirection == WayDirection.Horizontal || Data.wayDirection == WayDirection.All);
            m_arrowVertical.SetActive(Data.wayDirection == WayDirection.Vertical || Data.wayDirection == WayDirection.All);
        }
    }
}
