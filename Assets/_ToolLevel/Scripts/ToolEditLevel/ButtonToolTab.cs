using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class ButtonToolTab : MonoBehaviour
    {
        [SerializeField] private GameObject m_selectedIndicator;


        public void SetSelected(bool isSelected)
        {
            m_selectedIndicator.SetActive(isSelected);
        }
    }
}
