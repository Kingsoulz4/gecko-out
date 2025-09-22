using Dreamteck.Splines;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class SubColorIndicator : MonoBehaviour
    {
        [SerializeField] private SubColorTopMesh m_splineMeshPrefab;


        public void Init(SplineComputer splineComputer, List<ColorType> listColor)
        {
            var listInverse = new List<ColorType>(listColor);
            listInverse.Reverse();
            for(int i=0; i< listInverse.Count-1; i++)
            {
                var subColorTop = Instantiate(m_splineMeshPrefab, transform);
                subColorTop.gameObject.SetActive(true);
                subColorTop.Init(splineComputer, listInverse[i], i);
            }
        }
        
    }
}
