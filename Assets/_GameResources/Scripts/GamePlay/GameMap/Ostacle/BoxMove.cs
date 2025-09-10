using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BoxMove : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private LayerMask moveBoxLayer;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float inputThreshold = 0.5f;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Visual Prefabs")]
        [SerializeField] private GameObject m_tileCornerPrefab;
        [SerializeField] private GameObject m_tileEdgePrefab;
        [SerializeField] private GameObject m_tileCenterPrefab;
        [SerializeField] private GameObject m_tileCorner3EdgePrefab;
        [SerializeField] private GameObject m_tile2EdgePrefab;
        [SerializeField] private GameObject m_tile4EdgePrefab;

        [Header("Debug")]
        [SerializeField] private bool enablePositionDebug = true;

        [SerializeField] private WayDirection wayDirection = WayDirection.All;
        private MovableBoxData movableBoxData = new();
        private List<MovableBoxTile> listMovableBoxTile = new();

        // Input state
        private bool isDragging = false;
        private Vector2 dragStartPosition;
        private Vector2Int dragStartCoordinate;
        private Camera gameCamera;
        private bool isMoving = false;
        private Coroutine moveCoroutine;
        private Vector2Int selectedTileOffset = Vector2Int.zero;
        // Sequential movement
        private Queue<Vector2Int> movementQueue = new Queue<Vector2Int>();

        public bool IsDragging => isDragging;
        public bool IsMoving => isMoving;
        public MovableBoxData MovableBoxData => movableBoxData;

        private void Start()
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
                gameCamera = FindObjectOfType<Camera>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Check mouse input (for editor)
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 currentInputPos = Input.mousePosition;
                if (IsTouchOverBox(currentInputPos))
                {
                    StartDrag(currentInputPos);
                }
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                Vector2 currentInputPos = Input.mousePosition;
                EndDrag(currentInputPos);
            }

            // Check touch input (for mobile)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 currentInputPos = touch.position;

                if (touch.phase == TouchPhase.Began)
                {
                    if (IsTouchOverBox(currentInputPos))
                    {
                        StartDrag(currentInputPos);
                    }
                }
                else if (touch.phase == TouchPhase.Ended && isDragging)
                {
                    EndDrag(currentInputPos);
                }
            }
        }

        private bool IsTouchOverBox(Vector2 screenPosition)
        {
            if (gameCamera == null) return false;

            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moveBoxLayer))
            {
                return hit.transform == transform || hit.transform.IsChildOf(transform);
            }
            return false;
        }

        private void StartDrag(Vector2 inputPosition)
        {
            if (isMoving) return;

            // Tìm ô con được click
            var clickedTile = GetTileUnderScreenPosition(inputPosition);
            if (clickedTile == null)
            {
                // fallback: không tìm thấy ô con thì bỏ (hoặc cho phép kéo theo root cũ)
                return;
            }

            isDragging = true;
            dragStartPosition = inputPosition;
            dragStartCoordinate = movableBoxData.rootCoordinate;

            // offset = (toạ độ ô con) - (toạ độ root)
            selectedTileOffset = clickedTile.Coordinate - movableBoxData.rootCoordinate;

            DebugLog($"Started dragging from {dragStartPosition}, root={dragStartCoordinate}, clickedTile={clickedTile.Coordinate}, offset={selectedTileOffset}");
        }


        private void EndDrag(Vector2 inputPosition)
        {
            if (!isDragging) return;

            Vector2Int targetCoordForSelectedTile = ScreenToGridCoordinate(inputPosition);

            // Quy đổi về toạ độ root mục tiêu
            Vector2Int rootTarget = targetCoordForSelectedTile - selectedTileOffset;

            DebugLog($"End drag. start={dragStartPosition} -> end={inputPosition}");
            DebugLog($"selectedTile wants {targetCoordForSelectedTile}, rootTarget={rootTarget}, currentRoot={movableBoxData.rootCoordinate}");

            isDragging = false;

            // Validate & move
            if (rootTarget != movableBoxData.rootCoordinate && IsValidTargetPosition(rootTarget))
            {
                StartSequentialMovement(rootTarget);
            }
            else
            {
                DebugLog($"Invalid rootTarget: {rootTarget}");
            }
        }


        private Vector2Int ScreenToGridCoordinate(Vector2 screenPosition)
        {
            // Cast ray từ screen position vào world
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            // Tìm intersection với plane z=0 (assuming tiles nằm trên plane này)
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector2Int gridCoord = GameMap.WorldToGridPosition(worldPoint);

                DebugLog($"Screen {screenPosition} -> World {worldPoint} -> Grid {gridCoord}");
                return gridCoord;
            }

            // Fallback: return current coordinate
            DebugLog($"Failed to convert screen position {screenPosition} to grid");
            return movableBoxData.rootCoordinate;
        }

        private bool IsValidTargetPosition(Vector2Int targetCoord)
        {
            Vector2Int mapSize = GameMap.MapSize;
            Vector2Int boxSize = movableBoxData.boxSize;

            DebugLog($"=== BOUNDS CHECK ===");
            DebugLog($"Target coord: {targetCoord}");
            DebugLog($"Box size: {boxSize}");
            DebugLog($"Map size: {mapSize}");
            DebugLog($"Target + Box size: ({targetCoord.x + boxSize.x}, {targetCoord.y + boxSize.y})");

            // Check bounds - Box takes up space from targetCoord to targetCoord + boxSize
            bool xInBounds = targetCoord.x >= 0 && (targetCoord.x + boxSize.x) <= mapSize.x;
            bool yInBounds = targetCoord.y >= 0 && (targetCoord.y + boxSize.y) <= mapSize.y;

            DebugLog($"X bounds check: {targetCoord.x} >= 0 && {targetCoord.x + boxSize.x} <= {mapSize.x} = {xInBounds}");
            DebugLog($"Y bounds check: {targetCoord.y} >= 0 && {targetCoord.y + boxSize.y} <= {mapSize.y} = {yInBounds}");

            if (!xInBounds || !yInBounds)
            {
                DebugLog($"Target {targetCoord} is out of bounds");
                return false;
            }

            // Apply direction constraints
            Vector2Int startCoord = dragStartCoordinate;
            switch (wayDirection)
            {
                case WayDirection.Horizontal:
                    if (targetCoord.y != startCoord.y)
                    {
                        DebugLog($"Horizontal constraint violated: {targetCoord} vs {startCoord}");
                        return false;
                    }
                    break;
                case WayDirection.Vertical:
                    if (targetCoord.x != startCoord.x)
                    {
                        DebugLog($"Vertical constraint violated: {targetCoord} vs {startCoord}");
                        return false;
                    }
                    break;
            }

            DebugLog($"Target {targetCoord} is valid");
            return true;
        }

        private void StartSequentialMovement(Vector2Int targetCoordinate)
        {
            // Clear existing movement queue
            movementQueue.Clear();

            // If already moving, let current movement finish, then build new path from where we'll be
            Vector2Int startCoordinate = isMoving ?
                movableBoxData.rootCoordinate : // Will be updated by current movement
                movableBoxData.rootCoordinate;

            // Build path from current to target
            List<Vector2Int> path = BuildMovementPath(startCoordinate, targetCoordinate);

            if (path.Count == 0)
            {
                DebugLog("No valid path found");
                return;
            }

            // Add to movement queue
            foreach (var coord in path)
            {
                movementQueue.Enqueue(coord);
            }

            DebugLog($"Updated movement queue with {movementQueue.Count} steps to {targetCoordinate}");

            // Start moving if not already moving
            if (!isMoving)
            {
                ProcessNextMovement();
            }
        }

        private List<Vector2Int> BuildMovementPath(Vector2Int start, Vector2Int target)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int current = start;

            // Simple path: move one axis at a time
            // Move horizontally first, then vertically
            while (current.x != target.x)
            {
                int step = current.x < target.x ? 1 : -1;
                Vector2Int nextPos = current + new Vector2Int(step, 0);

                if (CanMoveToPosition(nextPos))
                {
                    path.Add(nextPos);
                    current = nextPos;
                }
                else
                {
                    DebugLog($"Path blocked at {nextPos}");
                    break; // Path blocked
                }
            }

            while (current.y != target.y)
            {
                int step = current.y < target.y ? 1 : -1;
                Vector2Int nextPos = current + new Vector2Int(0, step);

                if (CanMoveToPosition(nextPos))
                {
                    path.Add(nextPos);
                    current = nextPos;
                }
                else
                {
                    DebugLog($"Path blocked at {nextPos}");
                    break; // Path blocked
                }
            }

            DebugLog($"Built path with {path.Count} steps: {string.Join(" -> ", path)}");
            return path;
        }

        private void ProcessNextMovement()
        {
            if (movementQueue.Count == 0)
            {
                DebugLog("Sequential movement completed");
                return;
            }

            Vector2Int nextCoordinate = movementQueue.Dequeue();

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            moveCoroutine = StartCoroutine(AnimateMove(nextCoordinate));
        }

        private bool CanMoveToPosition(Vector2Int targetRootPos)
        {
            // Check all tiles that would be occupied at target position
            for (int x = 0; x < movableBoxData.boxSize.x; x++)
            {
                for (int y = 0; y < movableBoxData.boxSize.y; y++)
                {
                    Vector2Int checkPos = targetRootPos + new Vector2Int(x, y);

                    // Check bounds
                    if (!IsWithinMapBounds(checkPos))
                    {
                        return false;
                    }

                    // Check occupation (excluding current box tiles)
                    if (IsTileOccupiedByOther(checkPos))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private MovableBoxTile GetTileUnderScreenPosition(Vector2 screenPosition)
        {
            if (gameCamera == null) return null;

            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moveBoxLayer))
            {
                // Ưu tiên component trên chính collider
                var tile = hit.transform.GetComponent<MovableBoxTile>();
                if (tile != null) return tile;

                // Nếu collider không gắn trực tiếp, tìm lên cha
                return hit.transform.GetComponentInParent<MovableBoxTile>();
            }
            return null;
        }

        private bool IsWithinMapBounds(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.y >= 0 &&
                   gridPos.x < GameMap.MapSize.x && gridPos.y < GameMap.MapSize.y;
        }

        private bool IsTileOccupiedByOther(Vector2Int gridPos)
        {
            if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(gridPos, out var tile))
            {
                // Check if this tile is currently occupied by this box
                bool isOccupiedByThisBox = false;
                foreach (var boxTile in listMovableBoxTile)
                {
                    if (boxTile.Coordinate == gridPos)
                    {
                        isOccupiedByThisBox = true;
                        break;
                    }
                }

                return tile.IsOccupied && !isOccupiedByThisBox;
            }
            return true;
        }

        #region SEQUENTIAL MOVEMENT ANIMATION

        private IEnumerator AnimateMove(Vector2Int targetGridPos)
        {
            isMoving = true;

            Vector3 startPos = transform.position;
            Vector3 targetWorldPos = GetWorldPositionFromGrid(targetGridPos);

            DebugLog($"Animating step: {movableBoxData.rootCoordinate} -> {targetGridPos}");
            DebugLog($"World positions: {startPos} -> {targetWorldPos}");

            // Clear current occupation before moving
            ClearCurrentOccupation();

            // Animate movement
            float journey = 0f;
            while (journey < 1f)
            {
                journey = Mathf.Min(1f, journey + moveSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(startPos, targetWorldPos, journey);
                yield return null;
            }

            // Ensure exact final position
            transform.position = targetWorldPos;

            // Update logical coordinates
            UpdateBoxLogicalPosition(targetGridPos);

            isMoving = false;
            moveCoroutine = null;

            // Continue to next movement
            ProcessNextMovement();
        }

        private void UpdateBoxLogicalPosition(Vector2Int newRootPos)
        {
            Vector2Int offset = newRootPos - movableBoxData.rootCoordinate;
            movableBoxData.rootCoordinate = newRootPos;

            // Update logical coordinates and set occupation
            foreach (var boxTile in listMovableBoxTile)
            {
                boxTile.Coordinate += offset;

                if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(boxTile.Coordinate, out var tile))
                {
                    tile.IsOccupied = true;
                }
            }

            DebugLog($"Box moved to {newRootPos}, parent position: {transform.position}");
        }

        private Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
        {
            if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(gridPos, out var tile))
            {
                DebugLog($"Found tile at {gridPos}: {tile.transform.position}");
                return tile.transform.position;
            }

            Debug.LogError($"Could not find tile at {gridPos}");
            return GameMap.GetTileWorldPosition(gridPos);
        }

        #endregion

        private void ClearCurrentOccupation()
        {
            foreach (var boxTile in listMovableBoxTile)
            {
                if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(boxTile.Coordinate, out var tile))
                {
                    tile.IsOccupied = false;
                }
            }
        }

        #region PUBLIC API

        public void Init(MovableBoxData movableBoxData)
        {
            this.movableBoxData = movableBoxData;
            SpawnTiles();

            // Set initial parent position to match root coordinate
            if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate, out var rootTile))
            {
                transform.position = rootTile.transform.position;
            }
        }

        // Method để force stop movement nếu cần
        public void StopMovement()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            movementQueue.Clear();
            isMoving = false;
            isDragging = false;
        }

        #endregion

        #region TILE SPAWNING METHODS

        void SpawnTiles()
        {
            var boxSize = movableBoxData.boxSize;
            bool needSetOccupied = false;
            if (LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Contains(movableBoxData))
                needSetOccupied = true;

            if (movableBoxData.boxSize.x == 1 && movableBoxData.boxSize.y == 1)
            {
                SpawnSingleTile(needSetOccupied);
            }
            else if (movableBoxData.boxSize.x == 1)
            {
                SpawnVerticalTiles(needSetOccupied);
            }
            else if (movableBoxData.boxSize.y == 1)
            {
                SpawnHorizontalTiles(needSetOccupied);
            }
            else
            {
                SpawnRectangleTiles(needSetOccupied);
            }
        }

        private void SpawnSingleTile(bool needSetOccupied)
        {
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate, out var tile);
            GameObject obj = Instantiate(m_tile4EdgePrefab, transform);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.name = "Tile";
            obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);

            var movableBoxTile = obj.AddComponent<MovableBoxTile>();
            movableBoxTile.Coordinate = tile.MapTileData.coordinate;
            listMovableBoxTile.Add(movableBoxTile);
            if (needSetOccupied) tile.IsOccupied = true;
        }

        private void SpawnVerticalTiles(bool needSetOccupied)
        {
            var boxSize = movableBoxData.boxSize;
            for (int y = 0; y < boxSize.y; y++)
            {
                Vector2Int c = new Vector2Int(0, y);
                var prefab = (y == 0 || y == boxSize.y - 1) ? m_tileCorner3EdgePrefab : m_tile2EdgePrefab;

                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate + c, out var tile);
                GameObject obj = Instantiate(prefab, transform);

                obj.transform.localPosition = new Vector3(c.x, c.y, 0);
                obj.transform.localScale = Vector3.one;
                obj.name = $"Tile_{c.x}_{c.y}";

                if (y == 0)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));
                else if (y == boxSize.y - 1)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, -90, 90));
                else
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));

                var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                listMovableBoxTile.Add(movableBoxTile);
                if (needSetOccupied) tile.IsOccupied = true;
            }
        }

        private void SpawnHorizontalTiles(bool needSetOccupied)
        {
            var boxSize = movableBoxData.boxSize;
            for (int x = 0; x < boxSize.x; x++)
            {
                Vector2Int c = new Vector2Int(x, 0);
                var prefab = (x == 0 || x == boxSize.x - 1) ? m_tileCorner3EdgePrefab : m_tile2EdgePrefab;

                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate + c, out var tile);
                GameObject obj = Instantiate(prefab, transform);

                obj.transform.localPosition = new Vector3(c.x, c.y, 0);
                obj.transform.localScale = Vector3.one;
                obj.name = $"Tile_{c.x}_{c.y}";

                if (x == 0)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, 90, -90));
                else if (x == boxSize.x - 1)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, -90, 90));
                else
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, 90, -90));

                var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                listMovableBoxTile.Add(movableBoxTile);
                if (needSetOccupied) tile.IsOccupied = true;
            }
        }

        private void SpawnRectangleTiles(bool needSetOccupied)
        {
            var boxSize = movableBoxData.boxSize;
            for (int x = 0; x < boxSize.x; x++)
            {
                for (int y = 0; y < boxSize.y; y++)
                {
                    Vector2Int c = new Vector2Int(x, y);
                    LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate + c, out var tile);

                    GameObject obj = CreateTileForPosition(x, y, boxSize);

                    obj.transform.localPosition = new Vector3(c.x, c.y, 0);
                    obj.transform.localScale = Vector3.one;
                    obj.name = $"Tile_{c.x}_{c.y}";

                    var movableBoxTile = obj.AddComponent<MovableBoxTile>();
                    movableBoxTile.Coordinate = tile.MapTileData.coordinate;
                    listMovableBoxTile.Add(movableBoxTile);
                    if (needSetOccupied) tile.IsOccupied = true;
                }
            }
        }

        private GameObject CreateTileForPosition(int x, int y, Vector2Int boxSize)
        {
            GameObject obj = null;

            if (x == 0 && y == 0)
            {
                obj = Instantiate(m_tileCornerPrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(0, 90, -90);
            }
            else if (x == 0 && y == boxSize.y - 1)
            {
                obj = Instantiate(m_tileCornerPrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(90, 90, -90);
            }
            else if (x == boxSize.x - 1 && y == 0)
            {
                obj = Instantiate(m_tileCornerPrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(-90, -90, 90);
            }
            else if (x == boxSize.x - 1 && y == boxSize.y - 1)
            {
                obj = Instantiate(m_tileCornerPrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(0, -90, 90);
            }
            else if (x == 0)
            {
                obj = Instantiate(m_tileEdgePrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(90, 90, -90);
            }
            else if (x == boxSize.x - 1)
            {
                obj = Instantiate(m_tileEdgePrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(-90, -90, 90);
            }
            else if (y == 0)
            {
                obj = Instantiate(m_tileEdgePrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(-180, -90, 90);
            }
            else if (y == boxSize.y - 1)
            {
                obj = Instantiate(m_tileEdgePrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(-180, 90, -90);
            }
            else
            {
                obj = Instantiate(m_tileCenterPrefab, transform);
                obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);
            }

            return obj;
        }

        /// <summary>
        /// Force fix position based on current visual position
        /// </summary>
        [ContextMenu("Force Fix Position")]
        public void ForceFixPosition()
        {
            // Get actual visual position
            Vector3 currentWorldPos = transform.position;
            Vector2Int actualGridPos = GameMap.WorldToGridPosition(currentWorldPos);

            Debug.Log($"=== FORCE FIX POSITION ===");
            Debug.Log($"Current transform position: {currentWorldPos}");
            Debug.Log($"Current grid position: {actualGridPos}");
            Debug.Log($"Old root coordinate: {movableBoxData.rootCoordinate}");

            // Set root coordinate to current visual position
            movableBoxData.rootCoordinate = actualGridPos;

            // Fix all tile coordinates relative to new root
            for (int i = 0; i < listMovableBoxTile.Count; i++)
            {
                var tile = listMovableBoxTile[i];
                Vector3 tileLocalPos = tile.transform.localPosition;
                Vector2Int relativeTileCoord = new Vector2Int(
                    Mathf.RoundToInt(tileLocalPos.x),
                    Mathf.RoundToInt(tileLocalPos.y)
                );
                tile.Coordinate = actualGridPos + relativeTileCoord;

                Debug.Log($"Fixed tile[{i}] coordinate to: {tile.Coordinate} (local: {relativeTileCoord})");
            }

            Debug.Log($"New root coordinate: {movableBoxData.rootCoordinate}");

            // Update occupation
            ClearCurrentOccupation();
            foreach (var boxTile in listMovableBoxTile)
            {
                if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(boxTile.Coordinate, out var tile))
                {
                    tile.IsOccupied = true;
                }
            }
        }

        [ContextMenu("Debug Box Position")]
        public void DebugBoxPosition()
        {
            if (!enablePositionDebug) return;

            Debug.Log("=== BOX POSITION DEBUG ===");
            Debug.Log($"BoxMove Name: {name}");
            Debug.Log($"Root Coordinate: {movableBoxData.rootCoordinate}");
            Debug.Log($"Box Size: {movableBoxData.boxSize}");
            Debug.Log($"BoxMove World Position: {transform.position}");
            Debug.Log($"Movement Queue Count: {movementQueue.Count}");
            Debug.Log($"Is Moving: {isMoving}");
            Debug.Log($"Is Dragging: {isDragging}");
        }

        #endregion

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BoxMove] {message}");
            }
        }
    }
}