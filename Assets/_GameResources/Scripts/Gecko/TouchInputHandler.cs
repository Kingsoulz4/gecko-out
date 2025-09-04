using UnityEngine;
using UnityEngine.EventSystems;
using Geckout.PathFinding;
using System.Collections.Generic;
using System.Linq;

namespace Geckout
{
    public class TouchInputHandler : SingletonMono<TouchInputHandler>
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask tileLayerMask = 1;
        [SerializeField] private LayerMask segmentLayer = 7;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float pathUpdateInterval = 0.03f;

        private BodyController bodyController;
        private bool isDragging = false;
        private bool isDraggingFromHead = false;
        private Vector2Int lastTargetTile = Vector2Int.one * -1;
        private ASPathFinding pathfinder;
        private float lastPathUpdateTime = 0f;

        private List<ASNode> currentPath;
        private List<Vector2Int> smoothPath = new List<Vector2Int>();

        public bool IsDragging { get => isDragging; }

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

            var gecko = GetBodyControllerByMouse(screenPosition);
            if (gecko == null)
            {
                DebugLog("No gecko found at tile");
                return;
            }

            if (gecko.IsMoving)
            {
                DebugLog("Gecko is currently moving, but allowing path update");
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

            // Throttle path updates
            if (Time.time - lastPathUpdateTime < pathUpdateInterval)
            {
                return;
            }

            lastTargetTile = tileCoord.Value;
            lastPathUpdateTime = Time.time;

            DebugLog($"Dragging to NEW target tile: {tileCoord.Value}");

            FindAndSetSmoothPath(tileCoord.Value);
        }

        void OnTouchEnd()
        {
            isDragging = false;
            bodyController = null;
            currentPath?.Clear();
            smoothPath.Clear();
            lastTargetTile = Vector2Int.one * -1;
        }

        void StartDragging(BodyController gecko, bool fromHead)
        {
            bodyController = gecko;
            isDragging = true;
            isDraggingFromHead = fromHead;

            // Set control anchor based on drag source
            bodyController.SetControlAnchor(fromHead ? BodyController.ControlAnchor.Head : BodyController.ControlAnchor.Tail);
            DebugLog($"Started dragging gecko from {(fromHead ? "HEAD" : "TAIL")}");

            // Initialize pathfinder
            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            DebugLog($"Map size: {GameMap.MapSize}, total tiles: {mapState.Length}");
        }

        Vector2Int GetDirectionToTarget(Vector2Int start, Vector2Int target)
        {
            Vector2Int diff = target - start;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                return new Vector2Int(diff.x > 0 ? 1 : -1, 0);
            }
            else if (Mathf.Abs(diff.y) > 0)
            {
                return new Vector2Int(0, diff.y > 0 ? 1 : -1);
            }

            return Vector2Int.zero;
        }

        void FindAndSetSmoothPath(Vector2Int targetTile)
        {
            if (bodyController == null) return;

            var headPos = bodyController.OccupiedTileController.WorldToGridPositionForward(bodyController.Segments[0].transform.position);
            var tailPos = bodyController.OccupiedTileController.WorldToGridPositionForward(bodyController.Segments[bodyController.Segments.Count - 1].transform.position);
            Vector2Int startPos = isDraggingFromHead ? headPos : tailPos;

            if (startPos == targetTile) return;

            // Get current map state
            bool[] mapState = GameMap.GetCurrentMapState();

            // Block tiles occupied by gecko segments
            for (int i = 0; i < bodyController.Segments.Count; i++)
            {
                Vector3 worldPos = bodyController.Segments[i].transform.position;
                Vector2Int gridPos = bodyController.OccupiedTileController.WorldToGridPosition(worldPos);

                int index = gridPos.y * GameMap.MapSize.x + gridPos.x;
                if (index >= 0 && index < mapState.Length)
                {
                    mapState[index] = false; // Block occupied tiles
                }
            }

            // Only unblock the start tile (head or tail depending on control)
            int startIndex = startPos.y * GameMap.MapSize.x + startPos.x;
            if (startIndex >= 0 && startIndex < mapState.Length)
            {
                mapState[startIndex] = true;
            }

            // Create pathfinder with updated grid
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            pathfinder.Reset();
            pathfinder.FindPath(startPos, targetTile, OnSmoothPathFound);
        }

        void OnSmoothPathFound(List<ASNode> path)
        {
            if (path == null || path.Count == 0)
            {
                DebugLog("No path found!");
                return;
            }

            DebugLog($"Raw path found with {path.Count} steps");

            // Convert to coordinate list and remove starting position
            smoothPath.Clear();
            var headPos = bodyController.OccupiedTileController.WorldToGridPositionForward(bodyController.Segments[0].transform.position);
            var tailPos = bodyController.OccupiedTileController.WorldToGridPositionForward(bodyController.Segments[bodyController.Segments.Count - 1].transform.position);
            Vector2Int startPos = isDraggingFromHead ? headPos : tailPos;

            foreach (var node in path)
            {
                // Skip the starting position to avoid immediate completion
                if (node.Position != startPos)
                {
                    smoothPath.Add(node.Position);
                }
            }

            //string pathStr = string.Join(" -> ", smoothPath.Select(n => $"{n.x.ToString()} {n.y.ToString()}"));
            //Debug.Log($"Full path: {pathStr}, Start: {startPos}");


            if (smoothPath.Count == 0)
            {
                DebugLog("No movement needed - already at target");
                return;
            }

            // Execute path
            ExecuteSmoothPath(smoothPath);
        }

        void ExecuteSmoothPath(List<Vector2Int> path)
        {
            if (bodyController == null || path == null || path.Count == 0)
            {
                DebugLog("Cannot execute path: invalid state");
                return;
            }

            DebugLog($"Executing smooth path with {path.Count} points for {(isDraggingFromHead ? "HEAD" : "TAIL")} control");

            bodyController.ClearPath();
            bodyController.SetMovementPath(path);

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
                        return null;
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

        BodyController GetBodyControllerByMouse(Vector2 screenPosition)
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