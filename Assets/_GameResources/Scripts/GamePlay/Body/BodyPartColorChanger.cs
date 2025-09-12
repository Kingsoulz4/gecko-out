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
        [SerializeField] private SerializedDictionary<ColorType, Material> m_listMaterial;


        public void UpdateColor(ColorType colorType)
        {
            m_meshRenderer.material = m_listMaterial[colorType];
        }

        public void UpdateColor(Material material)
        {
            m_meshRenderer.material = material;
        }

        private void OnValidate()
        {
            m_meshRenderer = GetComponent<Renderer>();   
        }

    }
}
