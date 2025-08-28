using UnityEngine;
using System.Collections.Generic;

namespace Geckout
{
    public class GridHeadClamper : MonoBehaviour
    {
        [Header("Grid Clamping Settings")]
        [SerializeField] private float tileCenterThreshold = 0.1f;
        [SerializeField] private bool enableDebugLogs = false;

        private BodyController bodyController;
        private List<Vector2Int> currentPath = new List<Vector2Int>();
        private int currentWaypointIndex = 0;
        private Vector2Int currentDirection = Vector2Int.zero;

        public bool IsAtTileCenter { get; private set; }

        void Start()
        {
            bodyController = GetComponent<BodyController>();
            if (bodyController == null)
            {
                enabled = false;
            }
        }

        private List<Vector2Int> queuedPath = null;

        public void SetPath(List<Vector2Int> gridPath)
        {
            if (gridPath == null || gridPath.Count == 0)
            {
                currentPath.Clear();
                currentWaypointIndex = 0;
                currentDirection = Vector2Int.zero;
                queuedPath = null;
                DebugLog("Path cleared");
                return;
            }

            // Filter out the starting position if it matches current position
            Vector2Int currentPos = GetControllingSegmentCoordinate();
            List<Vector2Int> filteredPath = new List<Vector2Int>();

            foreach (var waypoint in gridPath)
            {
                if (waypoint != currentPos)
                {
                    filteredPath.Add(waypoint);
                }
            }

            if (filteredPath.Count == 0)
            {
                DebugLog("Path filtered to empty - no movement needed");
                currentPath.Clear();
                currentWaypointIndex = 0;
                currentDirection = Vector2Int.zero;
                return;
            }

            // If controlling segment is not at tile center and we have an active path, queue the new path
            if (!IsAtTileCenter && currentPath.Count > 0)
            {
                queuedPath = new List<Vector2Int>(filteredPath);
                DebugLog($"Path queued - controlling segment not at tile center. Queued path has {filteredPath.Count} waypoints");
                return;
            }

            // Apply path immediately
            ApplyNewPath(filteredPath);
        }

        private void ApplyNewPath(List<Vector2Int> gridPath)
        {
            currentPath = new List<Vector2Int>(gridPath);
            currentWaypointIndex = 0;

            // Calculate initial direction to first waypoint
            if (currentPath.Count > 0)
            {
                Vector2Int controllingCoord = GetControllingSegmentCoordinate();
                currentDirection = CalculateDirectionToWaypoint(controllingCoord, currentPath[0]);
                DebugLog($"New path applied with {currentPath.Count} waypoints. Initial direction: {currentDirection}. Control anchor: {bodyController.controlAnchor}");
            }

            // Clear queued path since we just applied a new one
            queuedPath = null;
        }

        // FIXED: Get the coordinate of the segment that's actually being controlled
        private Vector2Int GetControllingSegmentCoordinate()
        {
            if (bodyController?.Segments == null || bodyController.Segments.Count == 0)
                return Vector2Int.zero;

            // Get the controlling segment based on current control anchor
            Segment controllingSegment;
            var orderedSegments = bodyController.GetOrderedSegments();
            controllingSegment = orderedSegments[0]; // Always the first in ordered list

            // Use the actual world position and convert to grid coordinate
            Vector3 worldPos = controllingSegment.transform.position;
            Vector2Int gridPos = bodyController.OccupiedTileController.WorldToGridPosition(worldPos);

            DebugLog($"Controlling segment coordinate: {gridPos} (world: {worldPos})");
            return gridPos;
        }

        private Vector3 GetControllingSegmentPosition()
        {
            if (bodyController?.Segments == null || bodyController.Segments.Count == 0)
                return Vector3.zero;

            // Get the controlling segment based on current control anchor
            var orderedSegments = bodyController.GetOrderedSegments();
            return orderedSegments[0].transform.position; // Always the first in ordered list
        }

        public Vector3 ClampHeadPosition(Vector3 targetPosition)
        {
            if (bodyController?.Segments == null || bodyController.Segments.Count == 0 ||
                !TouchInputHandler.Instance.IsDragging)
                return targetPosition;

            // FIXED: Use controlling segment instead of always using head
            Vector3 currentControllingPos = GetControllingSegmentPosition();
            Vector2Int currentTileCoord = bodyController.OccupiedTileController.WorldToGridPosition(currentControllingPos);

            DebugLog($"Clamping position - Current: {currentControllingPos}, Target: {targetPosition}, Grid: {currentTileCoord}");

            // Check if controlling segment is at tile center
            CheckTileCenterAlignment(currentControllingPos, currentTileCoord);

            // Update direction if at tile center
            if (IsAtTileCenter)
            {
                UpdateDirectionAtTileCenter(currentTileCoord);
            }

            // If no path or direction, stay at current position
            if (currentPath.Count == 0 || currentDirection == Vector2Int.zero)
            {
                DebugLog("No path or direction - staying at current position");
                return currentControllingPos;
            }

            // Apply grid clamp movement
            Vector3 clampedPosition = ApplyGridClamp(currentControllingPos, targetPosition, currentTileCoord);
            DebugLog($"Clamped position result: {clampedPosition}");

            return clampedPosition;
        }

        private void CheckTileCenterAlignment(Vector3 controllingPos, Vector2Int tileCoord)
        {
            if (!GameMap.TryGetTileAt(tileCoord, out GameTile currentTile))
            {
                IsAtTileCenter = false;
                DebugLog($"No tile found at coordinate: {tileCoord}");
                return;
            }

            Vector3 tileCenter = currentTile.transform.position;
            float distanceToCenter = Vector3.Distance(controllingPos, tileCenter);

            bool wasAtCenter = IsAtTileCenter;
            IsAtTileCenter = distanceToCenter <= tileCenterThreshold;

            DebugLog($"Distance to center: {distanceToCenter}, threshold: {tileCenterThreshold}, at center: {IsAtTileCenter}" +
                $", at coor: {tileCoord}");

            // Log transition to tile center
            if (!wasAtCenter && IsAtTileCenter)
            {
                DebugLog($"Controlling segment reached tile center: {tileCoord}");
            }
        }

        private void UpdateDirectionAtTileCenter(Vector2Int currentTileCoord)
        {
            // Apply queued path if available
            if (queuedPath != null)
            {
                DebugLog("Applying queued path at tile center");
                ApplyNewPath(queuedPath);
                return; // Direction will be recalculated in ApplyNewPath
            }

            if (currentPath.Count == 0)
            {
                DebugLog("No current path to process");
                return;
            }

            // Check if we reached current waypoint
            if (currentWaypointIndex < currentPath.Count && currentTileCoord == currentPath[currentWaypointIndex])
            {
                DebugLog($"Reached waypoint {currentWaypointIndex}: {currentTileCoord}");
                currentWaypointIndex++;
            }

            // Calculate direction to next waypoint
            if (currentWaypointIndex < currentPath.Count)
            {
                Vector2Int nextWaypoint = currentPath[currentWaypointIndex];
                Vector2Int newDirection = CalculateDirectionToWaypoint(currentTileCoord, nextWaypoint);

                if (newDirection != currentDirection)
                {
                    currentDirection = newDirection;
                    DebugLog($"Direction changed to {currentDirection} heading to waypoint {currentWaypointIndex}: {nextWaypoint}");
                }
            }
            else
            {
                // Reached end of path
                if (currentDirection != Vector2Int.zero)
                {
                    currentDirection = Vector2Int.zero;
                    DebugLog("Reached end of path");
                }
            }
        }

        private Vector2Int CalculateDirectionToWaypoint(Vector2Int from, Vector2Int to)
        {
            Vector2Int diff = to - from;
            DebugLog($"Calculating direction from {from} to {to}, diff: {diff}");

            // Return primary direction (prioritize horizontal movement)
            if (Mathf.Abs(diff.x) > 0)
            {
                Vector2Int result = new Vector2Int(diff.x > 0 ? 1 : -1, 0);
                DebugLog($"Direction result (horizontal): {result}");
                return result;
            }
            else if (Mathf.Abs(diff.y) > 0)
            {
                Vector2Int result = new Vector2Int(0, diff.y > 0 ? 1 : -1);
                DebugLog($"Direction result (vertical): {result}");
                return result;
            }

            DebugLog("Direction result: zero");
            return Vector2Int.zero;
        }

        private Vector3 ApplyGridClamp(Vector3 currentPos, Vector3 targetPos, Vector2Int currentTileCoord)
        {
            // Get current tile center
            if (!GameMap.TryGetTileAt(currentTileCoord, out GameTile currentTile))
            {
                DebugLog($"Cannot find current tile at {currentTileCoord}");
                return targetPos;
            }

            Vector3 currentTileCenter = currentTile.transform.position;

            // Nếu không có direction thì đứng tại tâm tile hiện tại
            if (currentDirection == Vector2Int.zero)
            {
                DebugLog("No direction - staying at tile center");
                return currentTileCenter;
            }

            // Xác định tile kế tiếp
            Vector2Int nextTileCoord = currentTileCoord + currentDirection;
            if (!GameMap.TryGetTileAt(nextTileCoord, out GameTile nextTile))
            {
                return targetPos;
            }

            Vector3 nextTileCenter = nextTile.transform.position;

            // Tính movement cho frame này
            Vector3 moveDelta = targetPos - currentPos;
            float frameSpeed = moveDelta.magnitude;

            // Di chuyển mượt về tile kế tiếp
            Vector3 precisePosition = Vector3.MoveTowards(currentPos, nextTileCenter, frameSpeed);

            DebugLog($"Moving from {currentPos} towards {nextTileCenter} (direction: {currentDirection}), result: {precisePosition}");

            return precisePosition;
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GridHeadClamper] {message}");
            }
        }
    }
}