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

        public void FadeColor(ColorType targetColor, float duration)
        {
            var mat = GetMaterialByColorType(targetColor);
            StartCoroutine(FadeToColor(mat.color, duration));
        }

        private Material GetMaterialByColorType(ColorType colorType)
        {
            return m_listMaterial[colorType];
        }

        private IEnumerator FadeToColor(Color targetColor, float duration)
        {
            Color startColor = m_meshRenderer.material.color;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                t = Mathf.SmoothStep(0f, 1f, t);

                Color currentColor = Color.Lerp(startColor, targetColor, t);
                m_meshRenderer.material.color = currentColor;

                yield return null;
            }

            m_meshRenderer.material.color = targetColor;
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
