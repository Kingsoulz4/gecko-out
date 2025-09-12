using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Geckout
{
    public class IceRenderer : MechanicRendererBase
    {
        [SerializeField] private TextMeshPro m_textMeltCount;
        [SerializeField] private IceTile m_icePrefab;

        private List<IceTile> listIceTile= new();

        public void GenerateIces(List<Vector3> listPosition, int meltCount)
        {
            for (int i = 0; i < listPosition.Count; i++)
            {
                var newIceTile = Instantiate(m_icePrefab, transform);
                newIceTile.transform.position = listPosition[i];
                newIceTile.gameObject.SetActive(true);
                listIceTile.Add(newIceTile);
            }
            m_textMeltCount.text = meltCount + "";
            var midTile = listIceTile[listIceTile.Count / 2];
            m_textMeltCount.transform.position = new Vector3(midTile.transform.position.x, midTile.transform.position.y, -2);
        }

        public void UpdateMeltCount(int meltCount)
        {
            m_textMeltCount.text = meltCount.ToString();
        }

        public void Break()
        {
            gameObject.SetActive(false);
        }
    }
}
