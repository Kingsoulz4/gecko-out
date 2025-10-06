using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Geckout
{
    public class ScissorBooster : BoosterBase
    {
        [SerializeField] private GameObject m_scissorPrefab;
        [SerializeField] private Transform m_spawnPoint;
        protected override int CurrentCount { get => UserDataManager.CissorBooster; set => UserDataManager.CissorBooster = value; }

        public bool IsSelectedToUse { get; set; }

        private Coroutine coroutineActivated;

        public override void ActiveBooster()
        {
            if (!IsSelectedToUse) return;

            //IsSelectedToUse = true;

            base.ActiveBooster();
            UserDataManager.CissorBooster = CurrentCount;
            OnStartUseBooster?.Invoke(this, CurrentCount);
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
            scissorObject.transform.DOLocalRotate(scissorObject.transform.localRotation.eulerAngles + Vector3.up * 360, 0.5f, RotateMode.FastBeyond360);
            yield return new WaitForSeconds(0.6f);
            //scissorObject.transform.DOMove(body.Segments.Last().transform.position, 0.5f);

            var target = body.Segments.Last().transform;

            float flightDuration = 0.5f;

            Vector3 startPos = scissorObject.transform.position;
            Vector3 endPos = target.position - Vector3.forward * 0.5f;
            Transform controlPointA = null;
            Transform controlPointB = null;
            Vector3 cpA = controlPointA ? controlPointA.position : (startPos + (endPos - startPos) * 0.33f + new Vector3(3, 2, 0));
            Vector3 cpB = controlPointB ? controlPointB.position : (startPos + (endPos - startPos) * 0.66f + new Vector3(-3, 2, 0));

            // path goes through control points -> natural XY curve
            Vector3[] path = new Vector3[] {startPos, cpA, endPos };

            Tween flightTween = scissorObject.transform
                .DOPath(path, flightDuration, PathType.CatmullRom, PathMode.Full3D, 10, Color.green)
                .SetEase(Ease.InOutSine);
                //.SetLookAt(0.01f); // rotate toward movement

            yield return new WaitForSeconds(flightDuration);

            scissorObject.transform.DOLocalRotate(Vector3.forward * 30, 0.5f).SetLoops(2, LoopType.Yoyo);

            yield return new WaitForSeconds(0.8f);

            body.CutOutLastSegment();
            yield return new WaitForSeconds(0.5f);
            Destroy(scissorObject.gameObject);
            Done();
        }    

        protected override void Done()
        {
            base.Done();
           
        }
    }
}
