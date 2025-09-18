using AYellowpaper.SerializedCollections;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class ButtonPlay : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<LevelType, GameObject> m_listTypeDisplay;

        public void SetDisplayLevelType(LevelType levelType)
        {
            foreach (var item in m_listTypeDisplay)
            {
                item.Value.SetActive(false);
            }
            m_listTypeDisplay[levelType].SetActive(true);
        }

        
    }
}
