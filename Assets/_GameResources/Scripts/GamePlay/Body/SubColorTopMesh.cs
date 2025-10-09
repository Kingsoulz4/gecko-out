using Dreamteck.Splines;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class SubColorTopMesh : MonoBehaviour
    {
        [SerializeField] private SplineMesh m_splineMesh;
        [SerializeField] private SplineFollower m_splineFollowEnd;
        [SerializeField] private float m_clipFrom = 0.138f;
        [SerializeField] private float m_clipTo = 0.7f;
        [SerializeField] private float m_clipToOffet = 0.1f;
        [SerializeField] private float m_spacing = -0.02f;
        [SerializeField] private MeshRenderer m_mainMesh;
        [SerializeField] private MeshRenderer m_endMesh;
        [SerializeField] private ListMaterialByColor m_listMaterial;

        public void Init(SplineComputer splineComputer, ColorType colorType, int index)
        {
            m_mainMesh.material = m_listMaterial.listMaterial[colorType];
            m_endMesh.material = m_listMaterial.listMaterial[colorType];
            m_splineMesh.spline = splineComputer;
            m_splineFollowEnd.spline = splineComputer;
            var channel = m_splineMesh.GetChannel(0);
            //m_splineMesh.ad
            channel.clipFrom = m_clipFrom;
            channel.clipTo = m_clipTo - m_clipToOffet * index;
            channel.spacing = m_spacing;
            channel.minOffset = new Vector2(0, -0.3f - index * 0.01f);
            m_splineFollowEnd.clipTo = channel.clipTo;
        }
    }
}
