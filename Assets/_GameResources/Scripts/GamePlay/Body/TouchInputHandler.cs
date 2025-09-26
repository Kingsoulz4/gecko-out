using Geckout.PathFinding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Geckout.BodyController;
using static UnityEngine.Rendering.HableCurve;

namespace Geckout
{
    public class TouchInputHandler : SingletonMono<TouchInputHandler>
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask tileLayerMask = 1;
        [SerializeField] private LayerMask segmentLayer = 7;
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private float pathUpdateInterval = 0.02f;

        private BodyController bodyController;
        private bool canClick = true;
        private bool isDragging = false;
        private bool isDraggingFromHead = false;
        private Vector2Int lastTargetTile = Vector2Int.one * -1;
        private ASPathFinding pathfinder;
        private float lastPathUpdateTime = 0f;
        private ControlAnchor controlAnchorOriginal = ControlAnchor.Head;

        private List<ASNode> currentPath;
        private List<Vector2Int> smoothPath = new List<Vector2Int>();

        public bool IsDragging { get => isDragging; }
        public bool CanClick { get => canClick; set => canClick = value; }

        void Start()
        {
            Application.targetFrameRate = 60;
            if (gameCamera == null)
                gameCamera = Camera.main;
        }

        void Update()
        {
            if (!LevelManager.Instance.IsEdittingLevel)
                if (canClick && GameManager.GameState == GameState.Playing)
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
            if (GameManager.GameState == GameState.Playing && !LevelManager.Instance.LevelGame.IsFirstClick)
            {
                LevelManager.Instance.LevelGame.IsFirstClick = true;
            }

            Vector2Int? tileCoord = GetTileCoordinateFromScreen(screenPosition);
            if (!tileCoord.HasValue)
            {
                DebugLog("No tile found at touch position");
                return;
            }

            DebugLog($"Touch at tile coordinate: {tileCoord.Value}");

            // Kiểm tra xem có click trực tiếp lên segment không
            var body = GetBodyControllerByMouse(screenPosition);
            if (body != null)
            {
                HandleDirectBodyTouch(body, tileCoord.Value);
                return;
            }

            // Kiểm tra tile trống + tìm head/tail adjacent
            if (GameMap.TryGetTileAtCoord(tileCoord.Value, out GameTile tile) && !tile.IsOccupied)
            {
                var adjacentResult = FindAdjacentGeckoAnchor(tileCoord.Value);
                if (adjacentResult.HasValue)
                {
                    var (gecko, anchor, anchorPos) = adjacentResult.Value;
                    DebugLog($"Found adjacent {anchor} at {anchorPos} for target {tileCoord.Value}");
                    StartAutomaticMovement(gecko, anchor, tileCoord.Value);
                }
                else
                {
                    DebugLog("No adjacent head/tail found within 1 tile distance");
                }
            }
            else
            {
                DebugLog("Tile is occupied or invalid");
            }
        }

        void HandleDirectBodyTouch(BodyController gecko, Vector2Int tileCoord)
        {
            Vector2Int headCoord = gecko.Segments[0].Coordinate;
            Vector2Int tailCoord = gecko.Segments[gecko.Segments.Count - 1].Coordinate;

            DebugLog($"Gecko head at: {headCoord}, tail at: {tailCoord}");

            // BLOCK: đang move theo HEAD thì không cho grab TAIL và ngược lại
            if (gecko.IsMoving)
            {
                if (gecko.controlAnchor == BodyController.ControlAnchor.Head && tileCoord == tailCoord)
                {
                    DebugLog("Blocked: cannot start dragging from TAIL while gecko is moving from HEAD");
                    return;
                }
                else if (gecko.controlAnchor == BodyController.ControlAnchor.Tail && tileCoord == headCoord)
                {
                    DebugLog("Blocked: cannot start dragging from HEAD while gecko is moving from TAIL");
                    return;
                }
                else
                {
                    DebugLog("Gecko is currently moving, but allowing path update from same anchor");
                }
            }

            if (tileCoord == headCoord)
            {
                DebugLog("Starting drag from HEAD");
                StartDragging(gecko, true);
            }
            else if (tileCoord == tailCoord)
            {
                DebugLog("Starting drag from TAIL");
                StartDragging(gecko, false);
            }
            else
            {
                // Xử lý click vào body segment (không phải head/tail)
                // Tính anchor gần nhất với vị trí click
                float headDistance = Vector2Int.Distance(headCoord, tileCoord);
                float tailDistance = Vector2Int.Distance(tailCoord, tileCoord);

                bool useHead = headDistance <= tailDistance;
                BodyController.ControlAnchor selectedAnchor = useHead ?
                    BodyController.ControlAnchor.Head : BodyController.ControlAnchor.Tail;

                // BLOCK: kiểm tra movement conflict cho body segment touch
                if (gecko.IsMoving)
                {
                    if ((gecko.controlAnchor == BodyController.ControlAnchor.Head && selectedAnchor == BodyController.ControlAnchor.Tail) ||
                        (gecko.controlAnchor == BodyController.ControlAnchor.Tail && selectedAnchor == BodyController.ControlAnchor.Head))
                    {
                        DebugLog($"Blocked: cannot start movement from {selectedAnchor} while gecko is moving from {gecko.controlAnchor}");
                        return;
                    }
                }

                DebugLog($"Body segment clicked, selected nearest anchor: {selectedAnchor} (head dist: {headDistance}, tail dist: {tailDistance})");

                // Bắt đầu drag mode với anchor đã chọn
                StartBodyDrag(gecko, selectedAnchor);
            }
        }

        private (BodyController gecko, BodyController.ControlAnchor anchor, Vector2Int anchorPos)?
            FindAdjacentGeckoAnchor(Vector2Int targetTile)
        {
            // Tìm tất cả gecko trong scene
            List<BodyController> bodies = LevelManager.Instance.LevelGame.ListBody;

            foreach (var body in bodies)
            {
                if (body.IsMoving) continue; // Skip gecko đang di chuyển

                var headPos = GameMap.WorldToGridPosition(
                    body.Segments[0].transform.position);
                var tailPos = GameMap.WorldToGridPosition(
                    body.Segments[body.Segments.Count - 1].transform.position);

                // Kiểm tra head có adjacent với target không
                if (IsAdjacent(headPos, targetTile))
                {
                    DebugLog($"Found adjacent HEAD at {headPos}, target: {targetTile}");
                    return (body, BodyController.ControlAnchor.Head, headPos);
                }

                // Kiểm tra tail có adjacent với target không  
                if (IsAdjacent(tailPos, targetTile))
                {
                    DebugLog($"Found adjacent TAIL at {tailPos}, target: {targetTile}");
                    return (body, BodyController.ControlAnchor.Tail, tailPos);
                }
            }

            return null; // Không tìm thấy head/tail nào trong vòng 1 ô
        }

        private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            // Kiểm tra 8 hướng (bao gồm chéo): Chebyshev distance = 1
            int deltaX = Mathf.Abs(pos1.x - pos2.x);
            int deltaY = Mathf.Abs(pos1.y - pos2.y);

            // Adjacent nếu cả deltaX và deltaY <= 1, và ít nhất 1 trong 2 != 0
            bool adjacent = (deltaX <= 1 && deltaY <= 1) && (deltaX != 0 || deltaY != 0);

            if (enableDebugLogs && adjacent)
            {
                DebugLog($"Adjacent check (8-dir): {pos1} -> {pos2}, delta: ({deltaX}, {deltaY})");
            }

            return adjacent;
        }

        void StartBodyDrag(BodyController gecko, BodyController.ControlAnchor anchor)
        {
            bodyController = gecko;
            isDragging = true;
            isDraggingFromHead = (anchor == BodyController.ControlAnchor.Head);

            // Set control anchor
            bodyController.SetControlAnchor(anchor);
            DebugLog($"Started body drag with {anchor} anchor");

            // Initialize pathfinder
            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            // Reset target để OnTouchDrag có thể handle
            lastTargetTile = Vector2Int.one * -1;
            lastPathUpdateTime = 0f;
        }

        void StartAutomaticMovement(BodyController gecko, BodyController.ControlAnchor anchor, Vector2Int targetTile)
        {
            bodyController = gecko;
            isDragging = true; // Enable drag mode ngay để có thể continue drag
            isDraggingFromHead = (anchor == BodyController.ControlAnchor.Head);

            // Set control anchor
            bodyController.SetControlAnchor(anchor);
            DebugLog($"Auto movement started: {anchor} -> {targetTile} (drag mode enabled)");

            // Initialize pathfinder
            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            // Tìm path và di chuyển ngay
            FindAndSetSmoothPath(targetTile);

            // Set target để drag system có thể continue
            lastTargetTile = targetTile;
            lastPathUpdateTime = Time.time;
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

            if (IsPushTrigger(bodyController, isDraggingFromHead, tileCoord.Value))
            {
                DebugLog("Push trigger detected!");
                HandlePushMovement();
                return;
            }
            FindAndSetSmoothPath(tileCoord.Value);
        }

        void OnTouchEnd()
        {
            if (isDragging)
            {
                DebugLog("Drag ended");
            }

            isDragging = false;
            bodyController = null;
            currentPath?.Clear();
            smoothPath.Clear();
            lastTargetTile = Vector2Int.one * -1;
        }

        void StartDragging(BodyController gecko, bool fromHead)
        {
            controlAnchorOriginal = fromHead? ControlAnchor.Head : ControlAnchor.Tail;
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
        }

        Vector2Int startPosCache;
        void FindAndSetSmoothPath(Vector2Int targetTile)
        {
            Debug.Log($"11111StartDragging called for gecko:fromHead: {controlAnchorOriginal}");
            if (bodyController == null || !bodyController.CanControl) return;

            if (!bodyController.IsMoving && !IsPushTrigger(bodyController, isDraggingFromHead, targetTile))
            {
                bodyController.SetControlAnchor(controlAnchorOriginal);
            }

            isDraggingFromHead = (bodyController.controlAnchor == BodyController.ControlAnchor.Head);


            var headPos = GameMap.WorldToGridPositionForward(bodyController.Segments[0].transform.position, bodyController);
            var tailPos = GameMap.WorldToGridPositionForward(bodyController.Segments[bodyController.Segments.Count - 1].transform.position, bodyController);
            Vector2Int startPos = isDraggingFromHead ? headPos : tailPos;
            startPosCache = startPos;

            if (startPos == targetTile) return;

            // Get current map state
            bool[] mapState = GameMap.GetCurrentMapState();

            // Block tiles occupied by gecko segments
            for (int i = 0; i < bodyController.Segments.Count; i++)
            {
                if (i % bodyController.SubLength != 0 && i != 0 && i != bodyController.Segments.Count - 1)
                {
                    continue;
                }
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

            foreach (var node in path)
            {
                // Skip the starting position to avoid immediate completion
                if (node.Position != startPosCache)
                {
                    smoothPath.Add(node.Position);
                }
            }

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
            bodyController.StartMovePath(path);

            DebugLog("Smooth path movement started");
        }

        Vector2Int? GetTileCoordinateFromScreen(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
            {
                GameTile tile = hit.collider.transform.GetComponent<GameTile>();
                if (tile != null)
                {
                    return tile.Coordinate;
                }
                else
                {
                    DebugLog("Raycast hit but no GameTile component found");
                }
            }
            return null;
        }

        public BodyController GetBodyControllerByMouse(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, segmentLayer))
            {
                // Try direct component on hit object
                Segment segment = hit.collider.GetComponent<Segment>();
                var segments = segment.Controller.Segments;

                if (segment != null
                    && (segment.CurrentTile.Coordinate == segments[0].Coordinate
                    || segment.CurrentTile.Coordinate == segments[segments.Count - 1].Coordinate)
                    )
                {
                    DebugLog($"Found segment: {segment.name}, Controller: {segment.Controller?.name}");
                    return segment.Controller;
                }
                else
                {
                    DebugLog("No Segment component found in hit object hierarchy");
                }
            }
            else
            {
                DebugLog($"No raycast hit on segmentLayer ({segmentLayer})");
            }
            return null;
        }

        bool IsPushTrigger(BodyController gecko, bool isDragFromHead, Vector2Int dragTarget)
        {
            if (isDragFromHead && gecko.Segments.Count > 3)
            {
                Vector2Int targetSegmentCoord = gecko.Segments[3].Coordinate;
                return dragTarget == targetSegmentCoord;
            }
            else if (!isDragFromHead && gecko.Segments.Count > 4)
            {
                Vector2Int targetSegmentCoord = gecko.Segments[gecko.Segments.Count - 4].Coordinate;
                return dragTarget == targetSegmentCoord;
            }
            return false;
        }

        void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInput] {message}");
            }
        }

        #region Push Movement
        void HandlePushMovement()
        {
            if (bodyController == null) return;
            if (bodyController.IsMoving)
            {
                DebugLog("Push blocked - body is currently moving");
                return;
            }

            var newAnchor = isDraggingFromHead ?
                BodyController.ControlAnchor.Tail : BodyController.ControlAnchor.Head;

            var oldAnchor = bodyController.controlAnchor;
            bodyController.SetControlAnchor(newAnchor);

            // DON'T update isDraggingFromHead here - keep it for chain push detection

            Vector2Int oppositeAnchorPos;
            if (newAnchor == BodyController.ControlAnchor.Head)
            {
                oppositeAnchorPos = GameMap.WorldToGridPosition(bodyController.Segments[0].transform.position);
            }
            else
            {
                oppositeAnchorPos = GameMap.WorldToGridPosition(
                    bodyController.Segments[bodyController.Segments.Count - 1].transform.position);
            }

            Vector2Int? targetTile = FindPushTarget(oppositeAnchorPos);

            if (!targetTile.HasValue)
            {
                bodyController.SetControlAnchor(oldAnchor);
                return;
            }

            ExecutePushPath(targetTile.Value);
        }

        Vector2Int? FindPushTarget(Vector2Int startPos)
        {
            Vector2Int[] directions = {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

            foreach (var direction in directions)
            {
                Vector2Int candidate = startPos + direction;

                // Check if tile is valid and empty
                GameMap.TryGetTileAtCoord(candidate, out GameTile tile);
                if (tile != null && !tile.IsOccupied)
                {
                    DebugLog($"Found valid neighbor push target: {candidate}");
                    return candidate;
                }
            }

            DebugLog("No valid neighbor tile found for push");
            return null;
        }

        void ExecutePushPath(Vector2Int targetTile)
        {
            if (bodyController == null) return;

            // Get current anchor position based on new control anchor
            Vector2Int startPos;
            if (bodyController.controlAnchor == BodyController.ControlAnchor.Head)
            {
                startPos = GameMap.WorldToGridPositionForward(bodyController.Segments[0].transform.position, bodyController);
            }
            else
            {
                startPos = GameMap.WorldToGridPositionForward(
                    bodyController.Segments[bodyController.Segments.Count - 1].transform.position, bodyController);
            }

            // Initialize pathfinder with current map state
            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);

            // Find and execute path
            pathfinder.Reset();
            pathfinder.FindPath(startPos, targetTile, OnPushPathFound);
        }

        void OnPushPathFound(List<ASNode> path)
        {
            if (path == null || path.Count == 0)
            {
                DebugLog("Push pathfinding failed!");
                return;
            }

            DebugLog($"Push path found with {path.Count} steps");

            // Convert to coordinate list and remove starting position
            List<Vector2Int> pushPath = new List<Vector2Int>();
            Vector2Int currentAnchorPos;

            if (bodyController.controlAnchor == BodyController.ControlAnchor.Head)
            {
                currentAnchorPos = GameMap.WorldToGridPositionForward(bodyController.Segments[0].transform.position, bodyController);
            }
            else
            {
                currentAnchorPos = GameMap.WorldToGridPositionForward(
                    bodyController.Segments[bodyController.Segments.Count - 1].transform.position, bodyController);
            }

            foreach (var node in path)
            {
                // Skip the starting position
                if (node.Position != currentAnchorPos)
                {
                    pushPath.Add(node.Position);
                }
            }

            if (pushPath.Count == 0)
            {
                DebugLog("No movement needed for push - already at target");
                return;
            }

            // Execute the push movement
            DebugLog($"Executing push movement with {pushPath.Count} steps");
            bodyController.ClearPath();
            bodyController.StartMovePath(pushPath);

        }
        #endregion
    }
}