using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Geckout
{
    public class IcePortal : MonoBehaviour
    {
        [SerializeField] private Portal portal;
        private int currentCount = 0;
        private IceRenderer iceRenderer;
        private bool isInit = false;

        public void Init(int meltCount)
        {
            if (isInit) return;
            currentCount = meltCount;
            var prefabRenderFreeze = portal.MechanicReferences.listMechanicRenderer[MechanicNames.Freeze];
            iceRenderer = (IceRenderer)Instantiate(prefabRenderFreeze, transform);
            iceRenderer.GenerateIces(new List<Vector3>() { transform.position }, portal.PortalData.freezeTimeCount);
            portal.ListMechanicRender.Add(iceRenderer);
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
