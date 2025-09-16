using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public bool IsEnteringPortal { get => isEnteringPortal;}

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

            targetPortal = portal;
            bodyController.OccupiedTileController.ForceRestoreAll();

            DebugLog($"Initiating portal movement to {portal.name}");

            StartPortalEnterAnimation();
            bodyController.CanControl = false;
        }

        private void StartPortalEnterAnimation()
        {
            if (isEnteringPortal) return;

            isEnteringPortal = true;

            if (portalMovementCoroutine != null)
            {
                StopCoroutine(portalMovementCoroutine);
                portalMovementCoroutine = null;
            }

            ExtendPathToPortalCenter();
            StartCoroutine(EnterPortalAnimation());

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

                bodyController.StartMovePath(extendedPath);
            }
        }

        private IEnumerator EnterPortalAnimation()
        {
            //yield return new WaitForSeconds(1f);

            if (targetPortal == null || bodyController == null) yield break;
            Vector3 portalCenter = targetPortal.transform.position;
            var orderedSegments = bodyController.GetOrderedSegments();
            List<bool> segmentAnimated = new List<bool>(new bool[orderedSegments.Count]);
            while (!segmentAnimated.All(x => x))
            {
                for (int i = 0; i < orderedSegments.Count; i++)
                {
                    if (segmentAnimated[i]) continue;
                    var segment = orderedSegments[i];
                    float distanceToPortal = Vector3.Distance(segment.transform.position, portalCenter);
                    if (distanceToPortal <= portalEnterDistance)
                    {
                        segmentAnimated[i] = true;
                        StartCoroutine(AnimateSegmentDown(segment, portalCenter, i == orderedSegments.Count - 1));
                    }
                }
                yield return null;
            }
        }

        private IEnumerator AnimateSegmentDown(Segment segment, Vector3 portalCenter, bool isLast = false)
        {
            if (isLast)
            {
                bodyController.StopMoveCoroutine();
            }

            Vector3 startPos = segment.transform.position;
            Vector3 targetPos = portalCenter + Vector3.forward * bodyController.Length;

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

        private void FinishMoveToPortal()
        {
            if (targetPortal == null || bodyController == null) return;
            
            bodyController.OccupiedTileController.ClearAllOccupied();
            LevelEvent.OnMoveToPortalDone(bodyController, targetPortal);
            DebugLog("Finish move portal");
            targetPortal.Disappear();
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