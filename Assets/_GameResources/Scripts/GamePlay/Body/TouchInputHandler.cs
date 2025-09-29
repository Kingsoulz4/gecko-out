using Geckout.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static DG.Tweening.DOTweenAnimation;
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
        private ControlAnchor touchAnchor = ControlAnchor.Head;

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
                DebugLog("OnTouchStart No tile found at touch position");
                return;
            }

            DebugLog($"OnTouchStart Touch at tile coordinate: {tileCoord.Value}");

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
                    DebugLog($"OnTouchStart Found adjacent {anchor} at {anchorPos} for target {tileCoord.Value}");
                    StartAutomaticMovement(gecko, anchor, tileCoord.Value);
                }
                else
                {
                    DebugLog("OnTouchStart No adjacent head/tail found within 1 tile distance");
                }
            }
            else
            {
                DebugLog("OnTouchStart Tile is occupied or invalid");
            }
        }

        void HandleDirectBodyTouch(BodyController body, Vector2Int tileCoord)
        {
            Vector2Int headCoord = body.Segments[0].Coordinate;
            Vector2Int tailCoord = body.Segments[body.Segments.Count - 1].Coordinate;

            DebugLog($"HandleDirectBodyTouch head at: {headCoord}, tail at: {tailCoord}");

            // BLOCK: đang move theo HEAD thì không cho grab TAIL và ngược lại
            if (body.IsMoving)
            {
                if (body.controlAnchor == BodyController.ControlAnchor.Head && tileCoord == tailCoord)
                {
                    DebugLog("HandleDirectBodyTouch Blocked: cannot start dragging from TAIL while gecko is moving from HEAD");
                    return;
                }
                else if (body.controlAnchor == BodyController.ControlAnchor.Tail && tileCoord == headCoord)
                {
                    DebugLog("HandleDirectBodyTouch Blocked: cannot start dragging from HEAD while gecko is moving from TAIL");
                    return;
                }
                else
                {
                    DebugLog("HandleDirectBodyTouch Body is currently moving, but allowing path update from same anchor");
                }
            }

            if (tileCoord == headCoord)
            {
                DebugLog("HandleDirectBodyTouch Starting drag from HEAD");
                StartDragging(body, true);
            }
            else if (tileCoord == tailCoord)
            {
                DebugLog("HandleDirectBodyTouch Starting drag from TAIL");
                StartDragging(body, false);
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
                if (body.IsMoving)
                {
                    if ((body.controlAnchor == BodyController.ControlAnchor.Head && selectedAnchor == BodyController.ControlAnchor.Tail) ||
                        (body.controlAnchor == BodyController.ControlAnchor.Tail && selectedAnchor == BodyController.ControlAnchor.Head))
                    {
                        DebugLog($"Blocked: cannot start movement from {selectedAnchor} while gecko is moving from {body.controlAnchor}");
                        return;
                    }
                }

                DebugLog($"Body segment clicked, selected nearest anchor: {selectedAnchor} (head dist: {headDistance}, tail dist: {tailDistance})");

                // Bắt đầu drag mode với anchor đã chọn
                StartBodyDrag(body, selectedAnchor);
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

        void SetBody(BodyController body)
        {
            bodyController = body;
            bodyController.OnEndMove += OnBodyEndMove;
        }

        private void OnBodyEndMove()
        {
            bodyController.SetControlAnchor(touchAnchor);
            Debug.Log("OnBodyEndMove");
        }

        void StartBodyDrag(BodyController body, BodyController.ControlAnchor anchor)
        {
            touchAnchor = anchor;
            SetBody(body);
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

        void StartAutomaticMovement(BodyController body, BodyController.ControlAnchor anchor, Vector2Int targetTile)
        {
            touchAnchor = anchor;
            SetBody(body);
            isDragging = true;
            isDraggingFromHead = (anchor == BodyController.ControlAnchor.Head);

            // Set control anchor
            bodyController.SetControlAnchor(anchor);
            DebugLog($"StartAutomaticMovement movement started: {anchor} -> {targetTile} (drag mode enabled)");

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

            DebugLog($"OnTouchDrag to NEW target tile: {tileCoord.Value}");

            FindAndSetSmoothPath(tileCoord.Value);
        }

        void OnTouchEnd()
        {
            if (isDragging)
            {
                DebugLog("Drag ended");
            }

            isDragging = false;
            if (bodyController)
            {
                bodyController.OnEndMove -= OnBodyEndMove;
            }
            bodyController = null;
            currentPath?.Clear();
            smoothPath.Clear();
            lastTargetTile = Vector2Int.one * -1;
        }

        void StartDragging(BodyController body, bool fromHead)
        {
            touchAnchor = fromHead ? ControlAnchor.Head : ControlAnchor.Tail;
            SetBody(body);
            isDragging = true;
            isDraggingFromHead = fromHead;
            // Set control anchor based on drag source
            bodyController.SetControlAnchor(fromHead ? BodyController.ControlAnchor.Head : BodyController.ControlAnchor.Tail);
            DebugLog($"StartDragging from {(fromHead ? "HEAD" : "TAIL")}");

            // Initialize pathfinder
            bool[] mapState = GameMap.GetCurrentMapState();
            ASGrid grid = new ASGrid(GameMap.MapSize.x, GameMap.MapSize.y, mapState);
            pathfinder = new ASPathFinding(grid);
        }

        Vector2Int startPosCache;
        void FindAndSetSmoothPath(Vector2Int targetTile)
        {
            DebugLog("FindAndSetSmoothPath");
            if (bodyController == null || !bodyController.CanControl) return;

            if (IsPushTrigger(bodyController, isDraggingFromHead, targetTile))
            {
                DebugLog("PUSH FindAndSetSmoothPath trigger detected!");
                HandlePushMovement();
                return;
            }

            // BLOCK: không cho move vào tile occupied bởi body segments in normal move
            HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
            for (int i = 0; i < bodyController.Segments.Count; i++)
            {
                if (i % bodyController.SubLength == 0 || i == 0 || i == bodyController.Segments.Count - 1)
                {
                    occupiedTiles.Add(bodyController.Segments[i].Coordinate);
                }
            }

            if (occupiedTiles.Contains(targetTile))
            {
                DebugLog($"Blocked: Cannot move to occupied body tile {targetTile}");
                return;
            }

            isDraggingFromHead = (bodyController.controlAnchor == BodyController.ControlAnchor.Head);


            var headPos = GameMap.WorldToGridPositionForward(bodyController.Segments[0].transform.position, bodyController);
            var tailPos = GameMap.WorldToGridPositionForward(bodyController.Segments[bodyController.Segments.Count - 1].transform.position, bodyController);
            Vector2Int startPos = isDraggingFromHead ? headPos : tailPos;
            startPosCache = startPos;

            if (startPos == targetTile) return;

            // Get current map state
            bool[] mapState = GameMap.GetCurrentMapState();


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
                DebugLog("NORMAL No path found!");
                return;
            }

            DebugLog($"NORMAL OnSmoothPathFound {path.Count} steps");

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
                DebugLog("NORMAL No movement needed - already at target");
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

            // test set anchor here
            if (bodyController.IsMoving && bodyController.controlAnchor != touchAnchor)
            {
                return;
                //bodyController.SetControlAnchor(touchAnchor);
            }

            DebugLog("NORMAL execute path");

            bodyController.ClearPath();
            bodyController.StartMovePath(path);
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

        bool IsPushTrigger(BodyController body, bool isDragFromHead, Vector2Int targetTile)
        {
            if (body.Segments.Count <= 3) return false;

            Vector2Int anchorPos = touchAnchor == ControlAnchor.Head ?
        body.Segments[0].Coordinate : body.Segments[body.Segments.Count - 1].Coordinate;

            Vector2Int bodyDirection = touchAnchor == ControlAnchor.Head ?
                anchorPos - body.Segments[3].Coordinate :
                anchorPos - body.Segments[body.Segments.Count - 4].Coordinate;

            Vector2Int dragDirection = targetTile - anchorPos;

            // Convert to Vector2 for normalization
            Vector2 bodyDir = new Vector2(bodyDirection.x, bodyDirection.y);
            Vector2 dragDir = new Vector2(dragDirection.x, dragDirection.y);

            float dot = Vector2.Dot(bodyDir.normalized, dragDir.normalized);
            Debug.Log($"PUSH check - bodyDir: {bodyDir}, dragDir: {dragDir}, " +
                $"currentAchor: {touchAnchor}, dot: {dot}, anchorPos: {anchorPos}, dragTarget: {targetTile}");
            // Push nếu drag ngược hướng với body (dot < -0.5 = góc > 120 độ)
            bool isOppositeDirection = dot < -0.5f;

            // BLOCK: không cho move vào tile occupied bởi body segments in normal move
            HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
            for (int i = 0; i < bodyController.Segments.Count; i++)
            {
                if (i % bodyController.SubLength == 0 || i == 0 || i == bodyController.Segments.Count - 1)
                {
                    occupiedTiles.Add(bodyController.Segments[i].Coordinate);
                }
            }
            bool isWithinBodyRange = false;
            if (occupiedTiles.Contains(targetTile))
            {
                isWithinBodyRange = true;
            }

            return isOppositeDirection && isWithinBodyRange;
        }

        void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInput] {message}");
            }
        }

        #region Push Movement
        bool IsContinuousPush()
        {
            // Check nếu current movement cùng hướng với push intent
            return bodyController.controlAnchor != touchAnchor;
        }

        void HandlePushMovement()
        {
            if (bodyController == null) return;
            bool canPush = !bodyController.IsMoving || IsContinuousPush();

            if (!canPush)
            {
                DebugLog("PUSH blocked - conflicting movement");
                return;
            }

            // Calculate push distance từ original anchor position
            Vector2Int touchAnchorPos = touchAnchor == ControlAnchor.Head ?
                bodyController.Segments[0].Coordinate :
                bodyController.Segments[bodyController.Segments.Count - 1].Coordinate;

            Vector2Int pushDistance = lastTargetTile - touchAnchorPos;
            Debug.LogError($"PUSH lastTargetTile: {lastTargetTile}, originalAnchorPos: {touchAnchorPos}, pushDistance: {pushDistance}");
            int pushMagnitude = (Mathf.Abs(pushDistance.x) + Mathf.Abs(pushDistance.y));

            // Switch to opposite anchor
            var newAnchor = touchAnchor == ControlAnchor.Head ?
                BodyController.ControlAnchor.Tail : BodyController.ControlAnchor.Head;


            // Find target với same distance
            Vector2Int oppositeAnchorPos = newAnchor == ControlAnchor.Head ?
                bodyController.Segments[0].Coordinate :
                bodyController.Segments[bodyController.Segments.Count - 1].Coordinate;

            Vector2Int? targetTile = FindPushTargetWithDistance(oppositeAnchorPos, pushMagnitude);

            if (!targetTile.HasValue)
            {
                DebugLog("PUSH blocked - !targetTile.HasValue");
                return;
            }

            bodyController.SetControlAnchor(newAnchor);
            ExecutePushPath(targetTile.Value);
        }

        Vector2Int? FindPushTargetWithDistance(Vector2Int startPos, int targetDistance)
        {
            Vector2Int[] directions = {
        new Vector2Int(0, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };

            foreach (var direction in directions)
            {
                Vector2Int target = startPos + direction * targetDistance;
                GameMap.TryGetTileAtCoord(target, out GameTile tile);

                if (tile != null && !tile.IsOccupied)
                {
                    // Verify path is clear
                    bool pathClear = true;
                    for (int i = 1; i <= targetDistance; i++)
                    {
                        Vector2Int checkPos = startPos + direction * i;
                        GameMap.TryGetTileAtCoord(checkPos, out GameTile checkTile);
                        if (checkTile == null || checkTile.IsOccupied)
                        {
                            pathClear = false;
                            break;
                        }
                    }

                    if (pathClear)
                    {
                        DebugLog($"PUSH target: {target}, distance: {targetDistance}");
                        return target;
                    }
                }
            }

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
                DebugLog("PUSH pathfinding failed!");
                return;
            }

            DebugLog($"PUSH path found with {path.Count} steps");

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
            DebugLog($"PUSH Executing push movement with {pushPath.Count} steps");
            bodyController.ClearPath();
            bodyController.StartMovePath(pushPath);

        }
        #endregion
    }
}