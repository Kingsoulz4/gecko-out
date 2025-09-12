using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private List<BodyPartColorChanger> bodyPartColorChangers = new();
        public PortalData PortalData { get; private set; }
        [SerializeField] private MechanicsReferences m_mechanicReferences;

        private List<MechanicRendererBase> listMechanicRender = new();
        bool isMovingToPortal = false;

        private void Awake()
        {
            //bodyPartColorChangers = GetComponentsInChildren<BodyPartColorChanger>().ToList();
        }

        private void Start()
        {
            UpdateVisual();
        }

        public void Initialize(PortalData portalData)
        {
            PortalData = portalData;
            InitVisual();
            isMovingToPortal = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Segment>(out var segment))
            {
                var bodyController = segment.Controller;
                if (bodyController != null && bodyController.MoveToPortal != null)
                {
                    if (isMovingToPortal) return;
                    GameMap.TryGetTileAtCoord(PortalData.Coordinate, out GameTile tile);
                    if (tile != null)
                    {
                        tile.SetOccupied(false);
                    }
                    bodyController.MoveToPortal.InitiatePortalMovement(this);
                    isMovingToPortal = true;
                }
            }
        }

        public void UpdateVisual()
        {
            bodyPartColorChangers.ForEach(x => x.UpdateColor(PortalData.listColor.First()));
        }

        internal void Disappear()
        {
            this.gameObject.SetActive(false);
        }

        public void InitVisual()
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
                var prefabRenderFreeze = m_mechanicReferences.listMechanicRenderer[MechanicNames.Freeze];
                var newIceRenderer = (IceRenderer)Instantiate(prefabRenderFreeze, transform);
                newIceRenderer.GenerateIces(new List<Vector3>() { transform.position }, PortalData.freezeTimeCount);
                listMechanicRender.Add(newIceRenderer);
            }


        }
    }
}
