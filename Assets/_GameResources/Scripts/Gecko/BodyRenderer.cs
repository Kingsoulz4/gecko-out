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
        [SerializeField] private int pointsPerSegment = 8; // Tăng số điểm mỗi segment
        [SerializeField] private float cornerSharpness = 0.1f; // Độ sắc nét của góc (0 = sắc nhất)
        [SerializeField] private bool useGridAlignment = true; // Sử dụng grid alignment

        [Header("Advanced Corner Control")]
        [SerializeField] private float minCornerDistance = 0.5f; // Khoảng cách tối thiểu cho góc
        [SerializeField] private bool enforceRightAngles = true; // Bắt buộc góc vuông

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool showCornerDetection = true;

        List<GeckoSegment> _segments;
        Vector3[] _segmentPositions;
        List<Vector3> _processedPoints;

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

            // Cấu hình TubeRenderer để tạo góc sắc nét
            _renderer.normalMode = TubeRenderer.NormalMode.Hard;
            _renderer.edgeCount = Mathf.Max(6, _renderer.edgeCount);
            _renderer.postprocessContinously = false;
            _renderer.calculateTangents = false; // Tắt tangent calculation để tăng performance

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
                if (useGridAlignment)
                {
                    _processedPoints = CreatePerfectRightAngleCorners();
                }
                else
                {
                    _processedPoints = CreateEnhancedRectangularPath();
                }

                _renderer.points = _processedPoints.ToArray();
            }
            else
            {
                _renderer.points = _segmentPositions;
            }

            // Cấu hình UV mapping để tránh stretching
            _renderer.uvRect = new Rect(0, 0, _renderer.points.Length * 0.1f, 1);
        }

        // PHƯƠNG PHÁP MỚI: Tạo góc vuông hoàn hảo với kiểm soát chính xác
        private List<Vector3> CreatePerfectRightAngleCorners()
        {
            List<Vector3> result = new List<Vector3>();

            if (_segmentPositions.Length < 2)
            {
                return _segmentPositions.ToList();
            }

            // Thêm điểm đầu tiên
            result.Add(_segmentPositions[0]);

            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Vector3 current = _segmentPositions[i];
                Vector3 next = _segmentPositions[i + 1];

                Vector3 delta = next - current;

                // Kiểm tra xem có phải góc cua không
                bool isCorner = Mathf.Abs(delta.x) > 0.01f && Mathf.Abs(delta.z) > 0.01f;

                if (isCorner && enforceRightAngles)
                {
                    // Tạo góc vuông hoàn hảo
                    CreatePerfectLShape(result, current, next, i);
                }
                else
                {
                    // Đường thẳng với subdivision
                    CreateStraightSegment(result, current, next);
                }
            }

            if (debugMode)
            {
                Debug.Log($"Perfect right angle path: {_segmentPositions.Length} segments -> {result.Count} points");
                LogCornerAnalysis();
            }

            return result;
        }

        // Tạo hình chữ L hoàn hảo
        private void CreatePerfectLShape(List<Vector3> points, Vector3 start, Vector3 end, int segmentIndex)
        {
            Vector3 delta = end - start;

            // Quyết định hướng cua dựa trên segment index hoặc logic khác
            bool goXFirst = DetermineCornerDirection(delta, segmentIndex);

            Vector3 cornerPoint;
            if (goXFirst)
            {
                // Di chuyển X trước, sau đó Z
                cornerPoint = new Vector3(end.x, start.y + delta.y * 0.5f, start.z);
            }
            else
            {
                // Di chuyển Z trước, sau đó X  
                cornerPoint = new Vector3(start.x, start.y + delta.y * 0.5f, end.z);
            }

            // Đảm bảo khoảng cách tối thiểu
            if (Vector3.Distance(start, cornerPoint) < minCornerDistance)
            {
                Vector3 direction = (cornerPoint - start).normalized;
                cornerPoint = start + direction * minCornerDistance;
            }

            // Tạo đoạn thẳng đầu tiên (start -> corner)
            CreateStraightSegment(points, start, cornerPoint, pointsPerSegment / 2);

            // Tạo đoạn thẳng thứ hai (corner -> end)
            CreateStraightSegment(points, cornerPoint, end, pointsPerSegment - pointsPerSegment / 2);
        }

        // Quyết định hướng cua
        private bool DetermineCornerDirection(Vector3 delta, int segmentIndex)
        {
            // Có thể sử dụng nhiều chiến lược khác nhau:

            // 1. Dựa trên độ lớn của chuyển động
            if (Mathf.Abs(delta.x) != Mathf.Abs(delta.z))
            {
                return Mathf.Abs(delta.x) > Mathf.Abs(delta.z);
            }

            // 2. Alternating pattern
            return segmentIndex % 2 == 0;
        }

        // Tạo đoạn thẳng với subdivision
        private void CreateStraightSegment(List<Vector3> points, Vector3 start, Vector3 end, int subdivisions = -1)
        {
            if (subdivisions == -1) subdivisions = pointsPerSegment;

            for (int i = 1; i <= subdivisions; i++)
            {
                float t = (float)i / subdivisions;
                Vector3 point = Vector3.Lerp(start, end, t);
                points.Add(point);
            }
        }

        // PHƯƠNG PHÁP NÂNG CAO: Enhanced rectangular path với kiểm soát chi tiết
        private List<Vector3> CreateEnhancedRectangularPath()
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

                CreateEnhancedSegment(result, current, next, i);
            }

            return result;
        }

        private void CreateEnhancedSegment(List<Vector3> points, Vector3 start, Vector3 end, int segmentIndex)
        {
            Vector3 delta = end - start;

            // Kiểm tra xem có phải góc cua không
            bool isCorner = Mathf.Abs(delta.x) > 0.01f && Mathf.Abs(delta.z) > 0.01f;

            if (isCorner)
            {
                // Tạo góc vuông với kiểm soát độ sắc nét
                CreateControlledLCorner(points, start, end, segmentIndex);
            }
            else
            {
                // Đường thẳng
                CreateStraightSegment(points, start, end);
            }
        }

        private void CreateControlledLCorner(List<Vector3> points, Vector3 start, Vector3 end, int segmentIndex)
        {
            Vector3 delta = end - start;
            bool goXFirst = DetermineCornerDirection(delta, segmentIndex);

            Vector3 cornerPoint;
            if (goXFirst)
            {
                cornerPoint = new Vector3(end.x, start.y + delta.y * 0.5f, start.z);
            }
            else
            {
                cornerPoint = new Vector3(start.x, start.y + delta.y * 0.5f, end.z);
            }

            // Áp dụng corner sharpness
            if (cornerSharpness > 0)
            {
                Vector3 adjustedCorner = ApplyCornerSharpness(start, cornerPoint, end, cornerSharpness);
                cornerPoint = adjustedCorner;
            }

            // Subdivision cho từng đoạn
            int firstSegmentPoints = pointsPerSegment / 2;
            int secondSegmentPoints = pointsPerSegment - firstSegmentPoints;

            // Đoạn đầu: start -> corner
            for (int i = 1; i <= firstSegmentPoints; i++)
            {
                float t = (float)i / firstSegmentPoints;
                points.Add(Vector3.Lerp(start, cornerPoint, t));
            }

            // Đoạn thứ hai: corner -> end
            for (int i = 1; i <= secondSegmentPoints; i++)
            {
                float t = (float)i / secondSegmentPoints;
                points.Add(Vector3.Lerp(cornerPoint, end, t));
            }
        }

        // Áp dụng độ sắc nét cho góc
        private Vector3 ApplyCornerSharpness(Vector3 start, Vector3 corner, Vector3 end, float sharpness)
        {
            if (sharpness <= 0) return corner;

            // Tính toán điểm "mềm hóa" góc
            Vector3 toCornerFromStart = (corner - start).normalized;
            Vector3 toEndFromCorner = (end - corner).normalized;

            Vector3 bisector = (toCornerFromStart + toEndFromCorner).normalized;

            // Điều chỉnh corner point dựa trên sharpness
            float offset = Vector3.Distance(start, corner) * sharpness * 0.1f;
            return corner + bisector * offset;
        }

        // Phân tích và log thông tin góc cua
        private void LogCornerAnalysis()
        {
            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Vector3 delta = _segmentPositions[i + 1] - _segmentPositions[i];
                bool isCorner = Mathf.Abs(delta.x) > 0.01f && Mathf.Abs(delta.z) > 0.01f;

                if (isCorner)
                {
                    Debug.Log($"Corner detected at segment {i}: delta={delta}, angle≈90°");
                }
            }
        }

        // Kiểm tra xem điểm có phải là góc không
        private bool IsCornerPoint(int pointIndex)
        {
            if (_processedPoints == null || pointIndex <= 0 || pointIndex >= _processedPoints.Count - 1)
                return false;

            Vector3 prev = _processedPoints[pointIndex - 1];
            Vector3 current = _processedPoints[pointIndex];
            Vector3 next = _processedPoints[pointIndex + 1];

            Vector3 dir1 = (current - prev).normalized;
            Vector3 dir2 = (next - current).normalized;

            // Kiểm tra góc giữa 2 hướng
            float angle = Vector3.Angle(dir1, dir2);
            return angle > 45f; // Nếu góc > 45°, coi là corner
        }

        private void OnDrawGizmos()
        {
            if (_segmentPositions == null) return;

            // Vẽ segment positions gốc (RED)
            Gizmos.color = Color.red;
            for (int i = 0; i < _segmentPositions.Length; i++)
            {
                Gizmos.DrawWireSphere(_segmentPositions[i], 0.15f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(_segmentPositions[i] + Vector3.up * 0.3f, $"S{i}");
#endif
            }

            // Vẽ đường nối segments gốc (YELLOW) - đường chéo
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Gizmos.DrawLine(_segmentPositions[i], _segmentPositions[i + 1]);
            }

            // Vẽ processed points (GREEN) - điểm sau xử lý
            if (_processedPoints != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < _processedPoints.Count; i++)
                {
                    float size = IsCornerPoint(i) ? 0.08f : 0.04f;
                    Gizmos.DrawWireSphere(_processedPoints[i], size);
                }

                // Vẽ đường nối processed points (CYAN) - phải là góc vuông
                Gizmos.color = Color.cyan;
                for (int i = 0; i < _processedPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(_processedPoints[i], _processedPoints[i + 1]);
                }
            }

            // Vẽ renderer points nếu khác với processed points (MAGENTA)
            if (_renderer != null && _renderer.points != null && _renderer.points != _processedPoints?.ToArray())
            {
                Gizmos.color = Color.magenta;
                var rendererPoints = _renderer.points;

                for (int i = 0; i < rendererPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(rendererPoints[i], 0.03f);
                }
            }

            // Debug corner detection
            if (showCornerDetection && _segmentPositions.Length >= 2)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < _segmentPositions.Length - 1; i++)
                {
                    Vector3 delta = _segmentPositions[i + 1] - _segmentPositions[i];
                    bool isCorner = Mathf.Abs(delta.x) > 0.01f && Mathf.Abs(delta.z) > 0.01f;

                    if (isCorner)
                    {
                        Vector3 midPoint = (_segmentPositions[i] + _segmentPositions[i + 1]) * 0.5f;
                        Gizmos.DrawWireCube(midPoint, Vector3.one * 0.1f);
                    }
                }
            }
        }
    }
}