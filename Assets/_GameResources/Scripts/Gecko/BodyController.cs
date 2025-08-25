using Dreamteck.Splines;
using Geckout.Generals;
using Geckout.PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class BodyController : MonoBehaviour
    {
        public enum ControlAnchor
        {
            Head,
            Tail
        }

        [SerializeField] private int length = 4;
        [SerializeField] private Segment headPrefab;
        [SerializeField] private Segment segment;
        [SerializeField] private Segment tailPrefab;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float outerSmoothness = 0.7f;
        [SerializeField] private float cornerRadius = 0.3f;
        [SerializeField] private OccupiedTileController occupiedTileController;
        [SerializeField] private BodyRenderer _bodyRenderer;
        [SerializeField] private ControlAnchor controlAnchor = ControlAnchor.Head;

        private Segment _head, _tail;
        public List<Segment> Segments { private set; get; }
        public bool IsMoving { get => isMoving; }
        public OccupiedTileController OccupiedTileController { get => occupiedTileController; set => occupiedTileController = value; }

        bool isMoving = false;

        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private Coroutine moveCoroutine;

        private float segmentSpacing = 1f;

        private LinkedList<Vector3> historyPoints = new LinkedList<Vector3>();
        private float historyTotalLength = 0f;
        private const float minSampleStep = 0.1f;
        private const float extraHistoryPadding = 4f;

        private void Start()
        {
            Segments = new List<Segment>();

            // Head
            _head = Instantiate(headPrefab, transform);
            _head.name = "Head";
            _head.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Segments.Add(_head);

            // Body segments
            for (int i = 1; i < length - 1; i++)
            {
                Segment seg = Instantiate(this.segment, transform);
                seg.name = "Segment " + i;
                seg.transform.localPosition = new Vector3(0, -i, 0);
                Segments.Add(seg);
            }

            // Tail
            _tail = Instantiate(tailPrefab, transform);
            _tail.name = "Tail";
            _tail.transform.localPosition = new Vector3(0, -(length - 1), 0);
            Segments.Add(_tail);

            // Setup neighbors
            _head.Setup(null, Segments[1]);
            _head.SetController(this);

            _tail.Setup(Segments[Segments.Count - 2], null);

            for (int i = 1; i < Segments.Count - 1; i++)
            {
                var currentSegment = Segments[i];
                Segment prevSegment = Segments[i - 1];
                Segment nextSegment = Segments[i + 1];
                currentSegment.Setup(prevSegment, nextSegment);
                currentSegment.SetController(this);
                currentSegment.SetCorner(outerSmoothness, cornerRadius);
            }

            // Set coordinate ban đầu
            for (int i = 0; i < Segments.Count; i++)
            {
                var coordinate = new Vector2Int(0, GameMap.MapSize.y - i - 1);
                Segments[i].SetCoordinate(coordinate);
            }

            // Auto-calc spacing
            if (Segments.Count > 1)
            {
                segmentSpacing = Vector3.Distance(
                    Segments[0].transform.position,
                    Segments[1].transform.position
                );
            }

            InitHistoryFromSegments();

            if (_bodyRenderer != null)
                _bodyRenderer.Initialize(Segments);
        }

        public void SetMovementPath(List<Vector2Int> path)
        {
            if (path == null || path.Count == 0) return;

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            currentPath = new List<Vector2Int>(path);
            moveCoroutine = StartCoroutine(FollowPathContinuous());
        }
        public void SetControlAnchor(ControlAnchor anchor)
        {
            controlAnchor = anchor;
        }
        public void ClearPath()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            currentPath.Clear();
            isMoving = false;
        }

        IEnumerator FollowPathContinuous()
        {
            if (currentPath.Count == 0) yield break;
            isMoving = true;

            List<Vector3> worldPath = new List<Vector3>();
            if (controlAnchor == ControlAnchor.Head)
                worldPath.Add(_head.transform.position);
            else
                worldPath.Add(_tail.transform.position);

            foreach (var coord in currentPath)
            {
                if (GameMap.TryGetTileAt(coord, out var tile))
                    worldPath.Add(tile.transform.position);
            }

            yield return SmoothPathMovement_History(worldPath, controlAnchor == ControlAnchor.Head);

            isMoving = false;
            currentPath.Clear();
        }

        IEnumerator SmoothPathMovement_History(List<Vector3> worldPath, bool moveHead)
        {
            if (worldPath.Count < 2) yield break;

            float totalPathLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < worldPath.Count - 1; i++)
            {
                float length = Vector3.Distance(worldPath[i], worldPath[i + 1]);
                segmentLengths.Add(length);
                totalPathLength += length;
            }

            float anchorDist = 0f;
            Vector3 lastAnchorPos = moveHead ? _head.transform.position : _tail.transform.position;

            while (anchorDist < totalPathLength)
            {
                anchorDist += moveSpeed * Time.deltaTime;
                Vector3 anchorPos = GetPointAtDistanceOnWorldPath(worldPath, segmentLengths, anchorDist);

                if ((anchorPos - lastAnchorPos).sqrMagnitude > (minSampleStep * minSampleStep))
                {
                    if (moveHead) AddHeadSample(anchorPos);
                    else AddTailSample(anchorPos);
                    lastAnchorPos = anchorPos;
                }

                for (int segIdx = 0; segIdx < Segments.Count; segIdx++)
                {
                    float backDist = segIdx * segmentSpacing;
                    Vector3 pos = moveHead
                        ? GetHistoryPointAtDistanceBack(backDist)
                        : GetHistoryPointAtDistanceForward(backDist);
                    Segments[segIdx].transform.position = pos;
                }

                yield return null;
            }

            Vector3 finalAnchor = worldPath[worldPath.Count - 1];
            if (moveHead) AddHeadSample(finalAnchor);
            else AddTailSample(finalAnchor);

            for (int segIdx = 0; segIdx < Segments.Count; segIdx++)
            {
                float backDist = segIdx * segmentSpacing;
                Vector3 pos = moveHead
                    ? GetHistoryPointAtDistanceBack(backDist)
                    : GetHistoryPointAtDistanceForward(backDist);
                Segments[segIdx].transform.position = pos;
            }
        }

        private Vector3 GetPointAtDistanceOnWorldPath(List<Vector3> path, List<float> segLens, float distance)
        {
            if (path.Count < 2) return path[0];
            if (distance <= 0f) return path[0];

            float total = 0f;
            for (int i = 0; i < segLens.Count; i++)
            {
                float segLen = segLens[i];
                if (total + segLen >= distance)
                {
                    float t = (distance - total) / segLen;
                    t = movementCurve.Evaluate(Mathf.Clamp01(t));
                    return Vector3.Lerp(path[i], path[i + 1], t);
                }
                total += segLen;
            }
            return path[path.Count - 1];
        }

        // ===== History =====

        private float RequiredHistoryLength()
        {
            return Mathf.Max(0f, (Segments.Count - 1) * segmentSpacing + extraHistoryPadding);
        }

        private void InitHistoryFromSegments()
        {
            historyPoints.Clear();
            historyTotalLength = 0f;
            historyPoints.AddLast(Segments[Segments.Count - 1].transform.position);

            for (int i = Segments.Count - 2; i >= 0; i--)
            {
                Vector3 from = historyPoints.Last.Value;
                Vector3 to = Segments[i].transform.position;
                AppendSegmentSamples(from, to);
            }

            TrimHistory();
        }

        private void AppendSegmentSamples(Vector3 from, Vector3 to)
        {
            float segmentLen = Vector3.Distance(from, to);
            if (segmentLen <= Mathf.Epsilon)
            {
                if ((historyPoints.Last.Value - to).sqrMagnitude > 1e-6f)
                {
                    historyTotalLength += Vector3.Distance(historyPoints.Last.Value, to);
                    historyPoints.AddLast(to);
                }
                return;
            }

            int steps = Mathf.Max(1, Mathf.CeilToInt(segmentLen / minSampleStep));
            for (int s = 1; s <= steps; s++)
            {
                float t = (float)s / steps;
                Vector3 p = Vector3.Lerp(from, to, t);
                float d = Vector3.Distance(historyPoints.Last.Value, p);
                if (d > Mathf.Epsilon)
                {
                    historyTotalLength += d;
                    historyPoints.AddLast(p);
                }
            }
        }

        private void AddHeadSample(Vector3 headPos)
        {
            if (historyPoints.Count == 0)
            {
                historyPoints.AddLast(headPos);
                return;
            }

            float d = Vector3.Distance(historyPoints.Last.Value, headPos);
            if (d < minSampleStep) return;

            historyPoints.AddLast(headPos);
            historyTotalLength += d;

            TrimHistory();
        }

        private void AddTailSample(Vector3 tailPos)
        {
            if (historyPoints.Count == 0)
            {
                historyPoints.AddFirst(tailPos);
                return;
            }

            float d = Vector3.Distance(historyPoints.First.Value, tailPos);
            if (d < minSampleStep) return;

            historyPoints.AddFirst(tailPos);
            historyTotalLength += d;

            TrimHistory();
        }

        private void TrimHistory()
        {
            float need = RequiredHistoryLength();
            while (historyPoints.Count > 1 && historyTotalLength > need)
            {
                Vector3 first = historyPoints.First.Value;
                Vector3 second = historyPoints.First.Next.Value;
                float seg = Vector3.Distance(first, second);

                historyPoints.RemoveFirst();
                historyTotalLength -= seg;
            }
        }

        private Vector3 GetHistoryPointAtDistanceBack(float backDistance)
        {
            if (historyPoints.Count == 0) return Vector3.zero;
            if (backDistance <= 0f) return historyPoints.Last.Value;

            float remain = backDistance;
            LinkedListNode<Vector3> bNode = historyPoints.Last;
            while (bNode.Previous != null)
            {
                Vector3 b = bNode.Value;
                Vector3 a = bNode.Previous.Value;
                float seg = Vector3.Distance(a, b);

                if (remain <= seg)
                {
                    float t = 1f - (remain / Mathf.Max(seg, 1e-6f));
                    return Vector3.Lerp(a, b, t);
                }

                remain -= seg;
                bNode = bNode.Previous;
            }
            return historyPoints.First.Value;
        }

        private Vector3 GetHistoryPointAtDistanceForward(float forwardDistance)
        {
            if (historyPoints.Count == 0) return Vector3.zero;
            if (forwardDistance <= 0f) return historyPoints.First.Value;

            float remain = forwardDistance;
            LinkedListNode<Vector3> aNode = historyPoints.First;
            while (aNode.Next != null)
            {
                Vector3 a = aNode.Value;
                Vector3 b = aNode.Next.Value;
                float seg = Vector3.Distance(a, b);

                if (remain <= seg)
                {
                    float t = (remain / Mathf.Max(seg, 1e-6f));
                    return Vector3.Lerp(a, b, t);
                }

                remain -= seg;
                aNode = aNode.Next;
            }
            return historyPoints.Last.Value;
        }
    }
}
