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

        public int CurrentCount { get => currentCount; }

        public void Init(int meltCount)
        {
            if (isInit) return;

            currentCount = meltCount;

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
            if (currentCount == 0)
            {
                Break();
            }
        }

        private void Break()
        {

            for (int i = 1; i < body.Segments.Count - 1; i++)
            {
                if (i % 3 == 0)
                {

                }
            }
            //vfx
        }
    }
}
