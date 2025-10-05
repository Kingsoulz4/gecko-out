using Dreamteck.Splines;
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
        [SerializeField] private SplineFollower m_splineFollower;

        private List<BodyPartColorChanger> listBodyChanger = new();
        private Material instanceMat;

        public void Init(List<BodyPartColorChanger> listBodyChanger, int count, SplineComputer splineComputer)
        {
            this.listBodyChanger = listBodyChanger;
            m_splineFollower.spline = splineComputer;
            instanceMat = new Material(m_hiddenMat);
            foreach (var bodyPart in listBodyChanger)
            {
                bodyPart.UpdateColor(instanceMat);
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
    }
}
