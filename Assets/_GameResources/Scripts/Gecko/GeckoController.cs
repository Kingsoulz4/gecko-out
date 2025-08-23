using Dreamteck.Splines;
using Geckout.Generals;
using Geckout.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class GeckoController : MonoBehaviour
    {
        [SerializeField] private int length = 4;
        [SerializeField] private GeckoSegment headPrefab;
        [SerializeField] private float moveTime = 0.2f;
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float outerSmoothness = 0.7f;
        [SerializeField] private float cornerRadius = 0.3f;

        private GeckoSegment _head, _tail;
        public List<GeckoSegment> Segments { private set; get; }
        public bool IsMoving { get => isMoving; }

        BodyRenderer _bodyRenderer;
        bool isMoving = false;

        // Touch input integration
        private Queue<Vector2Int> moveQueue = new Queue<Vector2Int>();
        private bool useKeyboardInput = false; // Toggle for testing

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
            _head.Setup(null, Segments[1]);
            _tail.Setup(Segments[Segments.Count - 2], null);

            for (int i = 1; i < Segments.Count - 1; i++)
            {
                var currentSegment = Segments[i];
                GeckoSegment prevSegment = Segments[i - 1];
                GeckoSegment nextSegment = Segments[i + 1];
                currentSegment.Setup(prevSegment, nextSegment);
                currentSegment.SetCorner(outerSmoothness, cornerRadius);
            }

            for (int i = 0; i < Segments.Count; i++)
            {
                var coordinate = new Vector2Int(0, GameMap.MapSize.y - i - 1);
                Segments[i].SetCoordinate(coordinate);
            }

            _bodyRenderer.Initialize(Segments);
        }

        private void Update()
        {
            ProcessMoveQueue();
        }

        public void QueueMove(Vector2Int delta)
        {
            moveQueue.Enqueue(delta);
            Debug.Log($"[GeckoController] Move queued: {delta}, Total queue: {moveQueue.Count}");
        }

        public void ClearMoveQueue()
        {
            int oldCount = moveQueue.Count;
            moveQueue.Clear();
            Debug.Log($"[GeckoController] Queue cleared, was {oldCount} moves");
        }

        void ProcessMoveQueue()
        {
            if (!isMoving && moveQueue.Count > 0)
            {
                Vector2Int nextMove = moveQueue.Dequeue();
                Debug.Log($"[GeckoController] Processing move: {nextMove}, Remaining: {moveQueue.Count}");
                StartCoroutine(ExecuteMove(nextMove));
            }
        }

        // Public wrapper for external access - trả về IEnumerator
        public IEnumerator MoveHeadCoroutine(Vector2Int delta)
        {
            yield return ExecuteMove(delta);
        }

        // Alternative: Direct call without coroutine return
        public void MoveHeadDirect(Vector2Int delta)
        {
            QueueMove(delta);
        }

        IEnumerator ExecuteMove(Vector2Int delta)
        {
            if (isMoving) yield break; // Prevent overlapping moves

            Vector2Int newHeadCoordinate = _head.Coordinate + delta;
            var tailDirection = _tail.Coordinate - _tail.PrevSegment.Coordinate;
            Vector2Int newTailCoordinate = _tail.Coordinate + tailDirection;

            bool isMoveForward = !(Segments[1].Coordinate == newHeadCoordinate);

            if (isMoveForward)
            {
                if (GameMap.TryGetTileAt(newHeadCoordinate, out var tile))
                {
                    yield return MoveHeadToPosition(newHeadCoordinate, tile.transform.position);
                }
            }
            else
            {
                if (GameMap.TryGetTileAt(newTailCoordinate, out var tile))
                {
                    yield return MoveTailToPosition(newTailCoordinate, tile.transform.position);
                }
                else
                {
                    var orthogonalVectors = GetOrthogonalUnitVectors(tailDirection);
                    foreach (var orthogonalVector in orthogonalVectors)
                    {
                        var newTailPosition = _tail.Coordinate + orthogonalVector;
                        if (GameMap.TryGetTileAt(newTailPosition, out var orthogonalTile))
                        {
                            yield return MoveTailToPosition(newTailPosition, orthogonalTile.transform.position);
                            break;
                        }
                    }
                }
            }
        }

        IEnumerator MoveHeadToPosition(Vector2Int newCoordinate, Vector3 newWorldPosition)
        {
            isMoving = true;
            GameMap.TryGetTileAt(_head.Coordinate, out var headTile);
            Vector3 startPosition = headTile.transform.position;
            Vector3 targetPosition = newWorldPosition;

            Vector2Int moveDirection = newCoordinate - _head.Coordinate;
            bool isCornerTurn = IsCornerTurn(moveDirection);

            for (int i = 1; i < Segments.Count; i++)
            {
                Segments[i].Move(true, 0f);
            }

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveTime);
                float ratio = movementCurve.Evaluate(t);

                if (isCornerTurn)
                {
                    ratio = ApplyCornerSmoothing(ratio);
                }

                _head.transform.position = Vector3.Lerp(startPosition, targetPosition, ratio);

                for (int i = 1; i < Segments.Count; i++)
                {
                    Segments[i].Move(true, ratio);
                }

                yield return null;
            }

            _head.transform.position = targetPosition;
            for (int i = 1; i < Segments.Count; i++)
            {
                Segments[i].Move(true, 1f);
            }

            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                Segments[i].ReleaseCurrentTile();
            }

            for (int i = Segments.Count - 1; i >= 1; i--)
            {
                Segments[i].SetCoordinate(Segments[i - 1].Coordinate);
            }

            _head.SetCoordinate(newCoordinate);
            isMoving = false;
        }

        IEnumerator MoveTailToPosition(Vector2Int newCoordinate, Vector3 newWorldPosition)
        {
            isMoving = true;
            GameMap.TryGetTileAt(_tail.Coordinate, out var tailTile);
            Vector3 startPosition = tailTile.transform.position;
            Vector3 targetPosition = newWorldPosition;

            Vector2Int moveDirection = newCoordinate - _tail.Coordinate;
            bool isCornerTurn = IsCornerTurn(moveDirection);

            for (int i = Segments.Count - 2; i >= 0; i--)
            {
                Segments[i].Move(false, 0f);
            }

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveTime);
                float ratio = movementCurve.Evaluate(t);

                if (isCornerTurn)
                {
                    ratio = ApplyCornerSmoothing(ratio);
                }

                _tail.transform.position = Vector3.Lerp(startPosition, targetPosition, ratio);

                for (int i = Segments.Count - 2; i >= 0; i--)
                {
                    Segments[i].Move(false, ratio);
                }

                yield return null;
            }

            _tail.transform.position = targetPosition;
            for (int i = Segments.Count - 2; i >= 0; i--)
            {
                Segments[i].Move(false, 1f);
            }

            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                Segments[i].ReleaseCurrentTile();
            }

            for (int i = 0; i < Segments.Count - 1; i++)
            {
                Segments[i].SetCoordinate(Segments[i + 1].Coordinate);
            }

            _tail.SetCoordinate(newCoordinate);
            isMoving = false;
        }

        private bool IsCornerTurn(Vector2Int currentDirection)
        {
            return true;
        }

        private float ApplyCornerSmoothing(float ratio)
        {
            return Mathf.SmoothStep(0f, 1f, ratio);
        }

        public List<Vector2Int> GetOrthogonalUnitVectors(Vector2Int input)
        {
            var unitVectors = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1)
            };

            var result = new List<Vector2Int>();
            foreach (var v in unitVectors)
            {
                if (v == input || v == -input) continue;
                if (Vector2.Dot(input, v) == 0)
                    result.Add(v);
            }
            return result;
        }

        // Public method to toggle input mode
        public void SetInputMode(bool useKeyboard)
        {
            useKeyboardInput = useKeyboard;
            if (!useKeyboard)
            {
                moveQueue.Clear();
            }
        }
    }
}