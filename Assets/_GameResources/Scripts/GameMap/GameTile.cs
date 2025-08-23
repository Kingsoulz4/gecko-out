using System.Collections;
using System.Collections.Generic;
using Geckout.Generals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Geckout
{
    public class GameTile : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] public bool IsOccupied;
        [SerializeField] private Collider tileCollider; // For raycast detection

        public Vector2Int Coordinate { get; private set; }

        private void Start()
        {
            // Ensure collider exists for touch detection
            if (tileCollider == null)
            {
                tileCollider = GetComponent<Collider>();
                if (tileCollider == null)
                {
                    // Add collider if none exists
                    var boxCollider = gameObject.AddComponent<BoxCollider>();
                    boxCollider.size = Vector3.one;
                    tileCollider = boxCollider;
                }
            }
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
                    if (x == 0 && y == 0) continue;
                    if (GameMap.TryGetTileAt(Coordinate.x + x, Coordinate.y + y, out GameTile tile))
                    {
                        results.Add(tile);
                    }
                }
            }
            return results.ToArray();
        }

        public void ChangeColor(Color color)
        {
            if (tileRenderer != null)
            {
                tileRenderer.material.color = color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameEvents.OnTileSelected?.Invoke(this);
        }

        // Highlight tile for path visualization
        public void SetPathHighlight(bool highlight)
        {
            if (highlight)
            {
                ChangeColor(Color.yellow);
            }
            else
            {
                ChangeColor(Color.white); // Default color
            }
        }

        // Check if this tile can be occupied by gecko
        public bool CanBeOccupied()
        {
            return !IsOccupied;
        }

        // Get world position for movement calculations
        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);

            // Draw coordinate text
            var style = new GUIStyle();
            style.normal.textColor = Color.black;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                $"({Coordinate.x},{Coordinate.y})", style);
#endif
        }
    }
}