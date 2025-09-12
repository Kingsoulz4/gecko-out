using Geckout.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BoxMove : BoxBase<MovableBoxTile, MovableBoxData>
    {
        [Header("Movement Settings")]
        [SerializeField] private LayerMask moveBoxLayer;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float inputThreshold = 0.5f;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Debug")]
        [SerializeField] private bool enablePositionDebug = true;

        [SerializeField] private WayDirection wayDirection = WayDirection.All;

        [Header("Visual")]
        [SerializeField] private GameObject m_arrowHorizontal;
        [SerializeField] private GameObject m_arrowVertical;
        [SerializeField] private GameObject m_arrowBodyHorizontal;
        [SerializeField] private GameObject m_arrowBodyVertical;
        [SerializeField] private GameObject m_arrowLeft;
        [SerializeField] private GameObject m_arrowRight;
        [SerializeField] private GameObject m_arrowTop;
        [SerializeField] private GameObject m_arrowDown;

        // Data
        protected override Vector2Int RootCoordinate => Data.rootCoordinate;
        protected override Vector2Int BoxSize => Data.boxSize;

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
            dragStartCoordinate = Data.rootCoordinate;

            selectedTileOffset = clickedTile.Coordinate - Data.rootCoordinate;
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
            switch (Data.wayDirection)
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
            Debug.Log($"1 {rootTarget != Data.rootCoordinate}"  );
            Debug.Log($"2 {CanMoveRootTo(rootTarget)}");
            if (rootTarget != Data.rootCoordinate && CanMoveRootTo(rootTarget))
            {
                if (GameMap.TryGetTileAtCoord(rootTarget, out var targetTile))
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
                DebugLog("rootTarget != movableBoxData.rootCoordina");
                if (rootTarget == Data.rootCoordinate)
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

            return Data.rootCoordinate;
        }

        private bool CanMoveRootTo(Vector2Int targetRootPos)
        {
            // Bounds check toàn khối
            for (int x = 0; x < Data.boxSize.x; x++)
            {
                for (int y = 0; y < Data.boxSize.y; y++)
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
            if (GameMap.TryGetTileAtCoord(gridPos, out var tile))
            {
                // Bỏ qua các tile thuộc chính box này
                foreach (var boxTile in spawnedTiles)
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
            if (GameMap.TryGetTileAtCoord(gridPos, out var tile))
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
            foreach (var boxTile in spawnedTiles)
            {
                if (GameMap.TryGetTileAtCoord(boxTile.Coordinate, out var tile))
                {
                    tile.IsOccupied = false;
                }
            }
        }

        private void UpdateBoxLogicalPosition(Vector2Int newRootPos)
        {
            Vector2Int offset = newRootPos - Data.rootCoordinate;
            Data.rootCoordinate = newRootPos;

            foreach (var boxTile in spawnedTiles)
            {
                boxTile.Coordinate += offset;
                if (GameMap.TryGetTileAtCoord(boxTile.Coordinate, out var tile))
                {
                    tile.IsOccupied = true;
                }
            }

            DebugLog($"Box moved to {newRootPos}, world={transform.position}");
        }

        #endregion

        #region Public API

        public override void Init(MovableBoxData data)
        {
            //Data = data;
            //SpawnTiles();

            base.Init(data);

            // Đặt transform về đúng tâm root tile (giữ Z hiện tại)
            //if (LevelManager.Instance.LevelGame.GameMap.TryGetTileAtCoord(Data.rootCoordinate, out var rootTile))
            //{
            //    var p = rootTile.transform.position;
            //    transform.position = new Vector3(p.x, p.y, transform.position.z);
            //}
        }

        #endregion

        #region Visual 
        public override void UpdateVisual()
        {
            base.UpdateVisual();

            var offset = 0.2f;
            var centerWorlPos = GetCenterWorldPos();
            var root = RootCoordinate;
            GameMap.TryGetTileAtCoord(root, out var tileLeft);
            var topCoord = new Vector2Int(root.x + BoxSize.x - 1, root.y + BoxSize.y - 1);
            GameMap.TryGetTileAtCoord(topCoord, out var tileTop);

            m_arrowHorizontal.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowHorizontal.transform.position.z);
            m_arrowVertical.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowVertical.transform.position.z);
            //m_arrowBodyHorizontal.transform.position = centerWorlPos;
            //m_arrowBodyVertical.transform.position = centerWorlPos;
            m_arrowHorizontal.SetActive(Data.wayDirection == WayDirection.Horizontal || Data.wayDirection == WayDirection.All);
            m_arrowVertical.SetActive(Data.wayDirection == WayDirection.Vertical || Data.wayDirection == WayDirection.All);


            if (BoxSize.x == 1 && BoxSize.y == 1)
            {
                m_arrowBodyHorizontal.transform.localScale = Vector3.zero;
                m_arrowBodyVertical.transform.localScale = Vector3.zero;
                m_arrowLeft.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowLeft.transform.position.z) + Vector3.left * offset;
                m_arrowRight.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowRight.transform.position.z) + Vector3.right * offset;
                m_arrowTop.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowTop.transform.position.z) + Vector3.up * offset;
                m_arrowDown.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowDown.transform.position.z) + Vector3.down * offset;

            }
            else if(BoxSize.x == 1)
            {
                m_arrowBodyHorizontal.transform.localScale = Vector3.zero;
                m_arrowLeft.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowLeft.transform.position.z) + Vector3.left * offset;
                m_arrowRight.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowRight.transform.position.z) + Vector3.right * offset;
                m_arrowBodyVertical.transform.localScale = new Vector3(1, (BoxSize.y - 1), 1);
                m_arrowTop.transform.position = new Vector3(centerWorlPos.x,tileTop.transform.position.y, m_arrowTop.transform.position.z);
                m_arrowDown.transform.position = new Vector3(centerWorlPos.x, tileLeft.transform.position.y, m_arrowTop.transform.position.z);

            }
            else if(BoxSize.y == 1)
            {
                m_arrowBodyHorizontal.transform.localScale = new Vector3((BoxSize.x - 1), 1, 1);
                m_arrowLeft.transform.position = new Vector3(tileLeft.transform.position.x, centerWorlPos.y, m_arrowLeft.transform.position.z);
                m_arrowRight.transform.position = new Vector3(tileTop.transform.position.x, centerWorlPos.y, m_arrowRight.transform.position.z);
                m_arrowBodyVertical.transform.localScale = Vector3.zero;
                m_arrowTop.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowTop.transform.position.z) + Vector3.up * offset;
                m_arrowDown.transform.position = new Vector3(centerWorlPos.x, centerWorlPos.y, m_arrowDown.transform.position.z) + Vector3.down * offset;
            }
            else
            {
                m_arrowBodyHorizontal.transform.localScale = new Vector3((BoxSize.x - 1), 1, 1);
                m_arrowLeft.transform.position = new Vector3(tileLeft.transform.position.x, centerWorlPos.y, m_arrowLeft.transform.position.z);
                m_arrowRight.transform.position = new Vector3(tileTop.transform.position.x, centerWorlPos.y, m_arrowRight.transform.position.z);
                m_arrowBodyVertical.transform.localScale = new Vector3(1,(BoxSize.y - 1), 1);
                m_arrowTop.transform.position = new Vector3(centerWorlPos.x, tileTop.transform.position.y, m_arrowTop.transform.position.z);
                m_arrowDown.transform.position = new Vector3(centerWorlPos.x, tileLeft.transform.position.y, m_arrowTop.transform.position.z);

            }

        }
        #endregion

        #region Tile Spawning

        protected override bool ShouldSetOccupied()
        {
            return LevelManager.Instance.LevelGame.GameLevelData.listMovableBoxData.Contains(Data);
        }

        protected override void AddTileComponent(GameObject obj, Vector2Int coord)
        {
            var tile = obj.AddComponent<MovableBoxTile>();
            tile.Coordinate = coord;
            spawnedTiles.Add(tile);
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
