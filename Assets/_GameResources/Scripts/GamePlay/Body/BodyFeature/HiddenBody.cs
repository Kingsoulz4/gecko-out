using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class HiddenBody : MechanicBody
    {
        private int currentCount = 0;
        private bool isInit = false;
        private HiddenRenderer hiddenRenderer;
        public int CurrentCount { get => currentCount; }

        public void Init(int count)
        {
            if (isInit) return;

            currentCount = count;

            isInit = true;

            var prefabRenderFreeze = body.MechanicReferences.listMechanicRenderer[MechanicNames.Hidden];
            hiddenRenderer = (HiddenRenderer)Instantiate(prefabRenderFreeze, body.BodyRenderer.Segments.Last().transform);
            hiddenRenderer.Init(body.BodyRenderer.ListBodyPartChanger, count);

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
            hiddenRenderer.UpdateText(currentCount);
            if (currentCount == 0)
            {
                Break();
            }
        }

        private void Break()
        {
            body.BodyRenderer.UpdateBodyColor();
            for (int i = 1; i < body.Segments.Count - 1; i++)
            {
                if (i % 3 == 0)
                {
                    //vfx
                }
            }
            hiddenRenderer.Break();
        }
    }
}