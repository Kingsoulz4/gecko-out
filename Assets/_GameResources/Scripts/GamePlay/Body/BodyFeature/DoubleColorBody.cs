using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class DoubleColorBody : MechanicBody
    {
        private IceRenderer doubleColorRenderer;
        private bool isInit = false;
        private List<Vector2Int> lastPath = new();

        public void Init(ColorType colorType)
        {
            if (isInit) return;
            isInit = true;

            var prefabDoubleColorRender = body.MechanicReferences.listMechanicRenderer[MechanicNames.DoubleColor];
            doubleColorRenderer = (IceRenderer)Instantiate(prefabDoubleColorRender, transform);
            body.ListMechanicRender.Add(doubleColorRenderer);

            LevelEvent.OnMoveToPortalStart -= OnBodyMoveToPortalStart;
            LevelEvent.OnMoveToPortalStart += OnBodyMoveToPortalStart;
            LevelEvent.OnGetLastPath += OnGetLastPath;
        }

        private void OnGetLastPath(BodyController controller, List<Vector2Int> list)
        {
            if (controller != body) return;
            SetLastPath(list);
            SpawnNewBody();
        }

        private void SpawnNewBody()
        {
            var bodyData = new BodyData();
            bodyData.listColor = new List<ColorType>() { body.BodyData.doubleColor };
            bodyData.listCoordinate = new List<Vector2Int>(lastPath);

            LevelManager.Instance.LevelGame.SpawnBody(bodyData);
        }

        private void SetLastPath(List<Vector2Int> listPos)
        {
            this.lastPath = new List<Vector2Int>(listPos);
        }

        private void OnDisable()
        {
            isInit = false;
            LevelEvent.OnMoveToPortalStart -= OnBodyMoveToPortalStart;
            LevelEvent.OnGetLastPath -= OnGetLastPath;
        }

        private void OnBodyMoveToPortalStart(BodyController controller, Portal portal)
        {

        }
    }
}
