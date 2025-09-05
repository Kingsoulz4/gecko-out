using AYellowpaper.SerializedCollections;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class BodyPartColorChanger : MonoBehaviour
    {
        [SerializeField] private Renderer m_meshRenderer;
        [SerializeField] private SerializedDictionary<ColorList, Material> m_listMaterial;


        public void UpdateColor(ColorList colorType)
        {
            m_meshRenderer.material = m_listMaterial[colorType];
        }

        private void OnValidate()
        {
            if(m_meshRenderer == null)
            {
                m_meshRenderer = GetComponent<Renderer>();
            }
        }

    }
}
