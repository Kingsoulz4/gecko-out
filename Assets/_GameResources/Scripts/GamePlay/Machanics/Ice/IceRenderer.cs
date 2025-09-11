using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class IceRenderer : MechanicRendererBase
    {
        [SerializeField] private IceTile m_icePrefab;

        private List<IceTile> listIceTile= new();

        public void GenerateIces(List<Vector3> listPosition)
        {
            for (int i = 0; i < listPosition.Count; i++)
            {
                var newIceTile = Instantiate(m_icePrefab, transform);
                newIceTile.transform.position = listPosition[i];
                newIceTile.gameObject.SetActive(true);
                listIceTile.Add(newIceTile);
            }
        }
    }
}
