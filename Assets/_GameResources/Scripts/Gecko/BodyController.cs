using Dreamteck.Splines;
using Geckout.Data;
using Geckout.Generals;
using Geckout.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Geckout
{
    public partial class BodyController : MonoBehaviour
    {
        public enum ControlAnchor
        {
            Head,
            Tail
        }

        [SerializeField] private int length = 3;
        [SerializeField] private float moveSpeed = 18f;
        [SerializeField] private float lowSpeed = 10f;
        [SerializeField] private float lowDistance = 2f;
        [SerializeField] private float minSampleStep = 0.02f;
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Segment headPrefab;
        [SerializeField] private Segment segment;
        [SerializeField] private Segment tailPrefab;
        [SerializeField] private OccupiedTileController occupiedTileController;
        [SerializeField] private BodyRenderer _bodyRenderer;
        [SerializeField] private GridHeadClamper gridClamper;
        public ControlAnchor controlAnchor = ControlAnchor.Head;



        // Movement events
        public Action OnStartMove;
        public Action OnEndMove;

        private readonly int subLength = 3;
        private Segment _head, _tail;
        private float historyTotalLength = 0f;
        private const float extraHistoryPadding = 4f;
        private Coroutine moveCoroutine;
        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private LinkedList<Vector3> historyPoints = new LinkedList<Vector3>();
        private float segmentSpacing;

        public List<Segment> Segments { private set; get; }

        public OccupiedTileController OccupiedTileController { get => occupiedTileController; set => occupiedTileController = value; }
        public bool IsMoving
        {
            get
            {
                if (moveCoroutine != null) return true;

                //if (gridClamper != null && !gridClamper.IsAtTileCenter) return true;
                return false;
            }
        }

        public GridHeadClamper GridClamper { get => gridClamper; set => gridClamper = value; }

        public DogData DogData { get; set; }
        public int SubLength => subLength;



        private void Start()
        {
            //Init();
        }

        public void Initialize(DogData dogData)
        {
            DogData = dogData;
            List<Vector2Int> listDefaultCoordinate = dogData.listCoordinate;
            Segments = new List<Segment>();

            // ===== Tính toán tổng số segment =====
            length = listDefaultCoordinate.Count ;
            int totalSegments = length * SubLength - 2;
            float unitSpacing = 1f / SubLength;
            segmentSpacing = unitSpacing;
            float totalBodyLength = length;

            // ===== Head =====
            _head = Instantiate(headPrefab, transform);
            _head.name = "Head";
            _head.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Segments.Add(_head);

            // ===== Body segments =====
            // chỉ spawn từ 1 đến totalSegments - 2 (dành chỗ cho Tail)
            for (int i = 1; i < totalSegments - 1; i++)
            {
                Segment seg = Instantiate(this.segment, transform);
                seg.name = "Segment " + i;
                seg.transform.localPosition = new Vector3(0, -i * unitSpacing, 0);
                if (i % SubLength == 0)
                {
                    seg.gameObject.AddComponent<BoxCollider>();
                }
                Segments.Add(seg);
            }

            // ===== Tail =====
            _tail = Instantiate(tailPrefab, transform);
            _tail.name = "Tail";
            _tail.transform.localPosition = new Vector3(0, -(totalSegments - 1) * unitSpacing, 0);
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

            // ===== Set coordinate ban đầu =====
            //for (int i = 0; i < Segments.Count; i++)
            //{
            //    // unitIndex = segment thuộc về tile nào
            //    int unitIndex = i / subLength;

            //    var coordinate = new Vector2Int(0, GameMap.MapSize.y - unitIndex - 1);
            //    Segments[i].SetCoordinate(coordinate);
            //}

            for (int i = 0; i < Segments.Count; i++)
            {
                // unitIndex = segment thuộc về tile nào
                int unitIndex = Mathf.CeilToInt((float)i / SubLength);

                //var coordinate = new Vector2Int(0, GameMap.MapSize.y - unitIndex - 1);
                var coordinate = listDefaultCoordinate[Mathf.Clamp(unitIndex, 0, listDefaultCoordinate.Count - 2)];

                if(i == Segments.Count -1)
                {
                    coordinate = listDefaultCoordinate.Last();
                }    

                Segments[i].InitCoordinate(coordinate);
            }

            // ===== Init history system =====
            InitHistoryFromSegments();

            // ===== Initialize renderer =====
            if (_bodyRenderer != null)
                _bodyRenderer.Initialize(Segments);

            Debug.Log($"Body initialized: length={length}, subLength={SubLength}, totalSegments={totalSegments}, totalBodyLength={totalBodyLength}");
        }

        public List<Segment> GetOrderedSegments()
        {
            if (controlAnchor == ControlAnchor.Head)
                return Segments;
            else
                return Segments.AsEnumerable().Reverse().ToList();
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
            if (gridClamper != null)
            {
                gridClamper.SetPath(path);
            }

            moveCoroutine = StartCoroutine(StartMovePath());
        }

        public void SetControlAnchor(ControlAnchor anchor)
        {
            if (controlAnchor != anchor)
            {
                controlAnchor = anchor;

                // Rebuild history to match new control direction
                InitHistoryFromSegments();
            }
        }

        public void ClearPath()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            currentPath.Clear();
        }

        IEnumerator StartMovePath()
        {
            if (currentPath.Count == 0) yield break;

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

            yield return MovePath(worldPath);

            moveCoroutine = null;
            OnEndMove?.Invoke();
            currentPath.Clear();
        }

        IEnumerator MovePath(List<Vector3> worldPath)
        {
            if (worldPath.Count < 2) yield break;

            float totalPathDistance = 0f;

            for (int i = 0; i < worldPath.Count - 1; i++)
            {
                totalPathDistance += Vector3.Distance(worldPath[i], worldPath[i + 1]);
            }

            Debug.Log($"Moving along path with {worldPath.Count} waypoints, total distance: {totalPathDistance}");

            float currentMoveSpeed = totalPathDistance <= lowDistance ? lowSpeed : moveSpeed;

            var orderedSegments = GetOrderedSegments();
            Vector3 lastAnchorPos = orderedSegments[0].transform.position;

            int currentWaypointIndex = 0;

            while (currentWaypointIndex < worldPath.Count - 1)
            {
                Vector3 currentTarget = worldPath[currentWaypointIndex + 1];
                Vector3 anchorPos;

                // Precise movement towards target waypoint
                Vector3 intendedPos = Vector3.MoveTowards(lastAnchorPos, currentTarget, currentMoveSpeed * Time.deltaTime);

                // Clamp theo lưới (head/tail đều dùng được)
                if (gridClamper != null)
                {
                    anchorPos = gridClamper.ClampHeadPosition(intendedPos);
                }
                else
                {
                    anchorPos = intendedPos;
                }

                // Đến waypoint?
                if (Vector3.Distance(anchorPos, currentTarget) < gridClamper.TileCenterThreshold)
                {
                    anchorPos = currentTarget;
                    currentWaypointIndex++;
                }

                if ((anchorPos - lastAnchorPos).sqrMagnitude > (minSampleStep * minSampleStep))
                {
                    AddAnchorSample(anchorPos);
                    lastAnchorPos = anchorPos;
                }

                // Cập nhật vị trí mọi segment theo history
                for (int segIdx = 0; segIdx < orderedSegments.Count; segIdx++)
                {
                    float backDist = segIdx * segmentSpacing;
                    Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                    orderedSegments[segIdx].transform.position = pos;
                }

                occupiedTileController?.UpdateAllSegmentPositions();

                yield return null;
            }

            // Đảm bảo vị trí cuối cùng chính xác
            Vector3 finalAnchor = worldPath[worldPath.Count - 1];
            finalAnchor = gridClamper.ClampHeadPosition(finalAnchor);
            AddAnchorSample(finalAnchor);

            for (int segIdx = 0; segIdx < orderedSegments.Count; segIdx++)
            {
                float backDist = segIdx * segmentSpacing;
                Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                orderedSegments[segIdx].transform.position = pos;
            }
        }

        public void MoveToPortal(Portal portal)
        {
            portal.Disappear();
            gameObject.SetActive(false);
            OccupiedTileController.ClearAllOccupied();
            occupiedTileController.ForceRestoreAll();
            GamePlayManager.Instance.LevelGame.OnBodyMoveToHole(this);
        }

        #region History system
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
        #endregion
    }
}