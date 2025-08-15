
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

        [SerializeField] private bool forceSharpCorners = true;
        [SerializeField] private float cornerDuplicationDistance = 0.01f; // Khoảng cách duplicate corner points
        [SerializeField] private float cornerThreshold = 0.85f; // Ngưỡng để phát hiện góc (dot product)
        [SerializeField] private bool debugMode = true;

        List<GeckoSegment> _segments;
        Vector3[] _segmentPositions;

        public void Initialize(List<GeckoSegment> segments)
        {
            _renderer = GetComponent<TubeRenderer>();
            _segments = segments;
            _segmentPositions = new Vector3[_segments.Count];

            // Cấu hình TubeRenderer để có góc sắc nhất có thể
            ConfigureTubeRendererForSharpCorners();

            UpdateSegmentPositions();
            UpdateRenderer();
        }

        private void ConfigureTubeRendererForSharpCorners()
        {
            if (_renderer == null) return;

            // Cách 1: Sử dụng NormalMode.Hard để tạo góc cứng
            _renderer.normalMode = TubeRenderer.NormalMode.Hard;

            // Cách 2: Giảm edgeCount để giảm smoothing
            _renderer.edgeCount = Mathf.Max(4, _renderer.edgeCount); // Tối thiểu 4 cạnh

            // Cách 3: Tắt postprocess nếu có
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
                // PHƯƠNG PHÁP MỚI: Tạo points với micro-segments tại góc
                List<Vector3> sharpPoints = CreateMicroSegmentCorners();
                _renderer.points = sharpPoints.ToArray();
            }
            else
            {
                _renderer.points = _segmentPositions;
            }

            _renderer.uvRect = new Rect(0, 0, _renderer.points.Length, 1);
        }

        // PHƯƠNG PHÁP MỚI: Tạo micro-segments tại góc để force sharp corners
        private List<Vector3> CreateMicroSegmentCorners()
        {
            List<Vector3> result = new List<Vector3>();

            if (_segmentPositions.Length < 3)
            {
                return _segmentPositions.ToList();
            }

            // Thêm điểm đầu
            result.Add(_segmentPositions[0]);

            for (int i = 1; i < _segmentPositions.Length - 1; i++)
            {
                Vector3 prev = _segmentPositions[i - 1];
                Vector3 current = _segmentPositions[i];
                Vector3 next = _segmentPositions[i + 1];

                // Tính hướng
                Vector3 dirToPrev = (prev - current).normalized;
                Vector3 dirToNext = (next - current).normalized;

                // Kiểm tra góc
                float dotProduct = Vector3.Dot(-dirToPrev, dirToNext);

                if (dotProduct < cornerThreshold) // Đây là góc cua
                {
                    // Tạo 2 điểm rất gần góc để force sharp corner
                    Vector3 preCorner = current + dirToPrev * cornerDuplicationDistance;
                    Vector3 postCorner = current + dirToNext * cornerDuplicationDistance;

                    result.Add(preCorner);
                    result.Add(current);  // Điểm góc chính
                    result.Add(postCorner);
                }
                else
                {
                    // Đường thẳng - chỉ thêm điểm bình thường
                    result.Add(current);
                }
            }

            // Thêm điểm cuối
            result.Add(_segmentPositions[_segmentPositions.Length - 1]);

            if (debugMode)
            {
                Debug.Log($"Sharp corners: {_segmentPositions.Length} -> {result.Count} points");
            }

            return result;
        }

        // PHƯƠNG PHÁP BỔ SUNG: Override TubeRenderer bằng custom mesh
        public void UseCustomMesh()
        {
            // Tạo custom mesh với góc sắc nét
            var customMesh = CreateSharpCornerMesh();
            GetComponent<MeshFilter>().mesh = customMesh;

            // Tắt TubeRenderer
            _renderer.enabled = false;
        }

        private Mesh CreateSharpCornerMesh()
        {
            // Implement custom mesh generation với góc sắc nét
            // Điều này cho phép kiểm soát hoàn toàn hình dạng

            Mesh mesh = new Mesh();

            // TODO: Implement custom mesh generation
            // - Tạo vertices dọc theo path
            // - Tạo normals vuông góc với segment
            // - Tạo triangles với góc sắc nét

            return mesh;
        }

        private void OnDrawGizmos()
        {
            if (_segmentPositions == null) return;

            // Vẽ segment positions gốc (RED)
            Gizmos.color = Color.red;
            for (int i = 0; i < _segmentPositions.Length; i++)
            {
                Gizmos.DrawWireSphere(_segmentPositions[i], 0.15f);
            }

            // Vẽ đường nối segments (YELLOW - should be sharp)
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _segmentPositions.Length - 1; i++)
            {
                Gizmos.DrawLine(_segmentPositions[i], _segmentPositions[i + 1]);
            }

            // Vẽ renderer points (GREEN)
            if (_renderer != null && _renderer.points != null)
            {
                Gizmos.color = Color.green;
                var rendererPoints = _renderer.points;

                for (int i = 0; i < rendererPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(rendererPoints[i], 0.08f);
                }

                // Vẽ đường nối renderer points (CYAN)
                Gizmos.color = Color.cyan;
                for (int i = 0; i < rendererPoints.Length - 1; i++)
                {
                    Gizmos.DrawLine(rendererPoints[i], rendererPoints[i + 1]);
                }
            }

            // Vẽ góc detection
            if (forceSharpCorners && _segmentPositions.Length >= 3)
            {
                Gizmos.color = Color.magenta;
                for (int i = 1; i < _segmentPositions.Length - 1; i++)
                {
                    Vector3 prev = _segmentPositions[i - 1];
                    Vector3 current = _segmentPositions[i];
                    Vector3 next = _segmentPositions[i + 1];

                    Vector3 dirToPrev = (prev - current).normalized;
                    Vector3 dirToNext = (next - current).normalized;
                    float dot = Vector3.Dot(-dirToPrev, dirToNext);

                    if (dot < cornerThreshold)
                    {
                        // Vẽ góc được detect
                        Gizmos.DrawWireCube(current, Vector3.one * 0.3f);
                    }
                }
            }
        }
    }
}

// GIẢI PHÁP 2: Custom TubeRenderer Settings
// Thêm vào class TubeRenderer (nếu có thể modify)
/*
public class CustomTubeRenderer : TubeRenderer 
{
    [SerializeField] private bool forceSharpCorners = true;
    [SerializeField] private float sharpnessThreshold = 0.9f;
    
    protected override void ProcessPoints(Vector3[] inputPoints)
    {
        if (!forceSharpCorners)
        {
            base.ProcessPoints(inputPoints);
            return;
        }
        
        // Custom processing để giữ góc sắc nét
        var processedPoints = CreateSharpCornerPoints(inputPoints);
        base.ProcessPoints(processedPoints);
    }
    
    private Vector3[] CreateSharpCornerPoints(Vector3[] original)
    {
        // Logic tạo sharp corners
        return original; // Placeholder
    }
}
*/