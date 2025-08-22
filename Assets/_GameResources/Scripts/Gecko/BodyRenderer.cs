using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    [RequireComponent(typeof(SplineComputer))]
    public class BodyRenderer : MonoBehaviour
    {
        [SerializeField]private SplineComputer _spline;
        [SerializeField] private float tubeRadius = 0.5f;
        [SerializeField] private TubeGenerator _tubeGenerator;


        private List<GeckoSegment> _segments;
        private Vector3[] _lastPositions; // Cache để check thay đổi

        public void Initialize(List<GeckoSegment> segments)
        {
            _segments = segments;
            _tubeGenerator.useSplineSize = false;
            _tubeGenerator.size = tubeRadius;

            // Initialize position cache
            _lastPositions = new Vector3[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                _lastPositions[i] = segments[i].transform.position;
            }

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
                Vector3 worldPos = _segments[i].transform.position;
                Vector3 localPos = _spline.transform.InverseTransformPoint(worldPos);

                SplinePoint sp = new SplinePoint(localPos);
                sp.normal = Vector3.forward;

                //// Adjust tangent mode for corners
                //if (IsCorner(i))
                //{
                //    sp.type = SplinePoint.Type.Broken; // Allow independent control
                //}
                //else
                //{
                //    sp.type = SplinePoint.Type.SmoothFree;
                //}

                splinePoints[i] = sp;
            }

            _spline.SetPoints(splinePoints, SplineComputer.Space.Local);
            _spline.RebuildImmediate();
            _tubeGenerator.RebuildImmediate();
        }

        private bool IsCorner(int index)
        {
            if (index == 0 || index >= _segments.Count - 1) return false;

            Vector3 pos = _segments[index].transform.position;
            Vector3 prevPos = _segments[index - 1].transform.position;
            Vector3 nextPos = _segments[index + 1].transform.position;

            Vector3 dirIn = (pos - prevPos).normalized;
            Vector3 dirOut = (nextPos - pos).normalized;

            return Vector3.Angle(dirIn, dirOut) > 30f;
        }

        private void LateUpdate()
        {
            if (_segments == null || _segments.Count == 0) return;

            // Check if any segment has moved
            bool hasChanged = false;
            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 currentPos = _segments[i].transform.position;
                if (Vector3.Distance(currentPos, _lastPositions[i]) > 0.001f)
                {
                    _lastPositions[i] = currentPos;
                    hasChanged = true;
                }
            }

            if (hasChanged)
            {
                ForceUpdate();
            }
        }
    }
}