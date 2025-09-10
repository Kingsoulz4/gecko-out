using Geckout.Data;
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

        // Data
        private MovableBoxData movableBoxData = new();
        private readonly List<MovableBoxTile> listMovableBoxTile = new();

        // Input state
        private Camera gameCamera;
        private bool isDragging = false;
        private Vector2 dragStartPosition;
        private Vector2Int dragStartCoordinate;

        // Drag locking & targeting
        private Vector2Int selectedTileOffset = Vector2Int.zero;  // clicked child tile - root
        private Vector2Int baseSelectedTileCoord;                  // rootStart + offset
        private bool axisLocked = false;                           // for WayDirection.All
        private bool lockHorizontal = false;
        private bool hasDragTarget = false;
        private Vector2Int dragTargetRoot;                         // root coordinate target at this frame

        public bool IsDragging => isDragging;

        private void Start()
        {
            gameCamera = Camera.main ?? FindObjectOfType<Camera>();
        }

        private void Update()
        {
            HandleInput();
            DragMoveUpdate();
        }

        #region Input & Drag

        private void HandleInput()
        {
            // Mouse
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 pos = Input.mousePosition;
                if (IsTouchOverBox(pos)) StartDrag(pos);
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                DragTick(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                EndDrag();
            }

            //// Touch
            //if (Input.touchCount > 0)
            //{
            //    Touch touch = Input.GetTouch(0);
            //    Vector2 pos = touch.position;

            //    if (touch.phase == TouchPhase.Began)
            //    {
            //        if (IsTouchOverBox(pos)) StartDrag(pos);
            //    }
            //    else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isDragging)
            //    {
            //        DragTick(pos);
            //    }
            //    else if (touch.phase == TouchPhase.Ended && isDragging)
            //    {
            //        EndDrag();
            //    }
            //}
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

        private MovableBoxTile GetTileUnderScreenPosition(Vector2 screenPosition)
        {
            if (gameCamera == null) return null;

            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moveBoxLayer))
            {
                var tile = hit.transform.GetComponent<MovableBoxTile>();
                if (tile != null) return tile;
                return hit.transform.GetComponentInParent<MovableBoxTile>();
            }
            return null;
        }

        private void StartDrag(Vector2 inputPosition)
        {
            var clickedTile = GetTileUnderScreenPosition(inputPosition);
            if (clickedTile == null) return;

            isDragging = true;
            axisLocked = false;
            hasDragTarget = false;

            dragStartPosition = inputPosition;
            dragStartCoordinate = movableBoxData.rootCoordinate;

            selectedTileOffset = clickedTile.Coordinate - movableBoxData.rootCoordinate;
            baseSelectedTileCoord = dragStartCoordinate + selectedTileOffset;

            DebugLog($"StartDrag: root={dragStartCoordinate}, clicked={clickedTile.Coordinate}, offset={selectedTileOffset}");
        }

        private void DragTick(Vector2 currentInputPos)
        {
            // Nếu đang auto-move sau EndDrag, không cập nhật target mới
            if (!isDragging && hasDragTarget)
            {
                DebugLog("Skipping DragTick - auto-moving to previous target");
                return;
            }

            Vector2Int hoveredForSelected = ScreenToGridCoordinate(currentInputPos);
            Vector2Int constrainedForSelected = hoveredForSelected;

            // Apply direction constraints
            switch (wayDirection)
            {
                case WayDirection.Horizontal:
                    constrainedForSelected.y = baseSelectedTileCoord.y;
                    break;
                case WayDirection.Vertical:
                    constrainedForSelected.x = baseSelectedTileCoord.x;
                    break;
                case WayDirection.All:
                    if (!axisLocked)
                    {
                        Vector2 delta = currentInputPos - dragStartPosition;
                        if (delta.sqrMagnitude > inputThreshold * inputThreshold)
                        {
                            lockHorizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
                            axisLocked = true;
                        }
                    }
                    if (axisLocked)
                    {
                        if (lockHorizontal) constrainedForSelected.y = baseSelectedTileCoord.y;
                        else constrainedForSelected.x = baseSelectedTileCoord.x;
                    }
                    break;
            }

            Vector2Int rootTarget = constrainedForSelected - selectedTileOffset;

            // Validation và set target
            Debug.Log($"1 {rootTarget != movableBoxData.rootCoordinate}"  );
            Debug.Log($"2 {CanMoveRootTo(rootTarget)}");
            if (rootTarget != movableBoxData.rootCoordinate && CanMoveRootTo(rootTarget))
            {
                if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(rootTarget, out var targetTile))
                {
                    dragTargetRoot = rootTarget;
                    hasDragTarget = true;
                    DebugLog($"New drag target set: {rootTarget}");
                }
                else
                {
                    hasDragTarget = false;
                    DebugLog($"Invalid target - no tile: {rootTarget}");
                }
            }
            else
            {
                hasDragTarget = false;
                DebugLog("rootTarget != movableBoxData.rootCoordina");
                if (rootTarget == movableBoxData.rootCoordinate)
                {
                    DebugLog("Target same as current position");
                }
                   
            }
        }

        private void DragMoveUpdate()
        {
            if (!hasDragTarget) return;

            Vector3 targetWorld = GetWorldPositionFromGridKeepZ(dragTargetRoot);
            float step = moveSpeed * Time.deltaTime;

            // MoveTowards đảm bảo không bao giờ vượt qua target
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetWorld, step);
            transform.position = newPosition;

            // Kiểm tra đã đến target chưa
            if (Vector3.Distance(transform.position, targetWorld) < 0.01f)
            {
                // Đảm bảo position chính xác 100%
                transform.position = targetWorld;

                ClearCurrentOccupation();
                UpdateBoxLogicalPosition(dragTargetRoot);
                hasDragTarget = false;
                //axisLocked = false;
                DebugLog($"Reached target after EndDrag: {targetWorld}");
            }
            else
            {
                float remainingDist = Vector3.Distance(transform.position, targetWorld);
                DebugLog($"Still moving to target, remaining: {remainingDist}");
            }
        }

        private void EndDrag()
        {
            DebugLog($"EndDrag called - hasDragTarget: {hasDragTarget}");

            if (hasDragTarget)
            {
                Vector3 currentPos = transform.position;
                Vector3 targetPos = GetWorldPositionFromGridKeepZ(dragTargetRoot);
                float remainingDist = Vector3.Distance(currentPos, targetPos);

                DebugLog($"EndDrag with active target - remaining distance: {remainingDist}");
            }

            isDragging = false;
            axisLocked = false;
            // Không clear hasDragTarget - để DragMoveUpdate tiếp tục move
        }


        #endregion

        #region Grid Helpers & Validation

        private Vector2Int ScreenToGridCoordinate(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            float currentZ = transform.position.z;
            Plane groundPlane = new Plane(Vector3.forward, new Vector3(0, 0, currentZ));

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector2Int gridCoord = GameMap.WorldToGridPosition(worldPoint);

                DebugLog($"Screen {screenPosition} -> World {worldPoint} -> Grid {gridCoord}");
                return gridCoord;
            }

            return movableBoxData.rootCoordinate;
        }

        private bool CanMoveRootTo(Vector2Int targetRootPos)
        {
            // Bounds check toàn khối
            for (int x = 0; x < movableBoxData.boxSize.x; x++)
            {
                for (int y = 0; y < movableBoxData.boxSize.y; y++)
                {
                    Vector2Int check = targetRootPos + new Vector2Int(x, y);
                    if (!IsWithinMapBounds(check)) 
                        return false;
                    if (IsTileOccupiedByOther(check)) 
                        return false;
                }
            }
            return true;
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
                // Bỏ qua các tile thuộc chính box này
                foreach (var boxTile in listMovableBoxTile)
                {
                    if (boxTile.Coordinate == gridPos)
                        return false; // occupied nhưng là của mình
                }
                return tile.IsOccupied;
            }
            return true; // không tìm thấy tile → xem như không hợp lệ
        }

        private Vector3 GetWorldPositionFromGridKeepZ(Vector2Int gridPos)
        {
            if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(gridPos, out var tile))
            {
                var p = tile.transform.position;              
                return new Vector3(p.x, p.y, transform.position.z);
            }
            var f = GameMap.GetTileWorldPosition(gridPos);    
            return new Vector3(f.x, f.y, transform.position.z);
        }

        #endregion

        #region Occupation & Logical Update

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

        private void UpdateBoxLogicalPosition(Vector2Int newRootPos)
        {
            Vector2Int offset = newRootPos - movableBoxData.rootCoordinate;
            movableBoxData.rootCoordinate = newRootPos;

            foreach (var boxTile in listMovableBoxTile)
            {
                boxTile.Coordinate += offset;
                if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(boxTile.Coordinate, out var tile))
                {
                    tile.IsOccupied = true;
                }
            }

            DebugLog($"Box moved to {newRootPos}, world={transform.position}");
        }

        #endregion

        #region Public API

        public void Init(MovableBoxData data)
        {
            movableBoxData = data;
            SpawnTiles();

            // Đặt transform về đúng tâm root tile (giữ Z hiện tại)
            if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate, out var rootTile))
            {
                var p = rootTile.transform.position;
                transform.position = new Vector3(p.x, p.y, transform.position.z);
            }
        }

        #endregion

        #region Tile Spawning

        private void SpawnTiles()
        {
            bool needSetOccupied = false;
            if (LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Contains(movableBoxData))
                needSetOccupied = true;

            var size = movableBoxData.boxSize;

            if (size.x == 1 && size.y == 1)
                SpawnSingleTile(needSetOccupied);
            else if (size.x == 1)
                SpawnVerticalTiles(needSetOccupied);
            else if (size.y == 1)
                SpawnHorizontalTiles(needSetOccupied);
            else
                SpawnRectangleTiles(needSetOccupied);
        }

        private void SpawnSingleTile(bool needSetOccupied)
        {
            LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate, out var tile);
            GameObject obj = Instantiate(m_tile4EdgePrefab, transform);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.name = "Tile";
            obj.transform.localRotation = Quaternion.Euler(-90 * Vector3.right);

            var mbt = obj.AddComponent<MovableBoxTile>();
            mbt.Coordinate = tile.MapTileData.coordinate;
            listMovableBoxTile.Add(mbt);
            if (needSetOccupied) tile.IsOccupied = true;
        }

        private void SpawnVerticalTiles(bool needSetOccupied)
        {
            var size = movableBoxData.boxSize;
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int c = new Vector2Int(0, y);
                var prefab = (y == 0 || y == size.y - 1) ? m_tileCorner3EdgePrefab : m_tile2EdgePrefab;

                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate + c, out var tile);
                GameObject obj = Instantiate(prefab, transform);

                obj.transform.localPosition = new Vector3(c.x, c.y, 0);
                obj.transform.localScale = Vector3.one;
                obj.name = $"Tile_{c.x}_{c.y}";

                if (y == 0)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));
                else if (y == size.y - 1)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, -90, 90));
                else
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(0, 90, -90));

                var mbt = obj.AddComponent<MovableBoxTile>();
                mbt.Coordinate = tile.MapTileData.coordinate;
                listMovableBoxTile.Add(mbt);
                if (needSetOccupied) tile.IsOccupied = true;
            }
        }

        private void SpawnHorizontalTiles(bool needSetOccupied)
        {
            var size = movableBoxData.boxSize;
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int c = new Vector2Int(x, 0);
                var prefab = (x == 0 || x == size.x - 1) ? m_tileCorner3EdgePrefab : m_tile2EdgePrefab;

                LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate + c, out var tile);
                GameObject obj = Instantiate(prefab, transform);

                obj.transform.localPosition = new Vector3(c.x, c.y, 0);
                obj.transform.localScale = Vector3.one;
                obj.name = $"Tile_{c.x}_{c.y}";

                if (x == 0)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, 90, -90));
                else if (x == size.x - 1)
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(-90, -90, 90));
                else
                    obj.transform.localRotation = Quaternion.Euler(new Vector3Int(90, 90, -90));

                var mbt = obj.AddComponent<MovableBoxTile>();
                mbt.Coordinate = tile.MapTileData.coordinate;
                listMovableBoxTile.Add(mbt);
                if (needSetOccupied) tile.IsOccupied = true;
            }
        }

        private void SpawnRectangleTiles(bool needSetOccupied)
        {
            var size = movableBoxData.boxSize;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int c = new Vector2Int(x, y);
                    LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(movableBoxData.rootCoordinate + c, out var tile);

                    GameObject obj = CreateTileForPosition(x, y, size);

                    obj.transform.localPosition = new Vector3(c.x, c.y, 0);
                    obj.transform.localScale = Vector3.one;
                    obj.name = $"Tile_{c.x}_{c.y}";

                    var mbt = obj.AddComponent<MovableBoxTile>();
                    mbt.Coordinate = tile.MapTileData.coordinate;
                    listMovableBoxTile.Add(mbt);
                    if (needSetOccupied) tile.IsOccupied = true;
                }
            }
        }

        private GameObject CreateTileForPosition(int x, int y, Vector2Int boxSize)
        {
            GameObject obj;

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

        #endregion

        #region Utils

        private void DebugLog(string message)
        {
            if (enableDebugLogs) Debug.Log($"[BoxMove] {message}");
        }

        #endregion
    }
}
