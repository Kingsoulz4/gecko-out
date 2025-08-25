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
        [SerializeField] private int length = 4;
        [SerializeField] private Segment headPrefab;
        [SerializeField] private Segment segment;
        [SerializeField] private float moveSpeed = 5f; // tốc độ di chuyển (units/giây)
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float outerSmoothness = 0.7f;
        [SerializeField] private float cornerRadius = 0.3f;
        [SerializeField] private OccupiedTileController occupiedTileController;
        [SerializeField] private BodyRenderer _bodyRenderer;

        private Segment _head, _tail;
        public List<Segment> Segments { private set; get; }
        public bool IsMoving { get => isMoving; }
        public OccupiedTileController OccupiedTileController { get => occupiedTileController; set => occupiedTileController = value; }

        bool isMoving = false;

        // Path movement
        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private Coroutine moveCoroutine;

        // Khoảng cách giữa các segment
        private float segmentSpacing = 1f;

        private LinkedList<Vector3> historyPoints = new LinkedList<Vector3>();
        private float historyTotalLength = 0f;
        private const float minSampleStep = 0.1f;     // bước tối thiểu thêm mẫu vào history
        private const float extraHistoryPadding = 4f;  // đệm thêm sau tổng chiều dài thân

        private void Start()
        {
            Segments = new List<Segment>();
            _head = Instantiate(headPrefab, transform);
            _head.name = "Head";
            _head.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Segments.Add(_head);

            for (int i = 1; i < length; i++)
            {
                Segment segment = Instantiate(this.segment);
                segment.name = "Segment " + i;
                segment.transform.SetParent(transform);
                segment.transform.localPosition = new Vector3(0, -i, 0);
                Segments.Add(segment);
            }

            _head = Segments[0];
            _head.Setup(null, Segments[1]);
            _head.SetController(this);
            _tail = Segments[Segments.Count - 1];
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

            for (int i = 0; i < Segments.Count; i++)
            {
                var coordinate = new Vector2Int(0, GameMap.MapSize.y - i - 1);
                Segments[i].SetCoordinate(coordinate);
            }

            // Auto-calc spacing từ khoảng cách head -> segment kế
            if (Segments.Count > 1)
            {
                segmentSpacing = Vector3.Distance(
                    Segments[0].transform.position,
                    Segments[1].transform.position
                );
            }

            // Seed history từ dáng hiện tại (tail -> head)
            InitHistoryFromSegments();

            _bodyRenderer.Initialize(Segments);
        }

        // Set path và bắt đầu di chuyển liên tục
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

            // World path: từ vị trí head hiện tại + các tile target
            List<Vector3> worldPath = new List<Vector3> { _head.transform.position };
            foreach (var coord in currentPath)
            {
                if (GameMap.TryGetTileAt(coord, out var tile))
                {
                    worldPath.Add(tile.transform.position);
                }
            }

            yield return SmoothPathMovement_History(worldPath, currentPath);

            isMoving = false;
            currentPath.Clear();
        }

        //  Movement kiểu history
        IEnumerator SmoothPathMovement_History(List<Vector3> worldPath, List<Vector2Int> coordPath)
        {
            if (worldPath.Count < 2) yield break;

            // Chuẩn bị độ dài từng đoạn để lấy vị trí head theo distance
            float totalPathLength = 0f;
            List<float> segmentLengths = new List<float>();
            for (int i = 0; i < worldPath.Count - 1; i++)
            {
                float length = Vector3.Distance(worldPath[i], worldPath[i + 1]);
                segmentLengths.Add(length);
                totalPathLength += length;
            }

            float headDistance = 0f;

            // đặt head hiện tại để tránh jump
            Vector3 lastHeadPos = _head.transform.position;

            while (headDistance < totalPathLength)
            {
                // head di chuyển theo path
                headDistance += moveSpeed * Time.deltaTime;
                Vector3 headPos = GetPointAtDistanceOnWorldPath(worldPath, segmentLengths, headDistance);
                if ((headPos - lastHeadPos).sqrMagnitude > (minSampleStep * minSampleStep))
                {
                    AddHeadSample(headPos);
                    lastHeadPos = headPos;
                }

                // Cập nhật transform của từng segment dựa trên history
                for (int segIdx = 0; segIdx < Segments.Count; segIdx++)
                {
                    float backDist = segIdx * segmentSpacing;
                    Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                    Segments[segIdx].transform.position = pos;
                }

                yield return null;
            }

            // Bảo đảm head tới đúng cuối path + thêm mẫu cuối
            Vector3 finalHead = worldPath[worldPath.Count - 1];
            AddHeadSample(finalHead);

            for (int segIdx = 0; segIdx < Segments.Count; segIdx++)
            {
                float backDist = segIdx * segmentSpacing;
                Vector3 pos = GetHistoryPointAtDistanceBack(backDist);
                Segments[segIdx].transform.position = pos;
            }
        }

        // Lấy điểm head theo distance dọc worldPath
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

        private float RequiredHistoryLength()
        {
            return Mathf.Max(0f, (Segments.Count - 1) * segmentSpacing + extraHistoryPadding);
        }

        private void InitHistoryFromSegments()
        {
            historyPoints.Clear();
            historyTotalLength = 0f;

            // thêm tail đầu tiên
            historyPoints.AddLast(Segments[Segments.Count - 1].transform.position);

            // đi theo polyline từ tail -> head, thêm mẫu đều theo minSampleStep
            for (int i = Segments.Count - 2; i >= 0; i--)
            {
                Vector3 from = historyPoints.Last.Value;
                Vector3 to = Segments[i].transform.position;
                AppendSegmentSamples(from, to);
            }

            TrimHistory(); // cắt bớt nếu dư
        }

        private void AppendSegmentSamples(Vector3 from, Vector3 to)
        {
            float segmentLen = Vector3.Distance(from, to);
            if (segmentLen <= Mathf.Epsilon)
            {
                // nếu 2 điểm trùng, vẫn nên đảm bảo có to là điểm cuối
                if ((historyPoints.Last.Value - to).sqrMagnitude > 1e-6f)
                {
                    historyTotalLength += Vector3.Distance(historyPoints.Last.Value, to);
                    historyPoints.AddLast(to);
                }
                return;
            }

            // số mẫu chen thêm (mỗi bước ~minSampleStep)
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
            if (d < minSampleStep) return; // quá gần, bỏ qua để giảm nhiễu

            historyPoints.AddLast(headPos);
            historyTotalLength += d;

            TrimHistory();
        }

        private void TrimHistory()
        {
            float need = RequiredHistoryLength();

            // giữ đủ chiều dài để cover cả thân + padding
            while (historyPoints.Count > 1 && historyTotalLength > need)
            {
                // bớt từ đầu (tail)
                Vector3 first = historyPoints.First.Value;
                Vector3 second = historyPoints.First.Next.Value;
                float seg = Vector3.Distance(first, second);

                historyPoints.RemoveFirst();
                historyTotalLength -= seg;
            }
        }

        private Vector3 GetHistoryPointAtDistanceBack(float backDistance)
        {
            // Đi lùi từ head (Last) về phía tail cho đến khi gom đủ backDistance
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

            // nếu vượt quá chiều dài history, trả về điểm đầu tiên (tail)
            return historyPoints.First.Value;
        }
    }
}
