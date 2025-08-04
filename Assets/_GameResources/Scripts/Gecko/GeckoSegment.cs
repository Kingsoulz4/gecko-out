using UnityEngine;

namespace Geckout
{
    public class GeckoSegment : MonoBehaviour
    {
        private GeckoSegment prevSegment;
        private GeckoSegment nextSegment;

        public Vector2Int Coordinate { private set; get; }


        public void Setup(GeckoSegment prev, GeckoSegment next)
        {
            prevSegment = prev;
            nextSegment = next;
        }

        public void SetCoordinate(Vector2Int coordinate)
        {
            if (GameMap.TryGetTileAt(coordinate, out var tile))
            {
                Coordinate = coordinate;
                tile.SetOccupied(true);
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
                // transform.position = Vector3.Lerp(currentTile.transform.position, tile.transform.position, ratio);
                
                var startPosition = currentTile.transform.position;
                var endPosition = targetTile.transform.position;
                
                // Calculate the direction and normalize it
                var direction = (endPosition - startPosition).normalized;

                // Calculate the fixed distance
                var fixedDistance = Vector3.Distance(startPosition, endPosition);

                // Interpolate using the fixed distance
                transform.position = startPosition + direction * (fixedDistance * ratio);
            }
        }

        private Vector2Int GetTargetCoordinate(bool isMoveForward)
        {
            if (isMoveForward && prevSegment != null)
            {
                return prevSegment.Coordinate;
            }

            if (!isMoveForward && nextSegment != null)
            {
                return nextSegment.Coordinate;
            }

            return Coordinate;
        }
    }
}