using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geckout
{
    public class MoveToPortal : MonoBehaviour
    {
        [Header("Portal Movement Settings")]
        [SerializeField] private BodyController bodyController;
        [SerializeField] private float animationDurationPerSegment = 0.3f;
        [SerializeField] private float portalEnterDistance = 0.4f;
        [SerializeField] private bool enableDebugLogs = false;

        private Portal targetPortal = null;
        private bool isEnteringPortal = false;
        private Coroutine portalMovementCoroutine = null;

        public bool IsEnteringPortal { get => isEnteringPortal; }
        public float PortalEnterDistance { get => portalEnterDistance; }
        public Portal TargetPortal { get => targetPortal;}

        private void Start()
        {
            if (bodyController == null)
            {
                bodyController = GetComponent<BodyController>();
            }
        }

        public void InitiatePortalMovement(Portal portal)
        {
            if (isEnteringPortal)
            {
                return;
            }

            isEnteringPortal = true;

            targetPortal = portal;
            bodyController.OccupiedTileController.ForceRestoreAll();
            bodyController.OccupiedTileController.ClearAllOccupied();
            DebugLog($"Initiating portal movement to {portal.name}");

            //StartPortalEnterAnimation();
            ExtendPathToPortalCenter();
            bodyController.CanControl = false;
        }

        void ExtendPathToPortalCenter()
        {
            if (targetPortal == null) return;

            Vector2Int portalCoord = GameMap.WorldToGridPosition(targetPortal.transform.position);

            List<Vector2Int> extendedPath = new List<Vector2Int>();

            int segmentCount = bodyController.GetOrderedSegments().Count;
            for (int i = 0; i < segmentCount; i++)
            {
                extendedPath.Add(portalCoord);
            }

            bodyController.StartMovePath(extendedPath, true);
        }

        public IEnumerator AnimateSegmentDown(Segment segment, Vector3 portalCenter, bool isLast = false)
        {
            if (isLast)
            {
                bodyController.StopMoveCoroutine();
                this.Wait(Time.deltaTime * 6, () =>
                {
                    TargetPortal.PlayDoneAnim();
                });
            }

            Vector3 startPos = segment.transform.position;
            Vector3 targetPos = portalCenter + Vector3.forward * Mathf.Max(3, bodyController.Length - 1);

            float elapsed = 0f;
            while (elapsed < animationDurationPerSegment)
            {
                if (segment == null) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / animationDurationPerSegment;
                float curveT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 currentPos = startPos;
                currentPos.z = Mathf.Lerp(startPos.z, targetPos.z, curveT);
                segment.transform.position = currentPos;
                yield return null;
            }
            if (isLast)
            {
                FinishMoveToPortal();
            }
        }

        public IEnumerator IEEnterPortalBooster(Portal portal)
        {
            if (isEnteringPortal) yield break;

            targetPortal = portal;
            isEnteringPortal = true;
            bodyController.CanControl = false;

            DebugLog($"Starting portal booster jump to {portal.name}");

            Vector3 portalCenter = targetPortal.transform.position;
            var orderedSegments = bodyController.Segments;

            List<Tween> jumpTweens = new List<Tween>();

            for (int i = 0; i < orderedSegments.Count; i++)
            {
                var segment = orderedSegments[i];
                if (segment == null) continue;

                // Calculate jump parameters
                float jumpPower = 1;// Random.Range(2f, 4f); // Random jump height for variety
                float duration = animationDurationPerSegment + (i * 0.05f);

                var jumpTween = segment.transform.DOJump(
                    portalCenter,
                    jumpPower,
                    1,
                    duration
                ).SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    bool isLast = segment == orderedSegments[orderedSegments.Count - 1];
                    StartCoroutine(AnimateSegmentDown(segment, portalCenter, isLast));
                });

                jumpTweens.Add(jumpTween);
            }

            yield return new WaitUntil(() => jumpTweens.All(t => t == null || !t.IsActive()));

            DebugLog("Portal booster jump animation completed");
        }

        private void FinishMoveToPortal()
        {
            if (targetPortal == null || bodyController == null) return;

            LevelEvent.OnMoveToPortalDone?.Invoke(bodyController, targetPortal);
            DebugLog("Finish move portal");
            gameObject.SetActive(false);
            ResetPortalState();
        }

        private void ResetPortalState()
        {
            targetPortal = null;
            isEnteringPortal = false;

            if (portalMovementCoroutine != null)
            {
                StopCoroutine(portalMovementCoroutine);
                portalMovementCoroutine = null;
            }
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[MoveToPortal] {message}");
            }
        }

        private void OnDisable()
        {
            ResetPortalState();
        }

        private void OnDestroy()
        {
            ResetPortalState();
            DOTween.Kill(gameObject);
        }

        #region Editor Support
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bodyController == null)
            {
                bodyController = GetComponent<BodyController>();
            }
        }
#endif
        #endregion
    }
}