using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class TimeBoosterTopUI : MonoBehaviour
    {
        [SerializeField] private FillVisualUI fillVisualUI;
        [SerializeField] private GameObject frezzeTimeBG;
        [SerializeField] private GameObject frezzeTimeVfx;
   
        public void UpdateFill(float value, string valueText)
        {
            fillVisualUI.UpdateFill(value, valueText);
        }

        public void SetActive(bool isActive)
        {
            frezzeTimeBG.SetActive(isActive);
            frezzeTimeVfx.SetActive(isActive);
            fillVisualUI.gameObject.SetActive(isActive);
        }
    }
}
