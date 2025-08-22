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
        [SerializeField] private float moveTime = 0.2f; // Time to move one segment
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curve cho chuyển động mượt
        [SerializeField] private float outerSmoothness = 0.7f; // Độ mượt của outer curve
        [SerializeField] private float cornerRadius = 0.3f;    // Bán kính curve outer

        private GeckoSegment _head, _tail;
        public List<GeckoSegment> Segments { private set; get; }
        public bool IsMoving { get => isMoving;}

        BodyRenderer _bodyRenderer;

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
            _head.Setup(null, Segments[1]); // Head has no previous segment
            _tail.Setup(Segments[Segments.Count - 2], null); // Tail has no next segment

            for (int i = 1; i < Segments.Count - 1; i++)
            {
                var currentSegment = Segments[i];
                GeckoSegment prevSegment = Segments[i - 1];
                GeckoSegment nextSegment = Segments[i + 1]; // Fix: should be i+1, not i
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
            Vector2Int delta = GetDeltaMovement();
            if (delta != Vector2Int.zero)
            {
                StartCoroutine(MoveHead(delta));
            }
        }
       
        IEnumerator MoveHead(Vector2Int delta)
        {
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
                    var orthorgonalVectors = GetOrthogonalUnitVectors(tailDirection);

                    foreach (var orthogonalVector in orthorgonalVectors)
                    {
                        var newTailPosition = _tail.Coordinate + orthogonalVector;
                        if (GameMap.TryGetTileAt(newTailPosition, out var orthogonalTile))
                        {
                            yield return MoveTailToPosition(newTailPosition, orthogonalTile.transform.position);
                            break; // Only move tail once
                        }
                    }
                }
            }
        }

        private void MoveTail(Vector2Int delta)
        {
            Vector2Int newTailPosition = _tail.Coordinate + delta;
            if (GameMap.TryGetTileAt(newTailPosition, out var tile))
            {
                StartCoroutine(MoveTailToPosition(newTailPosition, tile.transform.position));
            }
            else
            {
                Debug.LogWarning("Invalid move: " + newTailPosition);
            }
        }

        // Chỉ thay thế phương thức MoveHeadToPosition trong GeckoController.cs

        IEnumerator MoveHeadToPosition(Vector2Int newCoordinate, Vector3 newWorldPosition)
        {
            isMoving = true;
            GameMap.TryGetTileAt(_head.Coordinate, out var headTile);
            Vector3 startPosition = headTile.transform.position;
            Vector3 targetPosition = newWorldPosition;

            // Tính hướng di chuyển để kiểm tra góc rẽ
            Vector2Int moveDirection = newCoordinate - _head.Coordinate;
            bool isCornerTurn = IsCornerTurn(moveDirection);

            // Reset tất cả segments về vị trí ban đầu trước khi bắt đầu di chuyển
            for (int i = 1; i < Segments.Count; i++)
            {
                Segments[i].Move(true, 0f); // Reset về ratio = 0
            }

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveTime);

                // Sử dụng animation curve cho chuyển động mượt mà
                float ratio = movementCurve.Evaluate(t);

                // Áp dụng smoothing đặc biệt cho góc rẽ
                if (isCornerTurn)
                {
                    ratio = ApplyCornerSmoothing(ratio);
                }

                // Di chuyển head
                _head.transform.position = Vector3.Lerp(startPosition, targetPosition, ratio);

                // Di chuyển body segments với cùng ratio
                for (int i = 1; i < Segments.Count; i++)
                {
                    Segments[i].Move(true, ratio);
                }

                yield return null;
            }

            // Đảm bảo tất cả segments đều ở vị trí cuối
            _head.transform.position = targetPosition;
            for (int i = 1; i < Segments.Count; i++)
            {
                Segments[i].Move(true, 1f);
            }

            // Cleanup logic - giải phóng tiles cũ
            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                Segments[i].ReleaseCurrentTile();
            }

            // Cập nhật coordinates
            for (int i = Segments.Count - 1; i >= 1; i--)
            {
                Segments[i].SetCoordinate(Segments[i - 1].Coordinate);
            }

            _head.SetCoordinate(newCoordinate);
            isMoving = false;
        }

        // Tương tự cho MoveTailToPosition
        IEnumerator MoveTailToPosition(Vector2Int newCoordinate, Vector3 newWorldPosition)
        {
            isMoving = true;
            GameMap.TryGetTileAt(_tail.Coordinate, out var tailTile);
            Vector3 startPosition = tailTile.transform.position;
            Vector3 targetPosition = newWorldPosition;

            // Tính hướng di chuyển để kiểm tra góc rẽ
            Vector2Int moveDirection = newCoordinate - _tail.Coordinate;
            bool isCornerTurn = IsCornerTurn(moveDirection);

            // Reset tất cả segments về vị trí ban đầu
            for (int i = Segments.Count - 2; i >= 0; i--)
            {
                Segments[i].Move(false, 0f);
            }

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveTime);

                // Sử dụng animation curve cho chuyển động mượt mà
                float ratio = movementCurve.Evaluate(t);

                // Áp dụng smoothing đặc biệt cho góc rẽ
                if (isCornerTurn)
                {
                    ratio = ApplyCornerSmoothing(ratio);
                }

                // Di chuyển tail
                _tail.transform.position = Vector3.Lerp(startPosition, targetPosition, ratio);

                // Di chuyển body segments
                for (int i = Segments.Count - 2; i >= 0; i--)
                {
                    Segments[i].Move(false, ratio);
                }

                yield return null;
            }

            // Đảm bảo tất cả segments đều ở vị trí cuối
            _tail.transform.position = targetPosition;
            for (int i = Segments.Count - 2; i >= 0; i--)
            {
                Segments[i].Move(false, 1f);
            }

            // Cleanup logic
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
            // Logic để kiểm tra góc rẽ dựa vào lịch sử di chuyển
            // Có thể mở rộng thêm logic phức tạp hơn
            return true; // Tạm thời return true để áp dụng smoothing cho tất cả chuyển động
        }

        // Áp dụng smoothing đặc biệt cho góc rẽ
        private float ApplyCornerSmoothing(float ratio)
        {
            // Sử dụng SmoothStep để tạo chuyển động mượt mà hơn
            return Mathf.SmoothStep(0f, 1f, ratio);
        }

        private Vector2Int GetDeltaMovement()
        {
            Vector2Int delta = Vector2Int.zero;
            if (isMoving)
                return delta;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                delta = Vector2Int.up;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                delta = Vector2Int.down;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                delta = Vector2Int.left;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                delta = Vector2Int.right;
            }

            return delta;
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
                // Exclude input and its reverse
                if (v == input || v == -input) continue;
                // Check for orthogonality
                if (Vector2.Dot(input, v) == 0)
                    result.Add(v);
            }
            return result;
        }
    }
}