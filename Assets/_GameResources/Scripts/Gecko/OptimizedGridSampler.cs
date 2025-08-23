using UnityEngine;

namespace Geckout
{
    // Attach vào GeckoController để handle tất cả segments
    public class OptimizedGridSampler : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private float updateInterval = 0.05f; // 20 FPS, có thể cao hơn
        [SerializeField] private GeckoController geckoController;

        private Vector2Int[] lastGridPositions;
        private GameTile[] currentOccupiedTiles;
        private float lastUpdateTime;

        // Cache để tránh tính toán offset mỗi frame
        private Vector2 gridOffset;
        private bool isInitialized = false;

        void Start()
        {
            InitializeGridSampler();
        }

        void InitializeGridSampler()
        {
            this.WaitUntil(() => geckoController != null, () =>
            {
                Debug.Log("GameMap is initialized, proceeding with GridSampler initialization.");
                if (geckoController?.Segments == null)
                {
                    Debug.LogWarning("Cannot initialize - no segments!");
                    return;
                }

                int segmentCount = geckoController.Segments.Count;
                lastGridPositions = new Vector2Int[segmentCount];
                currentOccupiedTiles = new GameTile[segmentCount];

                // Tính offset từ GameMap (giống như trong SpawnAllTiles)
                Vector2Int mapSize = GameMap.MapSize;
                gridOffset = new Vector2(mapSize.x - 1, mapSize.y - 1) * 0.5f;

                Debug.Log($"Map size: {mapSize}, calculated offset: {gridOffset}");

                // Test với tile (0,0) để verify
                if (GameMap.TryGetTileAt(new Vector2Int(0, 0), out GameTile testTile))
                {
                    Vector3 testWorldPos = testTile.transform.position;
                    Debug.Log($"Tile (0,0) is at world position: {testWorldPos}");
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
                Debug.Log($"GridSampler initialized for {segmentCount} segments");

                // Force update ngay lần đầu
                Debug.Log("Forcing initial update...");
                UpdateAllSegmentPositions();
            });
        }

        void Update()
        {
            Debug.Log($"Update called - initialized: {isInitialized}, time check: {Time.time - lastUpdateTime >= updateInterval}");

            if (!isInitialized)
            {
                Debug.LogWarning("GridSampler not initialized yet!");
                return;
            }

            if (Time.time - lastUpdateTime < updateInterval)
            {
                return; // Too early
            }

            Debug.Log("Calling UpdateAllSegmentPositions...");
            UpdateAllSegmentPositions();
            lastUpdateTime = Time.time;
        }

        void UpdateAllSegmentPositions()
        {
            var segments = geckoController.Segments;
            Debug.Log($"Updating {segments.Count} segments...");

            for (int i = 0; i < segments.Count; i++)
            {
                Vector3 worldPos = segments[i].transform.position;
                Vector2Int gridPos = WorldToGridPosition(worldPos);

                Debug.Log($"Segment {i}: world {worldPos} -> grid {gridPos}, last: {lastGridPositions[i]}");

                // Chỉ update khi thực sự di chuyển sang ô khác
                if (gridPos != lastGridPositions[i])
                {
                    Debug.Log($"Position changed for segment {i}!");
                    UpdateSegmentTile(i, gridPos);
                    lastGridPositions[i] = gridPos;
                }
            }
        }

        Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            // XY plane: X = horizontal, Y = vertical  
            // Công thức inverse: world = grid - offset
            // => grid = world + offset

            float gridX = worldPos.x + gridOffset.x;
            float gridY = worldPos.y + gridOffset.y; // Y axis cho vertical

            Vector2Int result = new Vector2Int(
                Mathf.RoundToInt(gridX),
                Mathf.RoundToInt(gridY)
            );

            // Debug để kiểm tra
            Debug.Log($"World XY({worldPos.x}, {worldPos.y}) + offset {gridOffset} = Grid {result}");

            return result;
        }

        void UpdateSegmentTile(int segmentIndex, Vector2Int gridPos)
        {
            var segment = geckoController.Segments[segmentIndex];

            // Debug validation
            Debug.Log($"Trying to update segment {segmentIndex} to grid {gridPos}");

            // Release tile cũ
            if (currentOccupiedTiles[segmentIndex] != null)
            {
                currentOccupiedTiles[segmentIndex].SetOccupied(false);
                Debug.Log($"Released old tile for segment {segmentIndex}");
            }

            // Tìm và set tile mới
            if (GameMap.TryGetTileAt(gridPos, out GameTile newTile))
            {
                currentOccupiedTiles[segmentIndex] = newTile;
                newTile.SetOccupied(true);
                segment.UpdateCoordinateOnly(gridPos);

                Debug.Log($"SUCCESS: Segment {segmentIndex} occupied tile at {gridPos}, tile occupied: {newTile.IsOccupied}");
            }
            else
            {
                currentOccupiedTiles[segmentIndex] = null;
                Debug.LogWarning($"FAILED: No tile found at grid position {gridPos} for segment {segmentIndex}");

                // Debug: List all available tiles
                Debug.LogWarning($"Available map size: {GameMap.MapSize}");
            }
        }

        // Cleanup khi destroy
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