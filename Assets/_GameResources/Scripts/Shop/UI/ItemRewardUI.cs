using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class ItemRewardUI : MonoBehaviour
    {
        [SerializeField] private ListItemRewardIcon m_listIcon;
        [SerializeField] private Image m_imageIcon;
        [SerializeField] private Text m_textQuantity;

        public void SetData(RewardData rewardData)
        {
            m_imageIcon.sprite = m_listIcon.listIcon[rewardData.type];
            if (rewardData.type != ItemType.INFINITY_LIVES)
            {
                m_textQuantity.text = $"x{rewardData.quantity}" ;
            }
            else if(rewardData.type == ItemType.REMOVE_INTER_ADS 
                || rewardData.type == ItemType.REMOVE_BANNER_ADS
                || rewardData.type == ItemType.REMOVE_ADS)
            {
                m_textQuantity.text = "";
            }
            else
            {
                m_textQuantity.text = $"{rewardData.quantity/3600}h";
            }
        }
    }
}
