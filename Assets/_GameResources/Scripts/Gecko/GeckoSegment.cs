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

        public void Setup(GeckoSegment prev, GeckoSegment next)
        {
            PrevSegment = prev;
            NextSegment = next;
        }

        public void ReleaseCurrentTile()
        {
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
            }
            else
            {
                Debug.LogError($"Invalid coordinate: {coordinate}");
            }
        }

        public void Move(bool isMoveForward, float ratio)
        {
            var targetCoordinate = GetTargetCoordinate(isMoveForward);
            GameMap.TryGetTileAt(Coordinate, out var currentTile);
            if (GameMap.TryGetTileAt(targetCoordinate, out var targetTile))
            {
                MoveDirection = targetTile.Coordinate - currentTile.Coordinate;
                var startPosition = currentTile.transform.position;
                var endPosition = targetTile.transform.position;
                transform.position = Vector3.Lerp(startPosition, endPosition, ratio);
            }
        }

        float GetRation(float ratio)
        {
            //Check if MoveDirection

            return ratio;
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