using Dreamteck.Splines;
using Geckout.Data;
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
        [SerializeField] private Segment headPrefab;
        [SerializeField] private Segment segment;
        [SerializeField] private Segment tailPrefab;
        [SerializeField] private OccupiedTileController occupiedTileController;
        [SerializeField] private BodyRenderer _bodyRenderer;
        [SerializeField] private GridHeadClamper gridClamper;
        [SerializeField] private MoveToPortal moveToPortal;
        [SerializeField] private SplineComputer splineComputer;
        [SerializeField] private IceBody iceBody;
        [SerializeField] private HiddenBody hiddenBody;
        [SerializeField] private MultipleColorBody doubleColorBody;
        [SerializeField] private Segment _head, _tail;
        [SerializeField] private BodySelected bodySelectedIcon;

        [Header("Mechanics")]
        [SerializeField] private MechanicsReferences m_mechanicReferences;

        public ControlAnchor controlAnchor = ControlAnchor.Head;

        // Movement events
        public Action OnStartMove;
        public Action OnEndMove;

        private readonly int subLength = 3;
        private float historyTotalLength = 0f;
        private const float extraHistoryPadding = 4f;
        private Coroutine moveCoroutine;
        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private LinkedList<Vector3> historyPoints = new LinkedList<Vector3>();
        private List<Vector3> worldPath = new List<Vector3>();
        private float segmentSpacing;
        private bool canControl = true;
        private bool canMovePortal = true;
        private List<MechanicRendererBase> listMechanicRender = new();
        private bool isMoving;
        private Crate crateContainer;

        public List<Segment> Segments { private set; get; }
        public OccupiedTileController OccupiedTileController { get => occupiedTileController; set => occupiedTileController = value; }
        public bool IsMoving
        {
            get
            {
                return isMoving;
            }
        }
        public GridHeadClamper GridClamper { get => gridClamper; set => gridClamper = value; }
        public BodyData BodyData { get; set; }
        public int SubLength => subLength;
        public MoveToPortal MoveToPortal { get => moveToPortal; }
        public bool CanControl
        {
            get
            {
                return canControl && iceBody.CurrentCount <= 0 && (crateContainer == null || !crateContainer.gameObject.activeInHierarchy);
            }

            set => canControl = value;
        }
        public MechanicsReferences MechanicReferences { get => m_mechanicReferences; }
        public List<MechanicRendererBase> ListMechanicRender { get => listMechanicRender; set => listMechanicRender = value; }
        public bool CanMovePortal
        {
            get
            {
                return canMovePortal && hiddenBody.CurrentCount <= 0;
            }
        }
        public BodyRenderer BodyRenderer { get => _bodyRenderer; }
        public int Length { get => length; }
        public SplineComputer SplineComputer { get => splineComputer; }
        public BodySelected BodySelectedIcon { get => bodySelectedIcon; set => bodySelectedIcon = value; }

#if UNITY_EDITOR
        [EditorButton]
        private void SetRef()
        {
            if (_bodyRenderer == null)
            {
                _bodyRenderer = GetComponentInChildren<BodyRenderer>();
            }
            if (occupiedTileController == null)
            {
                occupiedTileController = GetComponent<OccupiedTileController>();
            }
            if (gridClamper == null)
            {
                gridClamper = GetComponent<GridHeadClamper>();
            }
            if (moveToPortal == null)
            {
                moveToPortal = GetComponent<MoveToPortal>();
            }
            if (m_mechanicReferences == null)
            {
                m_mechanicReferences = Resources.Load<MechanicsReferences>("Mechanics/MechanicsReferences");
            }
            if (iceBody == null)
            {
                iceBody = FindUlti.FindChildDirect(transform, "IceBody").GetComponent<IceBody>();
            }
            if (hiddenBody == null)
            {
                hiddenBody = FindUlti.FindChildDirect(transform, "HiddenBody").GetComponent<HiddenBody>();
            }
            if (doubleColorBody == null)
            {
                doubleColorBody = FindUlti.FindChildDirect(transform, "DoubleColorBody").GetComponent<MultipleColorBody>();
            }
        }
#endif

        private void Update()
        {
            //Debug.Log("IsMoving " + IsMoving);
        }

        public virtual void Initialize(BodyData dogData)
        {
            BodyData = dogData;
            List<Vector2Int> listDefaultCoordinate = dogData.listCoordinate;

            ResetAllComponents();

            // ===== Tính toán tổng số segment =====
            length = listDefaultCoordinate.Count;
            int totalSegments = length * SubLength - 2;
            float unitSpacing = 1f / SubLength;
            segmentSpacing = unitSpacing;
            float totalBodyLength = length;

            // ===== Head =====
            if (_head == null)
            {
                _head = Instantiate(headPrefab, transform);
            }
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
            if (_tail == null)
            {
                _tail = Instantiate(tailPrefab, transform);
            }
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

            for (int i = 0; i < Segments.Count; i++)
            {
                // unitIndex = segment thuộc về tile nào
                //int unitIndex = Mathf.CeilToInt((float)i / SubLength);
                int unitIndex = i/subLength;

                var coordinate = listDefaultCoordinate[Mathf.Clamp(unitIndex, 0, listDefaultCoordinate.Count - 1)];

                if (i % SubLength == 0)
                {
                    Segments[i].InitCoordinate(coordinate);
                }
                else
                {
                    var temp = i % SubLength;
                    var previosCoord = i / SubLength;
                    var nextCoord = previosCoord + 1;
                    GameMap.TryGetTileAtCoord(listDefaultCoordinate[previosCoord], out var tilePreviosCoord);
                    GameMap.TryGetTileAtCoord(listDefaultCoordinate[nextCoord], out var tileNextCoord);
                    Segments[i].Coordinate = coordinate;
                    Segments[i].transform.position = Vector3.Lerp(tilePreviosCoord.transform.position, tileNextCoord.transform.position, (float)temp / SubLength);
                }
            }

            // ===== Init history system =====
            InitHistoryFromSegments();

            // ===== Initialize renderer =====
            if (_bodyRenderer != null)
                _bodyRenderer.Initialize(Segments);

            InitMechanic();
            occupiedTileController?.Init();
            canControl = true;

            Debug.Log($"Body initialized: length={length}, subLength={SubLength}, totalSegments={totalSegments}, totalBodyLength={totalBodyLength}");
        }

        public void ResetAllComponents()
        {
            occupiedTileController.ClearAllOccupied();
            if (Segments != null && Segments.Count >= 3)
            {
                foreach (var seg in Segments)
                {
                    if (seg != _head && seg != _tail)
                    {
                        Destroy(seg.gameObject);
                    }
                }
                Segments.Clear();
            }
            Segments = new List<Segment>();
        }
        public List<Segment> GetOrderedSegments()
        {
            if (controlAnchor == ControlAnchor.Head)
                return Segments;
            else
                return Segments.AsEnumerable().Reverse().ToList();
        }

        public void StartMovePath(List<Vector2Int> path, bool isMovingPortal = false)
        {
            if (path == null || path.Count == 0) return;

            StopMoveCoroutine();

            currentPath = new List<Vector2Int>(path);

            // Set path for GridHeadClamper if present and controlling head
            if (gridClamper != null)
            {
                gridClamper.SetPath(path);
            }

            moveCoroutine = StartCoroutine(StartMovePathIE(isMovingPortal));
        }

        public void StopMoveCoroutine()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            UpdateAllSegmentPos();
        }

        public void SetControlAnchor(ControlAnchor anchor)
        {
            if (controlAnchor != anchor)
            {
                controlAnchor = anchor;

                // Rebuild history to match new control direction
                InitHistoryFromSegments();
                Debug.Log("SetAnchor " + anchor);
            }
        }

        public void ClearPath()
        {
            StopMoveCoroutine();
            currentPath.Clear();
        }

        IEnumerator StartMovePathIE(bool isMovingPortal = false)
        {
            if (currentPath.Count == 0) yield break;

            OnStartMove?.Invoke();
            worldPath.Clear();
            isMoving = true;

            // Get current anchor position (always the leading segment in ordered view)
            var orderedSegments = GetOrderedSegments();
            worldPath.Add(orderedSegments[0].transform.position);

            Debug.Log($"Control Anchor: {controlAnchor} Move Along Path:" + String.Join(';', currentPath.Select(x => x.ToString())));

            foreach (var coord in currentPath)
            {
                if (GameMap.TryGetTileAtCoord(coord, out var tile))
                    worldPath.Add(tile.transform.position);

                AddCoordinate(coord);

                UpdateAllSegmentPos();
     
            }

            //yield return MovePath(worldPath, isMovingPortal);

            moveCoroutine = null;
            isMoving = false;
            Debug.Log("endMove");
            OnEndMove?.Invoke();
            currentPath.Clear();

            UpdateAllSegmentPos();
        }

        IEnumerator MovePath(List<Vector3> worldPath, bool isMovingToPortal = false)
        {
            if (worldPath.Count < 2) yield break;

            float totalPathDistance = 0f;

            for (int i = 0; i < worldPath.Count - 1; i++)
            {
                totalPathDistance += Vector3.Distance(worldPath[i], worldPath[i + 1]);
            }

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

                anchorPos = gridClamper.ClampHeadPosition(intendedPos);
                //anchorPos = intendedPos;

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
                else
                {
                    AddAnchorSample(intendedPos);
                    lastAnchorPos = intendedPos;
                }


                // Cập nhật vị trí mọi segment theo history
                for (int segIdx = 0; segIdx < orderedSegments.Count; segIdx++)
                {
                    if (orderedSegments[segIdx].IsPortalAnimating) continue;

                    float backDist = segIdx * segmentSpacing;
                    Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                    orderedSegments[segIdx].transform.position = pos;

                    if (isMovingToPortal)
                    {
                        CheckAndTriggerPortalAnimation(orderedSegments[segIdx], segIdx, orderedSegments.Count);
                    }
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
        private void CheckAndTriggerPortalAnimation(Segment segment, int segmentIndex, int totalSegments)
        {
            if (!moveToPortal.IsEnteringPortal) return;
            if (moveToPortal.TargetPortal == null) return;

            Vector3 portalCenter = moveToPortal.TargetPortal.transform.position;
            float distanceToPortal = Vector3.Distance(segment.transform.position, portalCenter);

            if (distanceToPortal <= moveToPortal.PortalEnterDistance)
            {
                //Debug.Log($"Animating segment {segmentIndex} into portal");

                // Mark segment as animating
                segment.IsPortalAnimating = true;

                bool isLastSegment = (segmentIndex == totalSegments - 1);

                // Trigger animation
                StartCoroutine(moveToPortal.AnimateSegmentDown(segment, portalCenter, isLastSegment));
            }
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


        #region Visualize
        public void InitMechanic()
        {
            if (listMechanicRender.Count > 0)
            {
                foreach (var item in listMechanicRender)
                {
                    Destroy(item.gameObject);
                }
                listMechanicRender.Clear();
            }

            if (BodyData.freezeTimeCount > 0)
            {
                iceBody.Init(BodyData.freezeTimeCount);
            }

            doubleColorBody.gameObject.SetActive(BodyData.listColor.Count > 1);
            if (BodyData.listColor.Count > 1)
            {
                doubleColorBody.Init();
            }

            if (BodyData.hiddenCount > 0)
            {
                hiddenBody.Init(BodyData.hiddenCount, SplineComputer);
                doubleColorBody.gameObject.SetActive(false);
            }

            if (GameMap.TryGetCratesAtCoord(BodyData.listCoordinate.FirstOrDefault(), out var listCrate))
            {
                crateContainer = listCrate.FirstOrDefault();
            }
        }

        public void ShowMultiSubColorColor()
        {
            doubleColorBody.gameObject.SetActive(true);
        }

        #endregion

        #region Booster
        public void CutOutLastSegment()
        {
            var bodyData = new BodyData(BodyData);
            bodyData.listCoordinate.Remove(bodyData.listCoordinate.Last());
            Initialize(bodyData);
        }

        public bool CanBeAffectedByBoosterMagicWand()
        {
            return BodyData.listCoordinate.Count >= 3 && BodyData.freezeTimeCount <= 0 && BodyData.hiddenCount <= 0 && CanControl;
        }
            

        #endregion
    }
}