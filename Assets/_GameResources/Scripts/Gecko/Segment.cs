using UnityEngine;
using UnityEngine.UIElements;

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
        private GameTile _currentTile;

        // Movement data
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector2Int _startCoordinate;
        private Vector2Int _targetCoordinate;

        // Turn detection
        private Vector2Int _previousMoveDirection;
        private Vector2Int _currentTurnDirection;


        // Movement state
        private bool _isInMovement = false;

        public Segment PrevSegment { private set; get; }
        public Segment NextSegment { private set; get; }
        public Vector2Int Coordinate { private set; get; }
        public Vector2Int MoveDirection { private set; get; }
        public BodyController Controller { get; private set; }
        public GameTile CurrentTile { get => _currentTile; set => _currentTile = value; }

        private BoxCollider boxCollider;

        private LevelGame levelGame;

        private LevelGame LevelGame
        {
            get
            {
                if(levelGame == null) levelGame = GetComponentInParent<LevelGame>(); 
                return levelGame;
            }
        }

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        private void Update()
        {
            UpdateRotation();
            CheckMoveToPortal();
        }

        private void CheckMoveToPortal()
        {
            if(segmentType != SegmentType.HEAD && segmentType != SegmentType.TAIL)
            {
                return;
            }

            var centerBox = boxCollider.transform.TransformPoint(boxCollider.center); 

            var listBoxOverlap = Physics.OverlapBox(
                centerBox,
                boxCollider.size * 0.5f,
                boxCollider.transform.rotation
            );

            var collidePortal = 
        }

        private void UpdateRotation()
        {
            if (segmentType == SegmentType.HEAD && NextSegment != null)
            {
                Vector3 dir = transform.position - NextSegment.transform.position;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // Offset the angle by +90 degrees so the head points correctly
                Quaternion targetRot = Quaternion.Euler(0, 0, angle - 90f);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 20);
            }

            if (segmentType == SegmentType.TAIL && PrevSegment != null)
            {
                Vector3 dir = transform.position - PrevSegment.transform.position;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // Offset the angle by +90 degrees so the head points correctly
                Quaternion targetRot = Quaternion.Euler(0, 0, angle + 90f);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 20);
            }
        }

        public void SetController(BodyController controller)
        {
            Controller = controller;
        }

        public void Setup(Segment prev, Segment next)
        {
            PrevSegment = prev;
            NextSegment = next;
        }

        public void InitCoordinate(Vector2Int coordinate)
        {
            if (LevelGame.GameMap.TryGetTileAtCoord(coordinate, out var tile))
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

        public void UpdateCoordinateAndTile(Vector2Int coordinate)
        {
            if (LevelGame.GameMap.TryGetTileAtCoord(coordinate, out var tile))
            {
                _currentTile = tile;
                Coordinate = coordinate;
            }
        }

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
