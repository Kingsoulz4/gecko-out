using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class TutorialHandGuide : MonoBehaviour
    {
        [SerializeField] private GameObject m_handObject;
        [SerializeField] private float m_duration = 2f;

        public void ShowGuidePath(List<Vector3> path)
        {
            StartCoroutine(IEGuideFollowPath(path));
        }    

        private IEnumerator IEGuideFollowPath(List<Vector3> path)
        {
            yield return null;
            float moveEachStepDuration = m_duration/path.Count;
            m_handObject.gameObject.SetActive(true);
            for(int i=0; i<path.Count; i++)
            {
                var pos = new Vector3(path[i].x, path[i].y, m_handObject.transform.position.z);
                m_handObject.transform.DOMove(pos, moveEachStepDuration).SetEase(Ease.Linear);
                yield return new WaitForSeconds(moveEachStepDuration);
            }
        }    
    }
}
