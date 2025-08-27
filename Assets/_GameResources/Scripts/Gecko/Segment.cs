using UnityEngine;

namespace Geckout
{
    public enum SegmentType
    {
        HEAD,
        BODY,
        TAIL
    }

    public class Segment : MonoBehaviour
    {
        public SegmentType segmentType = SegmentType.BODY; 
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
        public GameTile CurrentTile { get => _currentTile; set => _currentTile = value; }


        private void Update()
        {
            if (segmentType == SegmentType.HEAD && NextSegment != null)
            {
                Vector3 dir = transform.position - NextSegment.transform.position;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // Offset the angle by +90 degrees so the head points correctly
                Quaternion targetRot = Quaternion.Euler(0, 0, angle - 90f);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10);
            }

            if(segmentType == SegmentType.TAIL && PrevSegment != null)
            {

            }    
        }

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

        public void UpdateCoordinateOnly(Vector2Int coordinate)
        {
            if (GameMap.TryGetTileAt(coordinate, out var tile))
            {
                _currentTile = tile;
                Coordinate = coordinate;
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
