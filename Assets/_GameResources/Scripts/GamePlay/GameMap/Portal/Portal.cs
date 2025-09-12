using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.HableCurve;

namespace Geckout
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private List<BodyPartColorChanger> bodyPartColorChangers = new();
        [SerializeField] private MechanicsReferences m_mechanicReferences;
        [SerializeField] private IcePortal icePortal;

        private List<MechanicRendererBase> listMechanicRender = new();
        private bool isMovingToPortal = false;
        private bool isEnablePortal = true;
        public PortalData PortalData { get; private set; }
        public MechanicsReferences MechanicReferences { get => m_mechanicReferences; }
        public List<MechanicRendererBase> ListMechanicRender { get => listMechanicRender; }
        public bool IsEnablePortal
        {
            get
            {
                return isEnablePortal && icePortal.CurrentCount <= 0;
            }
        }

        public void Initialize(PortalData portalData)
        {
            isMovingToPortal = false;
            PortalData = portalData;
            InitMechanic();
            UpdateVisual();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Segment>(out var segment))
            {
                MoveToPortal(segment);
            }
        }

        protected virtual void MoveToPortal(Segment segment)
        {
            var bodyController = segment.Controller;
            if (bodyController != null && bodyController.MoveToPortal != null
                && bodyController.CanMovePortal && IsEnablePortal
                && bodyController.BodyData.listColor[0] == PortalData.listColor[0])
            {
                if (isMovingToPortal) return;

                GameMap.TryGetTileAtCoord(PortalData.Coordinate, out GameTile tile);
                if (tile != null)
                {
                    tile.SetOccupied(false);
                }
                bodyController.MoveToPortal.InitiatePortalMovement(this);
                LevelEvent.OnMoveToPortalStart?.Invoke(segment.Controller, this);
                isMovingToPortal = true;

                this.Wait(Time.deltaTime * 8, () =>
                {
                    GetLastPath(bodyController);
                });
            }
        }

        public void GetLastPath(BodyController bodyController)
        {
            var list = bodyController.OccupiedTileController.LastGridPositions.ToList();
            var lastpath = new List<Vector2Int>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 3 != 0 && i != 0 && i != list.Count - 1)
                {
                    continue;
                }
                lastpath.Add(list[i]);
            }
            LevelEvent.OnGetLastPath?.Invoke(bodyController, lastpath);
        }

        public virtual void UpdateVisual()
        {
            bodyPartColorChangers.ForEach(x => x.UpdateColor(PortalData.listColor.First()));
        }

        internal void Disappear()
        {
            this.gameObject.SetActive(false);
        }

        public void InitMechanic()
        {
            if (listMechanicRender.Count > 0)
            {
                foreach (var item in listMechanicRender)
                {
                    Destroy(item.gameObject);
                }
                listMechanicRender.Clear();
            }

            if (PortalData.freezeTimeCount > 0)
            {
                icePortal.Init(PortalData.freezeTimeCount);
            }
        }
    }
}
