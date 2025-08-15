using UnityEngine;
using UnityEngine.Serialization;

namespace Geckout
{
    public class GeckoSegment : MonoBehaviour
    {
        public GeckoSegment PrevSegment { private set; get; }
        public GeckoSegment NextSegment { private set; get; }
        public Vector2Int Coordinate { private set; get; }
        public Vector2Int MoveDirection { private set; get; }
        private GameTile _currentTile;

        // Thêm biến để lưu trữ vị trí bắt đầu và kết thúc cho smooth interpolation
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector2Int _startCoordinate;
        private Vector2Int _targetCoordinate;

        // Thêm biến để kiểm tra xem có phải đang rẽ không
        private bool _isTurning = false;
        private Vector2Int _previousMoveDirection;

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

                // Cập nhật position cho lần di chuyển tiếp theo
                _startPosition = transform.position;
                _startCoordinate = coordinate;
            }
            else
            {
                Debug.LogError($"Invalid coordinate: {coordinate}");
            }
        }

        public void Move(bool isMoveForward, float ratio)
        {
            var targetCoordinate = GetTargetCoordinate(isMoveForward);

            // Chỉ tính toán lại khi ratio = 0 (bắt đầu di chuyển mới)
            if (Mathf.Approximately(ratio, 0f))
            {
                PrepareMovement(targetCoordinate);
            }

            // Sử dụng interpolation mượt mà
            float smoothRatio = GetSmoothRatio(ratio);
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, smoothRatio);
        }

        private void PrepareMovement(Vector2Int targetCoordinate)
        {
            GameMap.TryGetTileAt(Coordinate, out var currentTile);

            if (GameMap.TryGetTileAt(targetCoordinate, out var targetTile))
            {
                Vector2Int newDirection = targetCoordinate - Coordinate;

                // Kiểm tra xem có đang rẽ không
                _isTurning = _previousMoveDirection != Vector2Int.zero && _previousMoveDirection != newDirection;

                // Cập nhật thông tin di chuyển
                MoveDirection = newDirection;
                _startPosition = currentTile.transform.position;
                _targetPosition = targetTile.transform.position;
                _startCoordinate = Coordinate;
                _targetCoordinate = targetCoordinate;

                _previousMoveDirection = newDirection;
            }
        }

        private float GetSmoothRatio(float ratio)
        {
            if (_isTurning)
            {
                // Sử dụng ease-in-out cho chuyển động mượt mà hơn tại góc
                return EaseInOutQuart(ratio);
            }
            else
            {
                // Sử dụng smoothstep cho chuyển động bình thường
                return Mathf.SmoothStep(0f, 1f, ratio);
            }
        }

        // Hàm easing mượt mà hơn cho góc cua
        private float EaseInOutQuart(float t)
        {
            return t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;
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
    }
}