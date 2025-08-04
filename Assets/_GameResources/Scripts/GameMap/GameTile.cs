using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Geckout
{
    public class GameTile : MonoBehaviour, IPointerClickHandler
    {
        public bool IsOccupied { get; private set; }
        public Vector2Int Coordinate { get; private set; }

        private void Start()
        {
            // GameEvents.OnEntityDestroyed
            //     .Where(t => t.Equals(CurrentOccupant))
            //     .Subscribe(_ => Assign(null)) // Unassign the occupant when it is destroyed
            //     .AddTo(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        public void SetCoordinate(int x, int y)
        {
            Coordinate = new Vector2Int(x, y);
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }

        public GameTile[] GetNeighbourTiles(int range)
        {
            List<GameTile> results = new();
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    if (x == 0 && y == 0) continue; // Skip the current tile
                    if (GameMap.TryGetTileAt(Coordinate.x + x, Coordinate.y + y,
                            out GameTile tile))
                    {
                        results.Add(tile);
                    }
                }
            }

            return results.ToArray();
        }
    }
}