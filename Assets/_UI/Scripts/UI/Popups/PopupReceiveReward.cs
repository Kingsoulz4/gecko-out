using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupReceiveReward : PopupUI
    {
        [SerializeField] private ItemRewardUI m_itemPrefab;
        [SerializeField] private List<Transform> m_listRewardHolders;
        [SerializeField] private Button m_buttonClaim;

        private void Awake()
        {
            m_buttonClaim.onClick.AddListener(OnClickClaim);
        }

        private void OnClickClaim()
        {
            Hide();
        }

        public void SetData(List<RewardDataGameGecko> listRewardData)
        {
            for (int i = 0; i < m_listRewardHolders.Count; i++)
            {
                MyUlti.RemoveAllChilds(m_listRewardHolders[i]);
            }
            if(listRewardData.Count <= 3)
            {
                for(int i=0; i<listRewardData.Count; i++)
                {
                    var newItem = Instantiate(m_itemPrefab, m_listRewardHolders[1]);
                    newItem.gameObject.SetActive(true);
                    newItem.SetData(listRewardData[i]);
                }    
            }    
            else
            {
                int j = 0;
                for (int i = 0; i < listRewardData.Count; i++)
                {
                    if(i % 3 == 0 && i > 0)
                    {
                        j ++;
                    }
                    var newItem = Instantiate(m_itemPrefab, m_listRewardHolders[j]);
                    newItem.gameObject.SetActive(true);
                    newItem.SetData(listRewardData[i]);
                  
                }
            }
        }
    }
}
