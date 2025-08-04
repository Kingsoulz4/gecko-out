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
        [Range(1, 5)] [SerializeField] private int smoothIterations = 3;
        List<GeckoSegment> _segments;
        Vector3[] _positions;

        public void Initialize(List<GeckoSegment> segments)
        {
            _renderer = GetComponent<TubeRenderer>();
            _segments = segments;
            _positions = new Vector3[_segments.Count];

            for (int i = 0; i < _segments.Count; i++)
            {
                _positions[i] = _segments[i].transform.position;
            }

            _renderer.uvRect = new Rect(0, 0, segments.Count, 1);
            _renderer.points = _positions;
        }

        private void Update()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                _positions[i] = _segments[i].transform.position;
            }

            _renderer.points = SmoothSnake(_positions.ToList(), smoothIterations).ToArray();
        }

        List<Vector3> SmoothSnake(List<Vector3> points, int iterations)
        {
            List<Vector3> result = new List<Vector3>(points);

            for (int iter = 0; iter < iterations; iter++)
            {
                List<Vector3> newPoints = new List<Vector3>();
                if (result.Count < 2) return result;

                newPoints.Add(result[0]); // Keep the first point

                for (int i = 0; i < result.Count - 1; i++)
                {
                    Vector3 p0 = result[i];
                    Vector3 p1 = result[i + 1];

                    // Calculate points in XZ plane, keep Y as linear interpolation
                    Vector3 Q = new Vector3(
                        0.75f * p0.x + 0.25f * p1.x,
                        Mathf.Lerp(p0.y, p1.y, 0.25f),
                        0.75f * p0.z + 0.25f * p1.z
                    );
                    Vector3 R = new Vector3(
                        0.25f * p0.x + 0.75f * p1.x,
                        Mathf.Lerp(p0.y, p1.y, 0.75f),
                        0.25f * p0.z + 0.75f * p1.z
                    );

                    newPoints.Add(Q);
                    newPoints.Add(R);
                }

                newPoints.Add(result[result.Count - 1]); // Keep the last point
                result = newPoints;
            }

            return result;
        }
    }
}