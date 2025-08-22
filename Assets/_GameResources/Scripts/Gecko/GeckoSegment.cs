using UnityEngine;

namespace Geckout
{
    public class GeckoSegment : MonoBehaviour
    {
        public GeckoSegment PrevSegment { private set; get; }
        public GeckoSegment NextSegment { private set; get; }
        public Vector2Int Coordinate { private set; get; }
        public Vector2Int MoveDirection { private set; get; }
        private GameTile _currentTile;

        // Movement data
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector2Int _startCoordinate;
        private Vector2Int _targetCoordinate;

        // Turn detection
        private bool _isTurning = false;
        private Vector2Int _previousMoveDirection;
        private Vector2Int _currentTurnDirection;

        // Hybrid corner settings
        [Header("Hybrid Corner Settings")]
        [SerializeField] private float outerSmoothness = 0.7f; // Độ mượt của outer curve
        [SerializeField] private float cornerRadius = 0.3f;    // Bán kính curve outer
        [SerializeField] private bool debugCorners = false;    // Debug visualization

        // Movement state
        private bool _isMovementPrepared = false;
        private bool _isInMovement = false;

        public void Setup(GeckoSegment prev, GeckoSegment next)
        {
            PrevSegment = prev;
            NextSegment = next;
        }

        public void ReleaseCurrentTile()
        {
            if (_currentTile != null)
                _currentTile.SetOccupied(false);
        }

        public void SetCoordinate(Vector2Int coordinate)
        {
            if (GameMap.TryGetTileAt(coordinate, out var tile))
            {
                _currentTile = tile;
                Coordinate = coordinate;
                _currentTile.SetOccupied(true);
                transform.position = tile.transform.position;

                _startPosition = transform.position;
                _startCoordinate = coordinate;
                _isMovementPrepared = false;
                _isInMovement = false;
            }
            else
            {
                Debug.LogError($"Invalid coordinate: {coordinate}");
            }
        }

        public void PrepareMovement(bool isMoveForward, float ratio)
        {
            if (!_isMovementPrepared || Mathf.Approximately(ratio, 0f))
            {
                var targetCoordinate = GetTargetCoordinate(isMoveForward);
                PrepareMovementData(targetCoordinate);
                _isMovementPrepared = true;
            }

            _isInMovement = true;
        }

        public void Move(bool isMoveForward, float ratio)
        {
            if (!_isMovementPrepared)
            {
                PrepareMovement(isMoveForward, ratio);
            }

            // Hybrid corner movement
            Vector3 finalPosition;

            if (_isTurning)
            {
                finalPosition = GetHybridCornerPosition(ratio);
            }
            else
            {
                // Straight movement - simple lerp
                finalPosition = Vector3.Lerp(_startPosition, _targetPosition, GetStraightMovementRatio(ratio));
            }

            transform.position = finalPosition;

            // Reset movement state khi hoàn thành
            if (Mathf.Approximately(ratio, 1f))
            {
                _isInMovement = false;
                _isMovementPrepared = false;
            }
        }

        private void PrepareMovementData(Vector2Int targetCoordinate)
        {
            GameMap.TryGetTileAt(Coordinate, out var currentTile);

            if (GameMap.TryGetTileAt(targetCoordinate, out var targetTile))
            {
                Vector2Int newDirection = targetCoordinate - Coordinate;

                // Detect turn
                _isTurning = _previousMoveDirection != Vector2Int.zero && _previousMoveDirection != newDirection;
                _currentTurnDirection = newDirection;

                // Update movement data
                MoveDirection = newDirection;
                _startPosition = currentTile.transform.position;
                _targetPosition = targetTile.transform.position;
                _startCoordinate = Coordinate;
                _targetCoordinate = targetCoordinate;

                _previousMoveDirection = newDirection;

                if (debugCorners && _isTurning)
                {
                    Debug.Log($"Segment {name} turning from {_previousMoveDirection} to {newDirection}");
                }
            }
            else
            {
                _startPosition = transform.position;
                _targetPosition = transform.position;
            }
        }

        private Vector3 GetHybridCornerPosition(float ratio)
        {
            // 1. Calculate sharp inner corner path
            Vector3 innerSharpPath = GetSharpInnerCornerPath(ratio);

            // 2. Calculate smooth outer curve path  
            Vector3 outerSmoothPath = GetSmoothOuterCurvePath(ratio);

            // 3. Blend based on position in the turn
            float outerBlendRatio = GetOuterBlendRatio(ratio);

            return Vector3.Lerp(innerSharpPath, outerSmoothPath, outerBlendRatio);
        }

        private Vector3 GetSharpInnerCornerPath(float ratio)
        {
            // Inner corner luôn đi theo path vuông góc 90°
            Vector3 cornerPoint = GetCornerPoint();

            if (ratio <= 0.5f)
            {
                // Nửa đầu: đi thẳng đến corner
                float t = ratio * 2f; // 0->1 trong nửa đầu
                return Vector3.Lerp(_startPosition, cornerPoint, t);
            }
            else
            {
                // Nửa sau: từ corner đi thẳng đến target
                float t = (ratio - 0.5f) * 2f; // 0->1 trong nửa sau
                return Vector3.Lerp(cornerPoint, _targetPosition, t);
            }
        }

        private Vector3 GetSmoothOuterCurvePath(float ratio)
        {
            // Outer path sử dụng curve mượt mà
            Vector3 cornerPoint = GetCornerPoint();

            // Tạo control points cho Bezier curve
            Vector3 p0 = _startPosition;
            Vector3 p3 = _targetPosition;

            // Control points cho smooth curve
            Vector3 p1 = GetOuterControlPoint1(cornerPoint);
            Vector3 p2 = GetOuterControlPoint2(cornerPoint);

            // Cubic Bezier curve
            return CalculateBezierPoint(ratio, p0, p1, p2, p3);
        }

        private Vector3 GetCornerPoint()
        {
            // Điểm góc vuông - intersection của 2 đường thẳng
            Vector3 prevDir = Vector3.zero;
            Vector3 currentDir = (_targetPosition - _startPosition).normalized;

            if (_previousMoveDirection != Vector2Int.zero)
            {
                prevDir = new Vector3(_previousMoveDirection.x, 0, _previousMoveDirection.y);
            }

            // Corner point tại intersection
            Vector3 cornerOffset = Vector3.zero;

            if (_currentTurnDirection.x != 0 && _previousMoveDirection.y != 0)
            {
                // Turn horizontal từ vertical
                cornerOffset = new Vector3(_currentTurnDirection.x, 0, 0);
            }
            else if (_currentTurnDirection.y != 0 && _previousMoveDirection.x != 0)
            {
                // Turn vertical từ horizontal  
                cornerOffset = new Vector3(0, 0, _currentTurnDirection.y);
            }

            return _startPosition + cornerOffset;
        }

        private Vector3 GetOuterControlPoint1(Vector3 cornerPoint)
        {
            // Control point 1 - phía start, offset ra ngoài
            Vector3 direction = (cornerPoint - _startPosition).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

            return _startPosition + direction * 0.5f + perpendicular * cornerRadius;
        }

        private Vector3 GetOuterControlPoint2(Vector3 cornerPoint)
        {
            // Control point 2 - phía target, offset ra ngoài
            Vector3 direction = (_targetPosition - cornerPoint).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

            return _targetPosition - direction * 0.5f + perpendicular * cornerRadius;
        }

        private float GetOuterBlendRatio(float ratio)
        {
            // Tỷ lệ blend từ inner (sharp) sang outer (smooth)
            // Peak ở giữa turn (ratio = 0.5)

            float peak = 0.5f;
            float distance = Mathf.Abs(ratio - peak);
            float normalizedDistance = distance / peak;

            // Smooth curve - peak tại giữa turn
            return (1f - normalizedDistance) * outerSmoothness;
        }

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Cubic Bezier curve formula
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 point = uuu * p0;
            point += 3 * uu * t * p1;
            point += 3 * u * tt * p2;
            point += ttt * p3;

            return point;
        }

        private float GetStraightMovementRatio(float ratio)
        {
            // Smooth easing cho straight movement
            return Mathf.SmoothStep(0f, 1f, ratio);
        }

        private Vector2Int GetTargetCoordinate(bool isMoveForward)
        {
            if (isMoveForward && PrevSegment != null)
            {
                return PrevSegment.Coordinate;
            }

            if (!isMoveForward && NextSegment != null)
            {
                return NextSegment.Coordinate;
            }

            return Coordinate;
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!debugCorners || !_isInMovement) return;

            // Vẽ start và target
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_startPosition, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetPosition, 0.1f);

            if (_isTurning)
            {
                // Vẽ corner point
                Vector3 cornerPoint = GetCornerPoint();
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(cornerPoint, Vector3.one * 0.1f);

                // Vẽ inner sharp path
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(_startPosition, cornerPoint);
                Gizmos.DrawLine(cornerPoint, _targetPosition);

                // Vẽ outer smooth curve (approximation)
                Gizmos.color = Color.magenta;
                Vector3 lastPoint = _startPosition;
                for (int i = 1; i <= 10; i++)
                {
                    float t = i / 10f;
                    Vector3 curvePoint = GetSmoothOuterCurvePath(t);
                    Gizmos.DrawLine(lastPoint, curvePoint);
                    lastPoint = curvePoint;
                }
            }
        }

        // Public properties for debugging
        public bool IsTurning => _isTurning;
        public Vector2Int CurrentTurnDirection => _currentTurnDirection;
        public Vector2Int PreviousMoveDirection => _previousMoveDirection;
    }
}