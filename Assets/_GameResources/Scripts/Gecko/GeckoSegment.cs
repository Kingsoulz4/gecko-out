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
        private float outerSmoothness = 0f;
        private float cornerRadius = 0f;

        // Movement state
        private bool _isMovementPrepared = false;
        private bool _isInMovement = false;

        public void SetCorner(float outerSmoothness, float cornerRadius)
        {
            this.outerSmoothness = outerSmoothness;
            this.cornerRadius = cornerRadius;
        }

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

        // Dùng khi spawn / init
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

        // Dùng khi đã di chuyển smooth -> chỉ update logic, không dịch chuyển transform
        public void UpdateCoordinateOnly(Vector2Int coordinate)
        {
            if (GameMap.TryGetTileAt(coordinate, out var tile))
            {
                _currentTile = tile;
                Coordinate = coordinate;
                _currentTile.SetOccupied(true);
            }
        }

        // ====== Các hàm Move / Corner giữ nguyên (bạn đã có sẵn) ======
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

            Vector3 finalPosition;

            if (_isTurning)
            {
                finalPosition = GetHybridCornerPosition(ratio);
            }
            else
            {
                finalPosition = Vector3.Lerp(_startPosition, _targetPosition, GetStraightMovementRatio(ratio));
            }

            transform.position = finalPosition;

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

                _isTurning = _previousMoveDirection != Vector2Int.zero && _previousMoveDirection != newDirection;
                _currentTurnDirection = newDirection;

                MoveDirection = newDirection;
                _startPosition = currentTile.transform.position;
                _targetPosition = targetTile.transform.position;
                _startCoordinate = Coordinate;
                _targetCoordinate = targetCoordinate;

                _previousMoveDirection = newDirection;
            }
            else
            {
                _startPosition = transform.position;
                _targetPosition = transform.position;
            }
        }

        private Vector3 GetHybridCornerPosition(float ratio)
        {
            Vector3 innerSharpPath = GetSharpInnerCornerPath(ratio);
            Vector3 outerSmoothPath = GetSmoothOuterCurvePath(ratio);
            float outerBlendRatio = GetOuterBlendRatio(ratio);
            return Vector3.Lerp(innerSharpPath, outerSmoothPath, outerBlendRatio);
        }

        private Vector3 GetSharpInnerCornerPath(float ratio)
        {
            Vector3 cornerPoint = GetCornerPoint();

            if (ratio <= 0.5f)
            {
                float t = ratio * 2f;
                return Vector3.Lerp(_startPosition, cornerPoint, t);
            }
            else
            {
                float t = (ratio - 0.5f) * 2f;
                return Vector3.Lerp(cornerPoint, _targetPosition, t);
            }
        }

        private Vector3 GetSmoothOuterCurvePath(float ratio)
        {
            Vector3 cornerPoint = GetCornerPoint();
            Vector3 p0 = _startPosition;
            Vector3 p3 = _targetPosition;
            Vector3 p1 = GetOuterControlPoint1(cornerPoint);
            Vector3 p2 = GetOuterControlPoint2(cornerPoint);
            return CalculateBezierPoint(ratio, p0, p1, p2, p3);
        }

        private Vector3 GetCornerPoint()
        {
            Vector3 cornerOffset = Vector3.zero;
            if (_currentTurnDirection.x != 0 && _previousMoveDirection.y != 0)
            {
                cornerOffset = new Vector3(_currentTurnDirection.x, 0, 0);
            }
            else if (_currentTurnDirection.y != 0 && _previousMoveDirection.x != 0)
            {
                cornerOffset = new Vector3(0, 0, _currentTurnDirection.y);
            }
            return _startPosition + cornerOffset;
        }

        private Vector3 GetOuterControlPoint1(Vector3 cornerPoint)
        {
            Vector3 direction = (cornerPoint - _startPosition).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            return _startPosition + direction * 0.5f + perpendicular * cornerRadius;
        }

        private Vector3 GetOuterControlPoint2(Vector3 cornerPoint)
        {
            Vector3 direction = (_targetPosition - cornerPoint).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            return _targetPosition - direction * 0.5f + perpendicular * cornerRadius;
        }

        private float GetOuterBlendRatio(float ratio)
        {
            float peak = 0.5f;
            float distance = Mathf.Abs(ratio - peak);
            float normalizedDistance = distance / peak;
            return (1f - normalizedDistance) * outerSmoothness;
        }

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
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

        // Debug visualization giữ nguyên...
        private void OnDrawGizmos()
        {
            if (_isInMovement) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_startPosition, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetPosition, 0.1f);
        }

        public bool IsTurning => _isTurning;
        public Vector2Int CurrentTurnDirection => _currentTurnDirection;
        public Vector2Int PreviousMoveDirection => _previousMoveDirection;
    }
}
