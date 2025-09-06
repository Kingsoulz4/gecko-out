using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private List<BodyPartColorChanger> bodyPartColorChangers = new();
        
        public PortalData PortalData { get; set; }

        private void Awake()
        {
            bodyPartColorChangers = GetComponentsInChildren<BodyPartColorChanger>().ToList();
        }

        private void Start()
        {
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            bodyPartColorChangers.ForEach(x => x.UpdateColor(PortalData.listColor.First()));
        }
    }
}
