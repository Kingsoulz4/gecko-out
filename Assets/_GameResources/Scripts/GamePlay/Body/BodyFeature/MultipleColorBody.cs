using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class MultipleColorBody : MechanicBody
    {
        [SerializeField] private SubColorIndicator m_subColorIndicatorPrefab;
        private bool isInit = false;
        private List<Vector2Int> lastPath = new();
        private List<ColorType> colorTypes = new();
        public void Init()
        {
            if (isInit) return;
            isInit = true;
            SpawnSubColorIndicators();
            colorTypes = new List<ColorType>(body.BodyData.listColor);
            LevelEvent.OnGetLastPath += OnGetLastPath;
        }

        private void OnGetLastPath(BodyController controller, List<Vector2Int> list)
        {
            if (!controller || controller != body) return;

            SetLastPath(list);
            SpawnNewBody();
        }

        private void SpawnNewBody()
        {
            colorTypes.RemoveAt(0);
            var bodyData = new BodyData();
            bodyData.listColor = new List<ColorType>(colorTypes);
            bodyData.listCoordinate = new List<Vector2Int>(lastPath);

            LevelManager.Instance.LevelGame.SpawnBody(bodyData);
        }

        private void SetLastPath(List<Vector2Int> listPos)
        {
            this.lastPath = new List<Vector2Int>(listPos);
        }

        private void SpawnSubColorIndicators()
        {
            var newSubColorIndicator = Instantiate(m_subColorIndicatorPrefab, transform);
            newSubColorIndicator.Init(body.SplineComputer, body.BodyData.listColor);
        }

        private void OnDisable()
        {
            isInit = false;
            LevelEvent.OnGetLastPath -= OnGetLastPath;
        }
    }
}
