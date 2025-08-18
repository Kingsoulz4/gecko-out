using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    [RequireComponent(typeof(TubeRenderer))]
    public class BodyRenderer : MonoBehaviour
    {
        private TubeRenderer _renderer;

        [Header("Sharp Corner Settings")]
        [SerializeField] private bool forceSharpCorners = true;
        [SerializeField] private int pointsPerSegment = 5; // Tăng số điểm mỗi segment
        [SerializeField] private float cornerDistance = 0.3f; // Khoảng cách từ góc để tạo L-shape
        [SerializeField] private int cornerExtraPoints = 3; // Số điểm bổ sung tại góc
        [SerializeField] private float cornerThreshold = 0.85f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        List<GeckoSegment> _segments;
        Vector3[] _segmentPositions;

        public void Initialize(List<GeckoSegment> segments)
        {
            _renderer = GetComponent<TubeRenderer>();
            _segments = segments;
            _segmentPositions = new Vector3[_segments.Count];

            ConfigureTubeRendererForSharpCorners();
            UpdateSegmentPositions();
            UpdateRenderer();
        }

        private void ConfigureTubeRendererForSharpCorners()
        {
            if (_renderer == null) return;

            _renderer.normalMode = TubeRenderer.NormalMode.Hard;
            _renderer.edgeCount = Mathf.Max(4, _renderer.edgeCount);
            _renderer.postprocessContinously = false;

            if (debugMode)
            {
                Debug.Log($"TubeRenderer configured: NormalMode={_renderer.normalMode}, EdgeCount={_renderer.edgeCount}");
            }
        }

        private void Update()
        {
            UpdateSegmentPositions();
            UpdateRenderer();
        }

        private void UpdateSegmentPositions()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                _segmentPositions[i] = _segments[i].transform.position;
            }
        }

        private void UpdateRenderer()
        {
            if (forceSharpCorners)
            {
                List<Vector3> rectangularPath = CreateRectangularPath();
                _renderer.points = rectangularPath.ToArray();
            }
            else
            {
                _renderer.points = _segmentPositions;
            }

            _renderer.uvRect = new Rect(0, 0, _renderer.points.Length, 1);
        }

        // PHƯƠNG PHÁP MỚI: Tạo đường đi hoàn toàn vuông góc
        private List<Vector3> CreateRectangularPath()
        {
            List<Vector3> result = new List<Vector3>();

            if (_segmentPositions.Length < 2)
            {
                return _segmentPositions.ToList();
            }

            result.Add(_segmentPositions[0]);

            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Vector3 current = _segmentPositions[i];
                Vector3 next = _segmentPositions[i + 1];

                // Tạo đường vuông góc giữa 2 điểm
                List<Vector3> rectangularSegment = CreateRectangularSegment(current, next, i);

                // Thêm các điểm (bỏ qua điểm đầu để tránh duplicate)
                for (int j = 1; j < rectangularSegment.Count; j++)
                {
                    result.Add(rectangularSegment[j]);
                }
            }

            if (debugMode)
            {
                Debug.Log($"Rectangular path: {_segmentPositions.Length} segments -> {result.Count} points");
            }

            return result;
        }

        // Tạo segment vuông góc giữa 2 điểm
        private List<Vector3> CreateRectangularSegment(Vector3 start, Vector3 end, int segmentIndex)
        {
            List<Vector3> points = new List<Vector3>();
            points.Add(start);

            Vector3 delta = end - start;

            // Phân tích chuyển động thành X và Z riêng biệt
            float deltaX = delta.x;
            float deltaZ = delta.z;
            float deltaY = delta.y; // Giữ nguyên Y

            // TH1: Chuyển động theo 1 trục duy nhất (đường thẳng)
            if (Mathf.Abs(deltaX) < 0.01f || Mathf.Abs(deltaZ) < 0.01f)
            {
                // Đường thẳng - subdivision bình thường
                for (int i = 1; i < pointsPerSegment; i++)
                {
                    float t = (float)i / pointsPerSegment;
                    points.Add(Vector3.Lerp(start, end, t));
                }
            }
            // TH2: Chuyển động theo cả 2 trục (góc cua)
            else
            {
                // Tạo góc vuông: đi theo 1 trục trước, sau đó trục kia

                // Quyết định đi trục nào trước (có thể tùy chỉnh logic này)
                bool goXFirst = Mathf.Abs(deltaX) >= Mathf.Abs(deltaZ);

                if (goXFirst)
                {
                    // Đi X trước, sau đó Z
                    Vector3 cornerPoint = new Vector3(end.x, start.y + deltaY * 0.5f, start.z);

                    // Subdivision từ start đến corner
                    int halfPoints = pointsPerSegment / 2;
                    for (int i = 1; i <= halfPoints; i++)
                    {
                        float t = (float)i / halfPoints;
                        points.Add(Vector3.Lerp(start, cornerPoint, t));
                    }

                    // Subdivision từ corner đến end
                    for (int i = 1; i < pointsPerSegment - halfPoints; i++)
                    {
                        float t = (float)i / (pointsPerSegment - halfPoints);
                        points.Add(Vector3.Lerp(cornerPoint, end, t));
                    }
                }
                else
                {
                    // Đi Z trước, sau đó X
                    Vector3 cornerPoint = new Vector3(start.x, start.y + deltaY * 0.5f, end.z);

                    // Subdivision từ start đến corner
                    int halfPoints = pointsPerSegment / 2;
                    for (int i = 1; i <= halfPoints; i++)
                    {
                        float t = (float)i / halfPoints;
                        points.Add(Vector3.Lerp(start, cornerPoint, t));
                    }

                    // Subdivision từ corner đến end
                    for (int i = 1; i < pointsPerSegment - halfPoints; i++)
                    {
                        float t = (float)i / (pointsPerSegment - halfPoints);
                        points.Add(Vector3.Lerp(cornerPoint, end, t));
                    }
                }
            }

            points.Add(end);
            return points;
        }

        // PHƯƠNG PHÁP THAY THẾ: Sử dụng grid-aligned path
        private List<Vector3> CreateGridAlignedPath()
        {
            List<Vector3> result = new List<Vector3>();

            if (_segmentPositions.Length < 2)
            {
                return _segmentPositions.ToList();
            }

            result.Add(_segmentPositions[0]);

            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Vector3 current = _segmentPositions[i];
                Vector3 next = _segmentPositions[i + 1];

                // Tạo path grid-aligned (chỉ di chuyển theo 1 trục tại 1 thời điểm)
                Vector3 delta = next - current;

                // Nếu có chuyển động theo cả X và Z, tạo góc vuông
                if (Mathf.Abs(delta.x) > 0.01f && Mathf.Abs(delta.z) > 0.01f)
                {
                    // Tạo điểm trung gian để tạo góc L
                    Vector3 intermediatePoint;

                    // Chọn hướng di chuyển dựa vào độ lớn
                    if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.z))
                    {
                        // Di chuyển X trước
                        intermediatePoint = new Vector3(next.x, current.y, current.z);
                    }
                    else
                    {
                        // Di chuyển Z trước  
                        intermediatePoint = new Vector3(current.x, current.y, next.z);
                    }

                    // Thêm điểm trung gian với subdivision
                    AddSubdividedSegment(result, current, intermediatePoint);
                    AddSubdividedSegment(result, intermediatePoint, next);
                }
                else
                {
                    // Đường thẳng
                    AddSubdividedSegment(result, current, next);
                }
            }

            return result;
        }

        // Thêm segment với subdivision
        private void AddSubdividedSegment(List<Vector3> points, Vector3 start, Vector3 end)
        {
            // Bỏ qua điểm start nếu đã có trong list
            for (int i = 1; i <= pointsPerSegment; i++)
            {
                float t = (float)i / pointsPerSegment;
                points.Add(Vector3.Lerp(start, end, t));
            }
        }

        private void OnDrawGizmos()
        {
            if (_segmentPositions == null) return;

            // Vẽ segment positions gốc (RED) - lớn hơn
            Gizmos.color = Color.red;
            for (int i = 0; i < _segmentPositions.Length; i++)
            {
                Gizmos.DrawWireSphere(_segmentPositions[i], 0.2f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(_segmentPositions[i] + Vector3.up * 0.3f, i.ToString());
#endif
            }

            // Vẽ đường nối segments gốc (YELLOW) - đây là đường chéo hiện tại
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Gizmos.DrawLine(_segmentPositions[i], _segmentPositions[i + 1]);
            }

            // Vẽ renderer points sau khi xử lý (GREEN)
            if (_renderer != null && _renderer.points != null)
            {
                Gizmos.color = Color.green;
                var rendererPoints = _renderer.points;

                for (int i = 0; i < rendererPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(rendererPoints[i], 0.05f);
                }

                // Vẽ đường nối renderer points (CYAN) - đây PHẢI là góc vuông
                Gizmos.color = Color.cyan;
                for (int i = 0; i < rendererPoints.Length - 1; i++)
                {
                    Gizmos.DrawLine(rendererPoints[i], rendererPoints[i + 1]);
                }
            }

            // Vẽ dự kiến path vuông góc (WHITE)
            if (forceSharpCorners && _segmentPositions.Length >= 2)
            {
                Gizmos.color = Color.white;
                var rectangularPath = CreateGridAlignedPath();

                for (int i = 0; i < rectangularPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(rectangularPath[i], rectangularPath[i + 1]);
                }
            }
        }

        [ContextMenu("Switch to Grid Aligned Method")]
        public void SwitchToGridAligned()
        {
            // Thay đổi phương thức tạo điểm
            UpdateRenderer();
        }

        [ContextMenu("Test Grid Aligned Path")]
        public void TestGridAlignedPath()
        {
            if (_segmentPositions == null) return;

            var gridPath = CreateGridAlignedPath();
            Debug.Log($"Grid aligned path: {gridPath.Count} points");

            for (int i = 0; i < gridPath.Count; i++)
            {
                Debug.Log($"Point {i}: {gridPath[i]}");
            }
        }
    }
}