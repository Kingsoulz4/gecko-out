using Dreamteck.Splines;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    [RequireComponent(typeof(SplineComputer))]
    public class BodyRenderer : MonoBehaviour
    {
        [SerializeField] private SplineComputer _spline;
        [SerializeField] private float tubeRadius = 0.5f;
        [SerializeField] private TubeGenerator _tubeGenerator;

        private List<Segment> _segments;
        private Vector3[] _lastPositions; // Cache để check thay đổi

        private List<BodyPartColorChanger> bodyPartColorChangers = new();

        private BodyController bodyController;
        private BodyController BodyController
        {
            get
            {
                if (bodyController == null)
                {
                    bodyController = GetComponent<BodyController>();
                }
                return bodyController;
            }
        }

        public List<Segment> Segments { get => _segments; }

        public List<BodyPartColorChanger> ListBodyPartChanger { get => bodyPartColorChangers; }

        public void UpdateBodyColor()
        {
            bodyPartColorChangers.ForEach(x => x.UpdateColor(BodyController.BodyData.listColor.First()));
        }

        public void FadeBodyColor(ColorType color)
        {
            bodyPartColorChangers.ForEach(x =>
            {
                if (x.gameObject.activeInHierarchy)
                {
                    x.FadeColor(color, 0.5f);
                }
            });
        }

        public void Initialize(List<Segment> segments)
        {
            //BodyController.DogData.
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

            bodyPartColorChangers = GetComponentsInChildren<BodyPartColorChanger>().ToList();
            UpdateBodyColor();

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
                splinePoints[i] = sp;
            }

            _spline.SetPoints(splinePoints, SplineComputer.Space.Local);
            _spline.RebuildImmediate();
            _tubeGenerator.RebuildImmediate();
        }

        private void LateUpdate()
        {
            if (_segments == null || _segments.Count == 0) return;

            // Check if any segment has moved
            bool hasChanged = false;
            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 currentPos = _segments[i].transform.position;
                if (Vector3.Distance(currentPos, _lastPositions[i]) > 0.01f)
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