using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class ScissorBooster : BoosterBase
    {
        [SerializeField] private GameObject m_scissorPrefab;
        [SerializeField] private Transform m_spawnPoint;

        public bool IsSelectedToUse { get; set; }

        private Coroutine coroutineActivated;

        public override void ActiveBooster()
        {
            if (!IsSelectedToUse) return;

            IsSelectedToUse = true;

            base.ActiveBooster();
            UserDataManager.TimeIngameBooster = CurrentCount;
            OnStartUseBooster?.Invoke(this, CurrentCount);
            InProgress = true;
        }

        public void ActiveBooster(BodyController body)
        {
            if(coroutineActivated != null)
            {
                StopCoroutine(coroutineActivated);
                coroutineActivated = null;
            }
            ActiveBooster();
            coroutineActivated = StartCoroutine(IEAnimateBooster(body));
        }    

        private IEnumerator IEAnimateBooster(BodyController body)
        {
            var scissorObject = Instantiate(m_scissorPrefab, transform);
            scissorObject.transform.localScale = Vector3.zero;
            scissorObject.transform.position = m_spawnPoint.position;
            scissorObject.transform.DOScale(1, 0.5f);
            yield return new WaitForSeconds(0.5f);
            scissorObject.transform.DOMove(body.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);
            body.CutOutLastSegment();
            yield return new WaitForSeconds(0.5f);
            Done();
        }    

        protected override void Done()
        {
            base.Done();
           
        }
    }
}
