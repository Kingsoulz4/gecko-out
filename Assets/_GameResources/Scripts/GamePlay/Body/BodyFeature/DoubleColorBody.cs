using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class DoubleColorBody : MechanicBody
    {
        private IceRenderer doubleColorRenderer;
        private bool isInit = false;


        public void Init(ColorType colorType)
        {
            if (isInit) return;
            isInit = true;

            var prefabDoubleColorRender = body.MechanicReferences.listMechanicRenderer[MechanicNames.DoubleColor];
            doubleColorRenderer = (IceRenderer)Instantiate(prefabDoubleColorRender, transform);
            //hiddenRenderer.GenerateIces(listPos, body.BodyData.freezeTimeCount);
            body.ListMechanicRender.Add(doubleColorRenderer);

            LevelEvent.OnMoveToPortalStart -= OnBodyMoveToPortalStart;
            LevelEvent.OnMoveToPortalStart += OnBodyMoveToPortalStart;
        }

        private void OnDisable()
        {
            LevelEvent.OnMoveToPortalStart -= OnBodyMoveToPortalStart;
        }

        private void OnBodyMoveToPortalStart(BodyController controller, Portal portal)
        {
            Break();
        }

        private void Break()
        {

        }
    }
}
