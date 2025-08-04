using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class GeckoController : MonoBehaviour
    {
        [SerializeField] private int length = 4;
        [SerializeField] private GeckoSegment headPrefab;
        [SerializeField] private float moveTime = 0.2f; // Time to move one segment
        private GeckoSegment _head, _tail;
        public List<GeckoSegment> Segments { private set; get; }
        BodyRenderer _bodyRenderer;

        [SerializeField] private Vector2Int deltaMovement;
        [Range(0, 1f)] [SerializeField] private float mockRatio;
bool isMoving = false;

        private void Start()
        {
            _bodyRenderer = GetComponent<BodyRenderer>();
            Segments = new List<GeckoSegment>();
            _head = Instantiate(headPrefab, transform);
            _head.name = "Head";
            _head.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Segments.Add(_head);

            for (int i = 1; i < length; i++)
            {
                GeckoSegment segment = new GameObject("Segment_" + i).AddComponent<GeckoSegment>();
                segment.transform.SetParent(transform);
                segment.transform.localPosition = new Vector3(0, -i, 0);
                Segments.Add(segment);
            }

            _head = Segments[0];
            _tail = Segments[Segments.Count - 1];

            for (int i = 1; i < Segments.Count; i++)
            {
                var currentSegment = Segments[i];
                GeckoSegment prevSegment = Segments[i - 1];
                if (i == Segments.Count - 1)
                {
                    currentSegment.Setup(prevSegment, null); // Last segment has no next segment
                }
                else
                {
                    GeckoSegment nextSegment = Segments[i];
                    currentSegment.Setup(prevSegment, nextSegment);
                }
            }

            _bodyRenderer.Initialize(Segments);

            for (int i = 0; i < Segments.Count; i++)
            {
                var coordinate = new Vector2Int(0, GameMap.MapSize.y - i - 1);
                Segments[i].SetCoordinate(coordinate);
            }
        }

        private void Update()
        {
            Vector2Int delta = GetDeltaMovement();
            if (delta != Vector2Int.zero)
            {
                MoveHead(delta);
            }

            // MoveHead(deltaMovement, mockRatio);
        }

        private void MoveHead(Vector2Int delta, float ratio)
        {
            Vector2Int newHeadCoordinate = _head.Coordinate + delta;
            bool isMoveForward = !(Segments[1].Coordinate == newHeadCoordinate);
            GameMap.TryGetTileAt(_head.Coordinate, out var headTile);
            if (GameMap.TryGetTileAt(newHeadCoordinate, out var tile))
            {
                Vector3 startPosition = headTile.transform.position;
                Vector3 targetPosition = tile.transform.position;

                float elapsedTime = 0f;
                _head.transform.position = Vector3.Lerp(startPosition, targetPosition, ratio);
                for (int i = 1; i < Segments.Count; i++)
                {
                    Segments[i].Move(isMoveForward, ratio);
                }

                if (ratio >= 1f)
                {
                    // Ensure final position is set
                    for (int i = Segments.Count - 1; i >= 1; i--)
                    {
                        Segments[i].SetCoordinate(Segments[i - 1].Coordinate);
                    }

                    _head.SetCoordinate(newHeadCoordinate);
                }
            }
            else
            {
                Debug.LogWarning("Invalid move: " + newHeadCoordinate);
            }
        }

        private void MoveHead(Vector2Int delta)
        {
            Vector2Int newHeadCoordinate = _head.Coordinate + delta;
            bool isMoveForward = !(Segments[1].Coordinate == newHeadCoordinate);

            if (GameMap.TryGetTileAt(newHeadCoordinate, out var tile))
            {
                StartCoroutine(MoveHeadToPosition(newHeadCoordinate, tile.transform.position, isMoveForward));
            }
            else
            {
                Debug.LogWarning("Invalid move: " + newHeadCoordinate);
            }
        }

        IEnumerator MoveHeadToPosition(Vector2Int newCoordinate, Vector2 newWorldPosition, bool isMoveForward)
        {
            isMoving = true;
            GameMap.TryGetTileAt(_head.Coordinate, out var headTile);
            Vector3 startPosition = headTile.transform.position;
            Vector3 targetPosition = newWorldPosition;

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float ratio = Mathf.Clamp01(elapsedTime / moveTime);
                _head.transform.position = Vector3.Lerp(startPosition, targetPosition, ratio);
                for (int i = 1; i < Segments.Count; i++)
                {
                    Segments[i].Move(isMoveForward, ratio);
                }

                yield return null;
            }

            // Ensure final position is set
            for (int i = Segments.Count - 1; i >= 1; i--)
            {
                Segments[i].SetCoordinate(Segments[i - 1].Coordinate);
            }

            _head.SetCoordinate(newCoordinate);
            
            isMoving = false;
        }

        private Vector2Int GetDeltaMovement()
        {
            Vector2Int delta = Vector2Int.zero;
            if(isMoving) 
                return delta;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                delta = Vector2Int.up;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                delta = Vector2Int.down;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                delta = Vector2Int.left;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                delta = Vector2Int.right;
            }

            return delta;
        }
    }
}