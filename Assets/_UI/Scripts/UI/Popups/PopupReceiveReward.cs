using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class PopupReceiveReward : PopupUI
    {
        [SerializeField] private ItemRewardUI m_itemPrefab;
        [SerializeField] private List<Transform> m_listRewardHolders;


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
                    newItem.SetData(listRewardData[i]);
                }    
            }    
        }
    }
}
