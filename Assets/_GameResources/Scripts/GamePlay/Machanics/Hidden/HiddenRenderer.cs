using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Geckout
{
    public class HiddenRenderer : MechanicRendererBase
    {
        [SerializeField] private Material m_hiddenMat;
        [SerializeField] private TextMeshPro m_textCount;

        private List<BodyPartColorChanger> listBodyChanger = new();

        public void Init(List<BodyPartColorChanger> listBodyChanger, int count)
        {
            this.listBodyChanger = listBodyChanger;
            foreach(var bodyPart in listBodyChanger)
            {
                bodyPart.UpdateColor(m_hiddenMat);
            }
            UpdateText(count);
        }

        public void UpdateText(int count)
        {
            m_textCount.text = count.ToString();
        }
        public void Break()
        {
            gameObject.SetActive(false);
        }

        //public void ChangeColor()
        //{
        //    body.BodyRenderer.UpdateBodyColor();

        //}
    }
}
