using UnityEngine;
using UnityEngine.EventSystems;
using Geckout.PathFinding;
using System.Collections.Generic;

namespace Geckout
{
    public class TouchInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask tileLayerMask = 1;
        [SerializeField] private bool enableDebugLogs = true;

        private GeckoController targetGecko;
        private bool isDragging = false;
        private bool isDraggingFromHead = false;
        private Vector2Int lastTargetTile = Vector2Int.one * -1;
        private ASPathFinding pathfinder;

        private List<ASNode> currentPath;

        void Start()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;

            DebugLog("TouchInputHandler initialized");
            DebugLog($"Camera: {gameCamera.name}, Orthographic: {gameCamera.orthographic}");
        }

        void Update()
        {
            HandleTouchInput();
        }

        void HandleTouchInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                DebugLog("Mouse button down");
                OnTouchStart(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                OnTouchDrag(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                DebugLog("Mouse button up");
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

            GeckoController gecko = GetGeckoAtTile(tileCoord.Value);
            if (gecko == null)
            {
                DebugLog("No gecko found at tile");
                return;
            }

            if (gecko.IsMoving)
            {
                DebugLog("Gecko is currently moving, ignoring input");
                return;
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

            lastTargetTile = tileCoord.Value;
            DebugLog($"Dragging to NEW target tile: {tileCoord.Value}");

            // Cancel current movement and find new path
            targetGecko.ClearMoveQueue();
            FindAndExecutePath(tileCoord.Value);
        }

        void OnTouchEnd()
        {
            if (isDragging)
            {
                DebugLog("Drag ended");
            }
            isDragging = false;
            targetGecko = null;
            currentPath?.Clear();
            lastTargetTile = Vector2Int.one * -1;
        }

        void StartDragging(GeckoController gecko, bool fromHead)
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

        void FindAndExecutePath(Vector2Int targetTile)
        {
            if (targetGecko == null || pathfinder == null) return;

            Vector2Int startPos = isDraggingFromHead ?
                targetGecko.Segments[0].Coordinate :
                targetGecko.Segments[targetGecko.Segments.Count - 1].Coordinate;

            if (startPos == targetTile)
            {
                DebugLog("Target is same as current position, skipping");
                return;
            }

            DebugLog($"Pathfinding from {startPos} to {targetTile}");

            pathfinder.Reset();
            pathfinder.FindPath(startPos, targetTile, OnPathFound);
        }

        void OnPathFound(List<ASNode> path)
        {
            if (path == null || path.Count == 0)
            {
                DebugLog("No path found!");
                return;
            }

            DebugLog($"Path found with {path.Count} steps");
            for (int i = 0; i < path.Count; i++)
            {
                DebugLog($"  Step {i}: {path[i].Position}");
            }

            currentPath = path;

            DebugLog("About to execute entire path...");
            // Execute entire path instead of just first step
            ExecuteEntirePath(path);
        }

        void ExecuteEntirePath(List<ASNode> path)
        {
            DebugLog("ExecuteEntirePath called");

            if (targetGecko == null)
            {
                DebugLog("ERROR: targetGecko is null!");
                return;
            }

            // REMOVE the IsMoving check - allow path updates during movement
            // if (targetGecko.IsMoving)
            // {
            //     DebugLog("WARNING: targetGecko is moving, skipping");
            //     return;
            // }

            Vector2Int currentPos = isDraggingFromHead ?
                targetGecko.Segments[0].Coordinate :
                targetGecko.Segments[targetGecko.Segments.Count - 1].Coordinate;

            DebugLog($"Executing entire path from {currentPos}, dragging from: {(isDraggingFromHead ? "HEAD" : "TAIL")}");

            // Always clear existing queue and add new moves
            targetGecko.ClearMoveQueue();
            DebugLog("Queue cleared");

            Vector2Int lastPos = currentPos;
            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int delta = path[i].Position - lastPos;
                DebugLog($"Queueing move {i}: {lastPos} + {delta} = {path[i].Position}");
                targetGecko.QueueMove(delta);
                lastPos = path[i].Position;
            }

            DebugLog($"Total {path.Count} moves queued");
        }

        Vector2Int? GetTileCoordinateFromScreen(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            DebugLog($"Raycast from screen {screenPosition} to world ray: {ray.origin} dir: {ray.direction}");

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
            {
                DebugLog($"Raycast hit: {hit.collider.name} at {hit.point}");

                GameTile tile = hit.collider.transform.parent.GetComponent<GameTile>();
                if (tile != null)
                {
                    DebugLog($"Found tile at coordinate: {tile.Coordinate}");
                    return tile.Coordinate;
                }
                else
                {
                    DebugLog("Hit object's parent has no GameTile component");
                }
            }
            else
            {
                DebugLog($"Raycast missed (LayerMask: {tileLayerMask})");
            }

            return null;
        }

        GeckoController GetGeckoAtTile(Vector2Int coordinate)
        {
            GeckoController[] allGeckos = FindObjectsOfType<GeckoController>();
            DebugLog($"Found {allGeckos.Length} geckos in scene");

            foreach (var gecko in allGeckos)
            {
                foreach (var segment in gecko.Segments)
                {
                    if (segment.Coordinate == coordinate)
                    {
                        DebugLog($"Found gecko at coordinate {coordinate}");
                        return gecko;
                    }
                }
            }

            DebugLog($"No gecko found at coordinate {coordinate}");
            return null;
        }

        void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInput] {message}");
            }
        }

        // Visual debug
        void OnDrawGizmos()
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    Vector3 from = new Vector3(currentPath[i].Position.x, 0, currentPath[i].Position.y);
                    Vector3 to = new Vector3(currentPath[i + 1].Position.x, 0, currentPath[i + 1].Position.y);
                    Gizmos.DrawLine(from, to);
                }
            }

            // Draw last touch position
            if (isDragging)
            {
                Vector2Int pos = lastTargetTile;
                if (pos != Vector2Int.one * -1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(new Vector3(pos.x, 0, pos.y), Vector3.one * 0.5f);
                }
            }
        }
    }
}