using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private List<BodyPartColorChanger> bodyPartColorChangers = new();
        [SerializeField] private MechanicsReferences m_mechanicReferences;

        public PortalData PortalData { get; set; }

        private List<MechanicRendererBase> listMechanicRender = new();


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
        }

        public void UpdateVisual()
        {
            bodyPartColorChangers.ForEach(x => x.UpdateColor(PortalData.listColor.First()));
        }

        internal void Disappear()
        {
            GetComponent<GameTile>().ChangeVisualToNormalTile();
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
                newIceRenderer.GenerateIces(new List<Vector3>() { transform.position });
                listMechanicRender.Add(newIceRenderer);
            }


        }
    }
}
