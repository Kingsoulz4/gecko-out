using System;
using UnityEngine;

namespace Geckout
{
    public class OccupiedTileController : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.05f; // 20 FPS, có thể cao hơn
        [SerializeField] private BodyController bodyController;

        private Vector2Int[] lastGridPositions;
        private GameTile[] currentOccupiedTiles;
        private float lastUpdateTime;

        // Cache để tránh tính toán offset mỗi frame
        private Vector2 gridOffset;
        private bool isInitialized = false;

        public Vector2Int[] LastGridPositions { get => lastGridPositions; }

        void Start()
        {
            Init();
            
        }

        private void OnStartMoveHandle()
        {
        }

        private void OnEndMoveHandle()
        {
            UpdateAllSegmentPositions();
        }

        void Init()
        {
            this.WaitUntil(() => bodyController != null, () =>
            {
                bodyController.OnStartMove += OnStartMoveHandle;
                bodyController.OnEndMove += OnEndMoveHandle;
                if (bodyController?.Segments == null)
                {
                    Debug.LogWarning("Cannot initialize - no segments!");
                    return;
                }

                int segmentCount = bodyController.Segments.Count;
                lastGridPositions = new Vector2Int[segmentCount];
                currentOccupiedTiles = new GameTile[segmentCount];

                Vector2Int mapSize = GameMap.MapSize;
                gridOffset = new Vector2(mapSize.x - 1, mapSize.y - 1) * 0.5f;

                Debug.Log($"Map size: {mapSize}, calculated offset: {gridOffset}");

                // Test với tile (0,0)
                if (GameMap.TryGetTileAt(new Vector2Int(0, 0), out GameTile testTile))
                {
                    Vector3 testWorldPos = testTile.transform.position;
                    Debug.Log($"Expected calculation: (0,0) -> world = (0 - {gridOffset.x}, 0 - {gridOffset.y}) = ({-gridOffset.x}, {-gridOffset.y})");

                    // Verify reverse calculation
                    Vector2Int backCalc = WorldToGridPosition(testWorldPos);
                    Debug.Log($"Reverse calc: world {testWorldPos} -> grid {backCalc}");
                }

                // Initialize với position hiện tại
                for (int i = 0; i < segmentCount; i++)
                {
                    lastGridPositions[i] = Vector2Int.one * int.MinValue; // Force update lần đầu
                }

                isInitialized = true;

                UpdateAllSegmentPositions();
            });
        }

        void Update()
        {

            if (!isInitialized)
            {
                return;
            }

            if (Time.time - lastUpdateTime < updateInterval)
            {
                return;
            }

            UpdateAllSegmentPositions();
            lastUpdateTime = Time.time;
        }

        public void UpdateAllSegmentPositions()
        {
            // Get segments in movement order to avoid conflicts
            var orderedSegments = bodyController.GetOrderedSegmentsForTileUpdate();
            var segments = bodyController.Segments;

            for (int i = 0; i < orderedSegments.Count; i++)
            {
                var segment = orderedSegments[i];
                int rawIndex = segments.IndexOf(segment);

                Vector3 worldPos = segment.transform.position;
                Vector2Int gridPos = WorldToGridPosition(worldPos);

                if (gridPos != lastGridPositions[rawIndex])
                {
                    UpdateSegmentTile(rawIndex, gridPos);
                    lastGridPositions[rawIndex] = gridPos;
                }
            }
        }

        public void ClearOccupied()
        {
            var segments = bodyController.Segments;

            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].CurrentTile.SetOccupied(false);
            }
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            float gridX = worldPos.x + gridOffset.x;
            float gridY = worldPos.y + gridOffset.y;

            Vector2Int result = new Vector2Int(
                Mathf.RoundToInt(gridX),
                Mathf.RoundToInt(gridY)
            );

            return result;
        }

        void UpdateSegmentTile(int segmentIndex, Vector2Int gridPos)
        {
            var segment = bodyController.Segments[segmentIndex];


            if (currentOccupiedTiles[segmentIndex] != null)
            {
                currentOccupiedTiles[segmentIndex].SetOccupied(false);
            }

            if (GameMap.TryGetTileAt(gridPos, out GameTile newTile))
            {
                currentOccupiedTiles[segmentIndex] = newTile;
                newTile.SetOccupied(true);
                segment.UpdateCoordinateOnly(gridPos);
            }
            else
            {
                currentOccupiedTiles[segmentIndex] = null;
                Debug.LogWarning($"FAILED: No tile found at grid position {gridPos} for segment {segmentIndex}");
            }
        }

        void OnDestroy()
        {
            if (currentOccupiedTiles == null) return;

            for (int i = 0; i < currentOccupiedTiles.Length; i++)
            {
                if (currentOccupiedTiles[i] != null)
                {
                    currentOccupiedTiles[i].SetOccupied(false);
                }
            }
        }
    }
}