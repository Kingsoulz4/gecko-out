using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BodyController
    {
        [HideInInspector]
        [Header("Tool")]
        [SerializeField] private Outline m_outlineSelected;

        private Outline OutlineSelected
        {
            get
            {
                if (m_outlineSelected == null)
                {
                    m_outlineSelected = gameObject.AddComponent<Outline>();
                    m_outlineSelected.OutlineColor = Color.red;
                    m_outlineSelected.OutlineWidth = 8;

                }
                return m_outlineSelected;
            }
        }

        #region Tool

        public void SetSelected(bool selected)
        {
            OutlineSelected.enabled = selected;
        }

        public void UpdateColor()
        {
            _bodyRenderer.UpdateBodyColor();
        }

        #endregion
    }
}
