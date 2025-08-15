using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    [RequireComponent(typeof(TubeRenderer))]
    public class BodyRenderer : MonoBehaviour
    {
        private TubeRenderer _renderer;
        [Range(1, 5)][SerializeField] private int smoothIterations = 3;
        [Range(0.1f, 1f)][SerializeField] private float cornerSmoothness = 0.5f;
        [Range(2, 10)][SerializeField] private int subdivisionPerSegment = 3;
        [Range(0.1f, 0.8f)][SerializeField] private float bulgeAmount = 0.3f;
        [Range(30f, 150f)][SerializeField] private float minAngleForBulge = 60f;

        // Thêm parameter để kiểm soát việc giữ nguyên góc cua
        [Range(0, 3)][SerializeField] private int preserveCornerSegments = 2; // Số segment từ góc cua để giữ nguyên
        [SerializeField] private bool enableCornerPreservation = true; // Toggle để bật/tắt tính năng

        List<GeckoSegment> _segments;
        Vector3[] _originalPositions;
        List<Vector3> _subdivisionPoints;
        List<bool> _isCornerSegment; // Đánh dấu segment nào là góc cua

        public void Initialize(List<GeckoSegment> segments)
        {
            _renderer = GetComponent<TubeRenderer>();
            _segments = segments;
            _originalPositions = new Vector3[_segments.Count];
            _subdivisionPoints = new List<Vector3>();
            _isCornerSegment = new List<bool>();

            UpdateOriginalPositions();
            int totalPoints = CalculateTotalSubdivisionPoints();
            _renderer.uvRect = new Rect(0, 0, totalPoints, 1);

            UpdateSubdivisionPoints();
            _renderer.points = _subdivisionPoints.ToArray();
        }

        private void Update()
        {
            UpdateOriginalPositions();
            DetectCornerSegments(); // Phát hiện góc cua
            UpdateSubdivisionPoints();
            _renderer.points = SmoothSnakeWithCornerPreservation(_subdivisionPoints).ToArray();
        }

        private void UpdateOriginalPositions()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                _originalPositions[i] = _segments[i].transform.position;
            }
        }

        // Phát hiện segment nào đang ở góc cua
        private void DetectCornerSegments()
        {
            _isCornerSegment.Clear();

            for (int i = 0; i < _segments.Count; i++)
            {
                bool isCorner = false;

                if (i > 0 && i < _segments.Count - 1)
                {
                    Vector3 dir1 = (_originalPositions[i] - _originalPositions[i - 1]).normalized;
                    Vector3 dir2 = (_originalPositions[i + 1] - _originalPositions[i]).normalized;
                    float angle = Vector3.Angle(dir1, dir2);

                    // Nếu góc lệch đáng kể từ 180 độ (đường thẳng) thì đây là góc cua
                    isCorner = angle < 170f && angle > 10f;
                }

                _isCornerSegment.Add(isCorner);
            }
        }

        private int CalculateTotalSubdivisionPoints()
        {
            return (_segments.Count - 1) * subdivisionPerSegment + 1;
        }

        private void UpdateSubdivisionPoints()
        {
            _subdivisionPoints.Clear();

            for (int i = 0; i < _segments.Count - 1; i++)
            {
                Vector3 currentPos = _originalPositions[i];
                Vector3 nextPos = _originalPositions[i + 1];

                for (int j = 0; j < subdivisionPerSegment; j++)
                {
                    float t = j / (float)subdivisionPerSegment;
                    Vector3 subdivisionPoint = Vector3.Lerp(currentPos, nextPos, t);
                    _subdivisionPoints.Add(subdivisionPoint);
                }
            }

            _subdivisionPoints.Add(_originalPositions[_originalPositions.Length - 1]);
        }

        // Thuật toán smoothing với tính năng bảo toàn góc cua
        List<Vector3> SmoothSnakeWithCornerPreservation(List<Vector3> points)
        {
            if (points.Count < 3) return points;
            if (!enableCornerPreservation) return SmoothSnakeWithSubdivision(points);

            List<Vector3> smoothedPoints = new List<Vector3>();
            smoothedPoints.Add(points[0]); // Giữ nguyên điểm đầu

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector3 prev = points[i - 1];
                Vector3 current = points[i];
                Vector3 next = points[i + 1];

                bool isMainSegmentPoint = (i % subdivisionPerSegment == 0);

                if (isMainSegmentPoint)
                {
                    int segmentIndex = i / subdivisionPerSegment;

                    // Kiểm tra xem segment này có nằm trong vùng góc cua cần bảo toàn không
                    bool shouldPreserveCorner = ShouldPreserveCornerAtSegment(segmentIndex);

                    if (shouldPreserveCorner)
                    {
                        // Giữ nguyên vị trí gốc cho góc cua
                        smoothedPoints.Add(current);
                    }
                    else
                    {
                        // Áp dụng smoothing bình thường
                        Vector3 smoothedPoint = ApplyAdvancedSmoothing(prev, current, next, i, points);
                        smoothedPoints.Add(smoothedPoint);
                    }
                }
                else
                {
                    // Subdivision points - kiểm tra xem có nằm gần góc cua không
                    int nearestSegmentIndex = Mathf.RoundToInt(i / (float)subdivisionPerSegment);
                    bool nearCorner = ShouldPreserveCornerAtSegment(nearestSegmentIndex);

                    if (nearCorner)
                    {
                        // Áp dụng smoothing rất nhẹ hoặc không smoothing
                        smoothedPoints.Add(current);
                    }
                    else
                    {
                        // Smoothing bình thường
                        Vector3 smoothedPoint = ApplyLightSmoothing(prev, current, next);
                        smoothedPoints.Add(smoothedPoint);
                    }
                }
            }

            smoothedPoints.Add(points[points.Count - 1]); // Giữ nguyên điểm cuối
            return smoothedPoints;
        }

        // Kiểm tra xem segment tại index có nên được bảo toàn góc cua không
        private bool ShouldPreserveCornerAtSegment(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= _isCornerSegment.Count)
                return false;

            // Kiểm tra segment hiện tại và các segment xung quanh
            for (int offset = -preserveCornerSegments; offset <= preserveCornerSegments; offset++)
            {
                int checkIndex = segmentIndex + offset;
                if (checkIndex >= 0 && checkIndex < _isCornerSegment.Count)
                {
                    if (_isCornerSegment[checkIndex])
                    {
                        return true; // Nằm trong vùng ảnh hưởng của góc cua
                    }
                }
            }

            return false;
        }

        private Vector3 ApplyAdvancedSmoothing(Vector3 prev, Vector3 current, Vector3 next, int index, List<Vector3> allPoints)
        {
            Vector3 dir1 = (current - prev).normalized;
            Vector3 dir2 = (next - current).normalized;
            float angle = Vector3.Angle(dir1, dir2);

            if (angle > minAngleForBulge && angle < 150f)
            {
                return CreateBulgePoint(prev, current, next, angle);
            }
            else
            {
                return (prev + current * 2 + next) * 0.25f;
            }
        }

        private Vector3 ApplyLightSmoothing(Vector3 prev, Vector3 current, Vector3 next)
        {
            return Vector3.Lerp(current, (prev + next) * 0.5f, 0.3f);
        }

        private Vector3 CreateBulgePoint(Vector3 prev, Vector3 current, Vector3 next, float angle)
        {
            Vector3 dir1 = (current - prev).normalized;
            Vector3 dir2 = (next - current).normalized;
            Vector3 bisector = (dir1 + dir2).normalized;
            Vector3 outwardNormal = Vector3.Cross(bisector, Vector3.up).normalized;

            if (outwardNormal.magnitude < 0.1f)
            {
                outwardNormal = new Vector3(-bisector.z, 0, bisector.x).normalized;
            }

            float dynamicBulge = bulgeAmount * Mathf.Lerp(0.5f, 1f, (180f - angle) / 120f);
            return current + outwardNormal * dynamicBulge;
        }

        // Thuật toán smoothing cũ để fallback
        List<Vector3> SmoothSnakeWithSubdivision(List<Vector3> points)
        {
            if (points.Count < 3) return points;

            List<Vector3> smoothedPoints = new List<Vector3>();
            smoothedPoints.Add(points[0]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector3 prev = points[i - 1];
                Vector3 current = points[i];
                Vector3 next = points[i + 1];

                bool isMainSegmentPoint = (i % subdivisionPerSegment == 0);

                if (isMainSegmentPoint)
                {
                    Vector3 smoothedPoint = ApplyAdvancedSmoothing(prev, current, next, i, points);
                    smoothedPoints.Add(smoothedPoint);
                }
                else
                {
                    Vector3 smoothedPoint = ApplyLightSmoothing(prev, current, next);
                    smoothedPoints.Add(smoothedPoint);
                }
            }

            smoothedPoints.Add(points[points.Count - 1]);
            return smoothedPoints;
        }

        // Catmull-Rom methods (giữ nguyên như cũ)
        private List<Vector3> SmoothWithCatmullRom(List<Vector3> points)
        {
            if (points.Count < 4) return points;

            List<Vector3> smoothedPoints = new List<Vector3>();
            smoothedPoints.Add(points[0]);

            for (int i = 1; i < points.Count - 2; i++)
            {
                Vector3 p0 = points[i - 1];
                Vector3 p1 = points[i];
                Vector3 p2 = points[i + 1];
                Vector3 p3 = points[i + 2];

                int steps = 4;
                for (int step = 0; step < steps; step++)
                {
                    float t = step / (float)steps;
                    Vector3 point = CatmullRomInterpolate(p0, p1, p2, p3, t);
                    smoothedPoints.Add(point);
                }
            }

            smoothedPoints.Add(points[points.Count - 1]);
            return smoothedPoints;
        }

        Vector3 CatmullRomInterpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (_subdivisionPoints == null || _subdivisionPoints.Count == 0) return;

            // Vẽ subdivision points
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _subdivisionPoints.Count; i++)
            {
                Gizmos.DrawWireSphere(_subdivisionPoints[i], 0.05f);

                // Highlight main segment points
                if (i % subdivisionPerSegment == 0)
                {
                    int segmentIndex = i / subdivisionPerSegment;

                    // Màu khác nhau cho corner segments
                    if (segmentIndex < _isCornerSegment.Count && _isCornerSegment[segmentIndex])
                    {
                        Gizmos.color = Color.red; // Góc cua
                        Gizmos.DrawWireSphere(_subdivisionPoints[i], 0.12f);
                    }
                    else if (ShouldPreserveCornerAtSegment(segmentIndex))
                    {
                        Gizmos.color = Color.yellow; // Vùng ảnh hưởng góc cua
                        Gizmos.DrawWireSphere(_subdivisionPoints[i], 0.10f);
                    }
                    else
                    {
                        Gizmos.color = Color.green; // Segment bình thường
                        Gizmos.DrawWireSphere(_subdivisionPoints[i], 0.08f);
                    }

                    Gizmos.color = Color.yellow;
                }
            }
        }
    }
}