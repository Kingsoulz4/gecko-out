using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    [RequireComponent(typeof(SplineComputer))]
    public class BodyRenderer : MonoBehaviour
    {
        private SplineComputer _spline;
        private List<GeckoSegment> _segments;

        [Header("Corner Settings")]
        [SerializeField] private float cornerDotThreshold = 0.1f;

        [Header("Debug")]
        [SerializeField] private float gizmoSize = 0.1f;
        [SerializeField] private Color rawColor = Color.green;
        [SerializeField] private Color cornerColor = Color.red;
        [SerializeField] private Color pathColor = Color.white;
        [SerializeField] private float tubeRadius = 0.5f;
        [SerializeField] private GeckoController geckoController;

        private TubeGenerator _tubeGenerator;
        private List<Vector3> _lastRaw = new List<Vector3>();
        private List<Vector3> _lastCorners = new List<Vector3>();
        private List<Vector3> _lastFinal = new List<Vector3>();

        private void Awake()
        {
            _spline = GetComponent<SplineComputer>();
            _tubeGenerator = GetComponent<TubeGenerator>();
            geckoController = GetComponent<GeckoController>();
        }

        public void Initialize(List<GeckoSegment> segments)
        {
            _segments = segments;
            _tubeGenerator.useSplineSize = false;
            _tubeGenerator.size = tubeRadius;
            Debug.Log("Init segments: " + _segments.Count);
            ForceUpdate();
        }



        [ContextMenu("Force Update Spline")]
        public void ForceUpdate()
        {
            if (_segments == null || _segments.Count == 0) return;

            SplinePoint[] splinePoints = new SplinePoint[_segments.Count];
            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 localPos = _spline.transform.InverseTransformPoint(_segments[i].transform.position);

                //Thêm tiny offset để tránh đường thẳng dọc hoàn toàn
                if (i > 0)
                {
                    Vector3 prevPos = splinePoints[i - 1].position;
                    if (Mathf.Abs(localPos.x - prevPos.x) < 0.01f &&
                        Mathf.Abs(localPos.z - prevPos.z) < 0.01f)
                    {
                        // Thêm micro offset
                        localPos.x += 0.001f * i;
                        localPos.z += 0.001f * i;
                    }
                }

                SplinePoint sp = new SplinePoint(localPos);
                sp.normal = Vector3.forward;
                splinePoints[i] = sp;
            }

            _spline.SetPoints(splinePoints, SplineComputer.Space.Local);
            _spline.RebuildImmediate();
            _tubeGenerator.RebuildImmediate();

        }


        private void LateUpdate()
        {
            if (_segments == null || _segments.Count == 0) return;

            // Convert positions to local space và set spline
            SplinePoint[] splinePoints = new SplinePoint[_segments.Count];
            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 localPos = _spline.transform.InverseTransformPoint(_segments[i].transform.position);
                splinePoints[i] = new SplinePoint(localPos);
                splinePoints[i].size = tubeRadius;
            }

            _spline.SetPoints(splinePoints, SplineComputer.Space.Local);
            ForceUpdate();
        }

        private void OnDrawGizmos()
        {
            // Raw points (green)
            Gizmos.color = rawColor;
            foreach (var p in _lastRaw)
                Gizmos.DrawSphere(p, gizmoSize * 0.7f);

            // Corner points (red)
            Gizmos.color = cornerColor;
            foreach (var c in _lastCorners)
                Gizmos.DrawSphere(c, gizmoSize);

            // Final path lines (white)
            Gizmos.color = pathColor;
            for (int i = 0; i < _lastFinal.Count - 1; i++)
                Gizmos.DrawLine(_lastFinal[i], _lastFinal[i + 1]);

            if (_tubeGenerator == null) return;
            var mf = _tubeGenerator.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return;

            // Vẽ bounding box của mesh
            Gizmos.color = Color.green;
            Gizmos.matrix = _tubeGenerator.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size);
        }
    }
}