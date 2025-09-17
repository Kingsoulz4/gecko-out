using Dreamteck.Splines;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class SubColorIndicator : MonoBehaviour
    {
        [SerializeField] private SplineMesh m_splineMeshPrefab;
        [SerializeField] private ListMaterialByColor m_listMaterial;
        [SerializeField] private float m_clipFrom = 0.138f;
        [SerializeField] private float m_clipTo = 0.7f;
        [SerializeField] private float m_clipToOffet = 0.08f;


        public void Init(SplineComputer splineComputer, List<ColorType> listColor)
        {
            for(int i=1; i<listColor.Count; i++)
            {
                var newSplineMesh = Instantiate(m_splineMeshPrefab, transform);
                newSplineMesh.gameObject.SetActive(true);
                newSplineMesh.GetComponent<MeshRenderer>().material = m_listMaterial.listMaterial[listColor[i]];
                newSplineMesh.spline = splineComputer;
                var channel = newSplineMesh.GetChannel(0);
                channel.clipFrom = m_clipFrom;
                channel.clipTo = m_clipTo + m_clipToOffet * i;
                channel.minOffset = new Vector2(0, -0.3f - (listColor.Count -i) * 0.01f);
            }
        }
        
    }
}
