using System;
using System.Collections;
using System.Collections.Generic;
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
            _renderer.points = _positions;
        }
    }
}