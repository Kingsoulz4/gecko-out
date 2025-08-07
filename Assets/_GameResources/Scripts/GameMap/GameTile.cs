using System.Collections;
using System.Collections.Generic;
using Geckout.Generals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Geckout
{
    public class GameTile : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] public bool IsOccupied;
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
            GameEvents.OnTileSelected?.Invoke(this);
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

        public void ChangeColor(Color color)
        {
            tileRenderer.material.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameEvents.OnTileSelected?.Invoke(this);
        }
    }
}