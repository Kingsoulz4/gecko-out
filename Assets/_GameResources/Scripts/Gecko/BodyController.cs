using Dreamteck.Splines;
using Geckout.Generals;
using Geckout.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] private int length = 3;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float minSampleStep = 0.05f;
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Segment headPrefab;
        [SerializeField] private Segment segment;
        [SerializeField] private Segment tailPrefab;
        [SerializeField] private OccupiedTileController occupiedTileController;
        [SerializeField] private BodyRenderer _bodyRenderer;
        [SerializeField] private ControlAnchor controlAnchor = ControlAnchor.Head;
       
        // Movement events
        public Action OnStartMove;
        public Action OnEndMove;

        private Segment _head, _tail;
        private bool isMoving = false;
        private float historyTotalLength = 0f;
        private const float extraHistoryPadding = 4f;
        private Coroutine moveCoroutine;
        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private LinkedList<Vector3> historyPoints = new LinkedList<Vector3>();
        private float segmentSpacing;

        public List<Segment> Segments { private set; get; }
        public bool IsMoving { get => isMoving; }
        public OccupiedTileController OccupiedTileController { get => occupiedTileController; set => occupiedTileController = value; }


        // Mỗi phần tử: (totalSegments, spacing)
        private static readonly List<(int totalSegments, float spacing)> SegmentConfig =
            new List<(int, float)>
            {
        (0, 0f),                 
        (3,  1f / (3 - 1)),      
        (6,  2f / (6 - 1)),      
        (9,  3f / (9 - 1)),      
        (12, 4f / (12 - 1)),     
        (15, 5f / (15 - 1)),     
        (18, 6f / (18 - 1)),     
        (21, 7f / (21 - 1))      
            };

        private void Start()
        {
            Segments = new List<Segment>();

            // ===== Lấy config theo length =====
            (int totalSegments, float segmentSpacing) = SegmentConfig[length-1];
            int bodySegmentCount = totalSegments - 2;
            this.segmentSpacing = segmentSpacing;
            // ===== Head =====
            _head = Instantiate(headPrefab, transform);
            _head.name = "Head";
            _head.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Segments.Add(_head);

            // ===== Body segments =====
            for (int i = 1; i <= bodySegmentCount; i++)
            {
                Segment seg = Instantiate(this.segment, transform);
                seg.name = "Body " + i;
                float yPos = -i * segmentSpacing;
                seg.transform.localPosition = new Vector3(0, yPos, 0);
                Segments.Add(seg);
            }

            // ===== Tail =====
            _tail = Instantiate(tailPrefab, transform);
            _tail.name = "Tail";
            float tailYPos = -length; // luôn ở cuối
            _tail.transform.localPosition = new Vector3(0, tailYPos, 0);
            Segments.Add(_tail);

            // ===== Setup neighbors =====
            _head.Setup(null, Segments[1]);
            _head.SetController(this);

            _tail.Setup(Segments[Segments.Count - 2], null);
            _tail.SetController(this);

            for (int i = 1; i < Segments.Count - 1; i++)
            {
                var currentSegment = Segments[i];
                Segment prevSegment = Segments[i - 1];
                Segment nextSegment = Segments[i + 1];
                currentSegment.Setup(prevSegment, nextSegment);
                currentSegment.SetController(this);
            }

            // ===== Set coordinates based on position =====
            for (int i = 0; i < Segments.Count; i++)
            {
                float segmentYPos = -i * segmentSpacing;
                if (i == Segments.Count - 1) // Tail
                {
                    segmentYPos = -length;
                }

                int tileIndex = Mathf.RoundToInt(Mathf.Abs(segmentYPos));
                var coordinate = new Vector2Int(0, GameMap.MapSize.y - tileIndex - 1);
                Segments[i].SetCoordinate(coordinate);
            }

            // ===== Init history system =====
            InitHistoryFromSegments();

            // ===== Initialize renderer =====
            if (_bodyRenderer != null)
                _bodyRenderer.Initialize(Segments);

            Debug.Log($"Body initialized:");
            Debug.Log($"- Length: {length} units");
            Debug.Log($"- Total segments: {totalSegments} (Head + {bodySegmentCount} body + Tail)");
            Debug.Log($"- Spacing: {segmentSpacing:F3}");
            Debug.Log($"- Head at: (0, 0), Tail at: (0, {tailYPos})");
        }

        private List<Segment> GetOrderedSegments()
        {
            if (controlAnchor == ControlAnchor.Head)
                return Segments; // Normal order: Head leads
            else
                return Segments.AsEnumerable().Reverse().ToList(); // Reversed: Tail leads
        }

        public List<Segment> GetOrderedSegmentsForTileUpdate()
        {
            return GetOrderedSegments();
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

            // Set path for GridHeadClamper if present and controlling head
            GridHeadClamper gridClamper = GetComponent<GridHeadClamper>();
            if (gridClamper != null && controlAnchor == ControlAnchor.Head)
            {
                gridClamper.SetPath(path);
            }

            moveCoroutine = StartCoroutine(FollowPathContinuous());
        }

        public void SetControlAnchor(ControlAnchor anchor)
        {
            if (controlAnchor != anchor)
            {
                controlAnchor = anchor;

                // Rebuild history to match new control direction
                InitHistoryFromSegments();

                // Force occupied tile update to sync with new positions
                if (occupiedTileController != null)
                {
                    occupiedTileController.UpdateAllSegmentPositions();
                }
            }
        }

        public void ClearPath()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;

                // If we were moving and got interrupted, fire end move event
                if (isMoving)
                {
                    isMoving = false;
                    OnEndMove?.Invoke();
                }
            }
            currentPath.Clear();
            isMoving = false;
        }

        IEnumerator FollowPathContinuous()
        {
            if (currentPath.Count == 0) yield break;

            isMoving = true;
            OnStartMove?.Invoke();

            List<Vector3> worldPath = new List<Vector3>();

            // Get current anchor position (always the leading segment in ordered view)
            var orderedSegments = GetOrderedSegments();
            worldPath.Add(orderedSegments[0].transform.position);

            foreach (var coord in currentPath)
            {
                if (GameMap.TryGetTileAt(coord, out var tile))
                    worldPath.Add(tile.transform.position);
            }

            yield return SmoothPathMovement_Simplified(worldPath);

            isMoving = false;
            currentPath.Clear();
            OnEndMove?.Invoke(); // Fire end move event
        }

        // ===== BodyController - Precise Movement Method =====
        IEnumerator SmoothPathMovement_Simplified(List<Vector3> worldPath)
        {
            if (worldPath.Count < 2) yield break;

            var orderedSegments = GetOrderedSegments();
            Vector3 lastAnchorPos = orderedSegments[0].transform.position;

            // For grid-precise movement when controlling head
            int currentWaypointIndex = 0;
            bool useGridPreciseMovement = (controlAnchor == ControlAnchor.Head);
            GridHeadClamper gridClamper = useGridPreciseMovement ? GetComponent<GridHeadClamper>() : null;

            while (currentWaypointIndex < worldPath.Count - 1)
            {
                Vector3 currentTarget = worldPath[currentWaypointIndex + 1];
                Vector3 anchorPos;

                if (useGridPreciseMovement)
                {
                    // Precise movement towards target waypoint
                    float frameSpeed = moveSpeed * Time.deltaTime;
                    Vector3 intendedPos = Vector3.MoveTowards(lastAnchorPos, currentTarget, frameSpeed);

                    // Apply grid constraints through GridHeadClamper
                    if (gridClamper != null)
                    {
                        anchorPos = gridClamper.ClampHeadPosition(intendedPos);
                    }
                    else
                    {
                        anchorPos = intendedPos;
                    }

                    // Check if we reached the target waypoint (with small tolerance)
                    if (Vector3.Distance(anchorPos, currentTarget) < 0.01f)
                    {
                        anchorPos = currentTarget; // Ensure exact position
                        currentWaypointIndex++; // Move to next waypoint
                    }
                }
                else
                {
                    // Original smooth movement for tail control
                    float totalPathLength = 0f;
                    List<float> segmentLengths = new List<float>();
                    for (int i = 0; i < worldPath.Count - 1; i++)
                    {
                        float length = Vector3.Distance(worldPath[i], worldPath[i + 1]);
                        segmentLengths.Add(length);
                        totalPathLength += length;
                    }

                    float anchorDist = 0f;
                    anchorDist += moveSpeed * Time.deltaTime;
                    anchorPos = GetPointAtDistanceOnWorldPath(worldPath, segmentLengths, anchorDist);
                }

                if ((anchorPos - lastAnchorPos).sqrMagnitude > (minSampleStep * minSampleStep))
                {
                    AddAnchorSample(anchorPos);
                    lastAnchorPos = anchorPos;
                }

                // Apply positions to ordered segments
                for (int segIdx = 0; segIdx < orderedSegments.Count; segIdx++)
                {
                    float backDist = segIdx * segmentSpacing;
                    Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                    orderedSegments[segIdx].transform.position = pos;
                }

                yield return null;
            }

            // Ensure final position is exact
            if (useGridPreciseMovement)
            {
                Vector3 finalAnchor = worldPath[worldPath.Count - 1];

                if (gridClamper != null)
                {
                    finalAnchor = gridClamper.ClampHeadPosition(finalAnchor);
                }

                AddAnchorSample(finalAnchor);

                for (int segIdx = 0; segIdx < orderedSegments.Count; segIdx++)
                {
                    float backDist = segIdx * segmentSpacing;
                    Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                    orderedSegments[segIdx].transform.position = pos;
                }
            }
        }

        // ===== GridHeadClamper - Updated for Precise Movement =====
        
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

        // ===== History System =====

        private float RequiredHistoryLength()
        {
            return Mathf.Max(0f, (Segments.Count - 1) * segmentSpacing + extraHistoryPadding);
        }

        private void InitHistoryFromSegments()
        {
            historyPoints.Clear();
            historyTotalLength = 0f;

            var orderedSegments = GetOrderedSegments();

            if (controlAnchor == ControlAnchor.Head)
            {
                // Normal: Start from tail, build toward head
                historyPoints.AddLast(orderedSegments[orderedSegments.Count - 1].transform.position);

                for (int i = orderedSegments.Count - 2; i >= 0; i--)
                {
                    Vector3 from = historyPoints.Last.Value;
                    Vector3 to = orderedSegments[i].transform.position;
                    AppendSegmentSamples(from, to, true); // Add to end
                }
            }
            else
            {
                // Reverse: Start from head, build toward tail
                historyPoints.AddFirst(orderedSegments[orderedSegments.Count - 1].transform.position);

                for (int i = orderedSegments.Count - 2; i >= 0; i--)
                {
                    Vector3 from = historyPoints.First.Value;
                    Vector3 to = orderedSegments[i].transform.position;
                    AppendSegmentSamples(from, to, false); // Add to beginning
                }
            }

            TrimHistory();
        }

        private void AppendSegmentSamples(Vector3 from, Vector3 to, bool addToEnd = true)
        {
            float segmentLen = Vector3.Distance(from, to);
            if (segmentLen <= Mathf.Epsilon)
            {
                Vector3 comparePoint = addToEnd ? historyPoints.Last.Value : historyPoints.First.Value;
                if ((comparePoint - to).sqrMagnitude > 1e-6f)
                {
                    historyTotalLength += Vector3.Distance(comparePoint, to);
                    if (addToEnd)
                        historyPoints.AddLast(to);
                    else
                        historyPoints.AddFirst(to);
                }
                return;
            }

            int steps = Mathf.Max(1, Mathf.CeilToInt(segmentLen / minSampleStep));
            for (int s = 1; s <= steps; s++)
            {
                float t = (float)s / steps;
                Vector3 p = Vector3.Lerp(from, to, t);

                Vector3 comparePoint = addToEnd ? historyPoints.Last.Value : historyPoints.First.Value;
                float d = Vector3.Distance(comparePoint, p);

                if (d > Mathf.Epsilon)
                {
                    historyTotalLength += d;
                    if (addToEnd)
                        historyPoints.AddLast(p);
                    else
                        historyPoints.AddFirst(p);
                }
            }
        }

        private void AddAnchorSample(Vector3 anchorPos)
        {
            if (historyPoints.Count == 0)
            {
                historyPoints.AddLast(anchorPos);
                return;
            }

            if (controlAnchor == ControlAnchor.Head)
            {
                // Normal: Add to end, segments follow behind
                float d = Vector3.Distance(historyPoints.Last.Value, anchorPos);
                if (d < minSampleStep) return;

                historyPoints.AddLast(anchorPos);
                historyTotalLength += d;
            }
            else
            {
                // Reverse: Add to beginning, segments follow behind in reverse
                float d = Vector3.Distance(historyPoints.First.Value, anchorPos);
                if (d < minSampleStep) return;

                historyPoints.AddFirst(anchorPos);
                historyTotalLength += d;
            }

            TrimHistory();
        }

        private void TrimHistory()
        {
            float need = RequiredHistoryLength();

            if (controlAnchor == ControlAnchor.Head)
            {
                // Normal: Remove from beginning (oldest)
                while (historyPoints.Count > 1 && historyTotalLength > need)
                {
                    Vector3 first = historyPoints.First.Value;
                    Vector3 second = historyPoints.First.Next.Value;
                    float seg = Vector3.Distance(first, second);

                    historyPoints.RemoveFirst();
                    historyTotalLength -= seg;
                }
            }
            else
            {
                // Reverse: Remove from end (oldest in reverse direction)
                while (historyPoints.Count > 1 && historyTotalLength > need)
                {
                    Vector3 last = historyPoints.Last.Value;
                    Vector3 secondLast = historyPoints.Last.Previous.Value;
                    float seg = Vector3.Distance(last, secondLast);

                    historyPoints.RemoveLast();
                    historyTotalLength -= seg;
                }
            }
        }

        private Vector3 GetHistoryPointAtDistanceBack(float backDistance)
        {
            if (historyPoints.Count == 0) return Vector3.zero;
            if (backDistance <= 0f)
            {
                // Return anchor position
                return controlAnchor == ControlAnchor.Head ?
                    historyPoints.Last.Value : historyPoints.First.Value;
            }

            if (controlAnchor == ControlAnchor.Head)
            {
                // Normal direction: traverse from Last to First
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
            else
            {
                // Reverse direction: traverse from First to Last
                float remain = backDistance;
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
}