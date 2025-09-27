using DG.Tweening;
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
        [SerializeField] private GameObject m_ribbon;

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
            StartCoroutine(IEAnimateShow());
        }

        private IEnumerator IEAnimateShow()
        {
            for(int i=0; i<m_listRewardHolders.Count; i++)
            {
                for(int j=0; j < m_listRewardHolders[i].childCount; j++)
                {
                    m_listRewardHolders[j].transform.localScale = Vector3.zero;
                }
            }
            m_ribbon.transform.localScale = Vector3.zero;
            m_buttonClaim.transform.localScale = Vector3.zero;

            yield return new WaitForEndOfFrame();

            m_ribbon.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < m_listRewardHolders.Count; i++)
            {
                for (int j = 0; j < m_listRewardHolders[i].childCount; j++)
                {
                    m_listRewardHolders[j].transform.DOScale(1, 0.2f).SetEase(Ease.OutBack);
                    yield return new WaitForSeconds(0.15f);
                }
            }

            yield return new WaitForSeconds(0.2f);

            m_buttonClaim.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
        }
    }
}
