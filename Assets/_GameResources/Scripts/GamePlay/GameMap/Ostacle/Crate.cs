using Geckout.Data;
using TMPro;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
namespace Geckout
{
    public partial class Crate : BoxBase<CrateTile, CrateData>
    {
        [SerializeField] private TextMeshPro m_textCount;
        private int currentCount = 0;
        protected override Vector2Int RootCoordinate => Data.rootCoordinate;
        protected override Vector2Int BoxSize => Data.boxSize;

        public override void Init(CrateData data)
        {
            base.Init(data);
            currentCount = data.difusionCount;
            m_textCount.text = Data.difusionCount.ToString();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            m_textCount.transform.position = new Vector3(GetCenterWorldPos().x, GetCenterWorldPos().y, m_textCount.transform.position.z);
            m_textCount.text = currentCount.ToString();
        }

        private void OnEnable()
        {
            LevelEvent.OnMoveToPortalDone += OnBodyMoveToPortalDone;
        }

        private void OnDisable()
        {
            LevelEvent.OnMoveToPortalDone -= OnBodyMoveToPortalDone;
        }

        private void OnBodyMoveToPortalDone(BodyController controller, Portal portal)
        {
            if (currentCount <= 0)
            {
                return;
            }

            currentCount -= 1;
            if (currentCount == 0)
            {
                Break();
            }
            m_textCount.text = currentCount.ToString();
        }

        private void Break()
        {
            GameMap.SetTilesUnoccupied(spawnedTiles.Select(tile => tile.Coordinate).ToList());
            LevelManager.Instance.LevelGame.UpdateAllBodyState();
            gameObject.SetActive(false);

            //vfx
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
