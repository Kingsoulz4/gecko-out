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

        [SerializeField] private float tubeRadius = 0.5f;

        private TubeGenerator _tubeGenerator;

        private void Awake()
        {
            _spline = GetComponent<SplineComputer>();
            _tubeGenerator = GetComponent<TubeGenerator>();
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
    }
}