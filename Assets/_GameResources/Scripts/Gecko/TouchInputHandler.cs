using UnityEngine;
using UnityEngine.EventSystems;
using Geckout.PathFinding;
using System.Collections.Generic;
using System.Linq;

namespace Geckout
{
    public class TouchInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask tileLayerMask = 1;
        [SerializeField] private LayerMask segmentLayer = 7;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float pathUpdateInterval = 0.2f; // Cập nhật path mỗi 0.2s khi drag

        private BodyController targetGecko;
        private bool isDragging = false;
        private bool isDraggingFromHead = false;
        private Vector2Int lastTargetTile = Vector2Int.one * -1;
        private ASPathFinding pathfinder;
        private float lastPathUpdateTime = 0f;

        private List<ASNode> currentPath;
        private List<Vector2Int> smoothPath = new List<Vector2Int>();

        void Start()
        {
            Application.targetFrameRate = 60;
            if (gameCamera == null)
                gameCamera = Camera.main;
        }

        void Update()
        {
            HandleTouchInput();
        }

        void HandleTouchInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTouchStart(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                OnTouchDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnTouchEnd();
            }
        }

        void OnTouchStart(Vector2 screenPosition)
        {
            DebugLog($"Touch start at screen: {screenPosition}");

            Vector2Int? tileCoord = GetTileCoordinateFromScreen(screenPosition);
            if (!tileCoord.HasValue)
            {
                DebugLog("No tile found at touch position");
                return;
            }

            DebugLog($"Touch at tile coordinate: {tileCoord.Value}");

            var gecko = GetGeckoByMouse(screenPosition);
            if (gecko == null)
            {
                DebugLog("No gecko found at tile");
                return;
            }

            if (gecko.IsMoving)
            {
                DebugLog("Gecko is currently moving, but allowing path update");
                // Không return nữa, cho phép cập nhật path khi đang di chuyển
            }

            Vector2Int headCoord = gecko.Segments[0].Coordinate;
            Vector2Int tailCoord = gecko.Segments[gecko.Segments.Count - 1].Coordinate;

            DebugLog($"Gecko head at: {headCoord}, tail at: {tailCoord}");

            if (tileCoord.Value == headCoord)
            {
                DebugLog("Starting drag from HEAD");
                StartDragging(gecko, true);
            }
            else if (tileCoord.Value == tailCoord)
            {
                DebugLog("Starting drag from TAIL");
                StartDragging(gecko, false);
            }
            else
            {
                DebugLog("Touch not on head or tail");
            }
        }

        void OnTouchDrag(Vector2 screenPosition)
        {
            if (!isDragging) return;
            Vector2Int? tileCoord = GetTileCoordinateFromScreen(screenPosition);
            if (!tileCoord.HasValue) return;

            if (tileCoord.Value == lastTargetTile) return;

            // Throttle path updates để tránh spam
            if (Time.time - lastPathUpdateTime < pathUpdateInterval)
            {
                return;
            }

            lastTargetTile = tileCoord.Value;
            lastPathUpdateTime = Time.time;

            DebugLog($"Dragging to NEW target tile: {tileCoord.Value}");

            // Find and set new smooth path
            FindAndSetSmoothPath(tileCoord.Value);
        }

        void OnTouchEnd()
        {
            isDragging = false;
            targetGecko = null;
            currentPath?.Clear();
            smoothPath.Clear();
            lastTargetTile = Vector2Int.one * -1;
        }

        void StartDragging(BodyController gecko, bool fromHead)
        {
            targetGecko = gecko;
            isDragging = true;
            isDraggingFromHead = fromHead;

            DebugLog($"Started dragging gecko from {(fromHead ? "HEAD" : "TAIL")}");

            // Initialize pathfinder
            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            DebugLog($"Map size: {GameMap.MapSize}, total tiles: {mapState.Length}");
        }

        void FindAndSetSmoothPath(Vector2Int targetTile)
        {
            if (targetGecko == null) return;

            Vector2Int startPos = isDraggingFromHead ?
                targetGecko.Segments[0].Coordinate :
                targetGecko.Segments[targetGecko.Segments.Count - 1].Coordinate;

            if (startPos == targetTile) return;

            // Lấy map state hiện tại
            bool[] mapState = GameMap.GetCurrentMapState();

            // block tiles mà gecko đang chiếm
            for (int i = 0; i < targetGecko.Segments.Count; i++)
            {
                // Lấy vị trí THỰC TẾ hiện tại của segment
                Vector3 worldPos = targetGecko.Segments[i].transform.position;
                Vector2Int gridPos = targetGecko.OccupiedTileController.WorldToGridPosition(worldPos);

                int index = gridPos.y * GameMap.MapSize.x + gridPos.x;
                if (index >= 0 && index < mapState.Length)
                {
                    mapState[index] = false; // Force block
                }
            }

            // Chỉ mở tile xuất phát
            int startIndex = startPos.y * GameMap.MapSize.x + startPos.x;
            mapState[startIndex] = true;

            // Tạo pathfinder với grid đã update
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            pathfinder.Reset();
            pathfinder.FindPath(startPos, targetTile, OnSmoothPathFound);
            Debug.Log(targetTile);
        }

        void OnSmoothPathFound(List<ASNode> path)
        {
            if (path == null || path.Count == 0)
            {
                DebugLog("No path found!");
                return;
            }

            DebugLog($"Path found with {path.Count} steps");

            // Convert to coordinate list
            smoothPath.Clear();
            foreach (var node in path)
            {
                smoothPath.Add(node.Position);
            }

            // Execute smooth continuous movement immediately
            ExecuteSmoothPath(smoothPath);
        }

        void ExecuteSmoothPath(List<Vector2Int> path)
        {
            if (targetGecko == null || path == null || path.Count == 0)
            {
                DebugLog("Cannot execute path: invalid state");
                return;
            }

            DebugLog($"Executing smooth path with {path.Count} points");
            targetGecko.ClearPath();
            // PATCH: không cần ClearPath phức tạp nữa, chỉ set path
            targetGecko.SetMovementPath(path);

            DebugLog("Smooth path movement started");
        }


        Vector2Int? GetTileCoordinateFromScreen(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
            {
                GameTile tile = hit.collider.transform.parent.GetComponent<GameTile>();
                if (tile != null)
                {
                    if (tile.IsOccupied && isDragging)
                    {
                        //Debug.Log($"Tile at {tile.Coordinate} is occupied");
                        return null;
                    }
                    else
                    {
                        //Debug.Log($"Tile at {tile.Coordinate} is free");
                    }
                    return tile.Coordinate;
                }
                else
                {
                    DebugLog("Raycast hit but no GameTile component found");
                }
            }

            return null;
        }

        BodyController GetGeckoByMouse(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, segmentLayer))
            {
                Segment segment = hit.collider.transform.parent.GetComponent<Segment>();
                if (segment != null)
                {
                    return segment.Controller;
                }
            }
            return null;
        }

        void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInput] {message}");
            }
        }
    }
}