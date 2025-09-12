using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class IceBody : MonoBehaviour
    {
        [SerializeField] private BodyController body;
        private int currentCount = 0;
        private IceRenderer iceRenderer;
        private bool isInit = false;

        public int CurrentCount { get => currentCount;}

        public void Init(int meltCount)
        {
            if (isInit) return;
            currentCount = meltCount;
            var prefabRenderFreeze = body.MechanicReferences.listMechanicRenderer[MechanicNames.Freeze];
            iceRenderer = (IceRenderer)Instantiate(prefabRenderFreeze, transform);
            var listPos = body.BodyData.listCoordinate.Select(x =>
            {
                GameMap.TryGetTileAtCoord(x, out var tile);
                return tile.transform.position;
            }).ToList();
            iceRenderer.GenerateIces(listPos, body.BodyData.freezeTimeCount);
            body.ListMechanicRender.Add(iceRenderer);
            isInit = true;

            LevelEvent.OnMoveToPortalDone -= OnBodyMoveToPortalDone;
            LevelEvent.OnMoveToPortalDone += OnBodyMoveToPortalDone;
        }

        private void OnDisable()
        {
            LevelEvent.OnMoveToPortalDone -= OnBodyMoveToPortalDone;
        }

        private void OnBodyMoveToPortalDone(BodyController controller, Portal portal)
        {
            if (currentCount < 0)
            {
                return;
            }

            currentCount -= 1;
            iceRenderer.UpdateMeltCount(currentCount);
            if (currentCount == 0)
            {
                Break();
            }
        }

        private void Break()
        {
            iceRenderer.Break();
            //vfx
        }
    }
}
