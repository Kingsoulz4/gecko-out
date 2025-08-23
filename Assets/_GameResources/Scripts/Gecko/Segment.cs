using UnityEngine;

namespace Geckout
{
    public class Segment : MonoBehaviour
    {
        public Segment PrevSegment { private set; get; }
        public Segment NextSegment { private set; get; }
        public Vector2Int Coordinate { private set; get; }
        public Vector2Int MoveDirection { private set; get; }
        private GameTile _currentTile;

        // Movement data
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector2Int _startCoordinate;
        private Vector2Int _targetCoordinate;

        // Turn detection
        private Vector2Int _previousMoveDirection;
        private Vector2Int _currentTurnDirection;

        // Hybrid corner settings
        private float outerSmoothness = 0f;
        private float cornerRadius = 0f;

        // Movement state
        private bool _isInMovement = false;
        public BodyController Controller { get; private set; }

        public void SetController(BodyController controller)
        {
            Controller = controller;
        }
        public void SetCorner(float outerSmoothness, float cornerRadius)
        {
            this.outerSmoothness = outerSmoothness;
            this.cornerRadius = cornerRadius;
        }

        public void Setup(Segment prev, Segment next)
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

        // Debug visualization giữ nguyên...
        private void OnDrawGizmos()
        {
            if (_isInMovement) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_startPosition, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetPosition, 0.1f);
        }
    }
}
