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
        [SerializeField] private SnakeController head;
        List<Transform> _segments;
        Vector3[] _positions;

        private IEnumerator Start()
        {
            _renderer = GetComponent<TubeRenderer>();
            yield return null;
            _segments = head.Segments;
            _positions = new Vector3[_segments.Count];

            for (int i = 0; i < _segments.Count; i++)
            {
                _positions[i] = _segments[i].position;
            }

            _renderer.points = _positions;
        }

        private void Update()
        {
            if (head.Segments.Count != _positions.Length)
            {
                _positions = new Vector3[head.Segments.Count - 1];
                _segments = head.Segments;
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                _positions[i] = _segments[i].position;
            }

            _renderer.points = SmoothSnake(_positions.ToList(), 0.2f, 3).ToArray();
        }

        List<Vector3> SmoothSnake(List<Vector3> points, float radius, int iterations)
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