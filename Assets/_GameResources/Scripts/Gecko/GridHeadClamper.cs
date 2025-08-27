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
                Debug.LogError("GridHeadClamper requires BodyController component!");
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

            // If head is not at tile center and we have an active path, queue the new path
            if (!IsAtTileCenter && currentPath.Count > 0)
            {
                queuedPath = new List<Vector2Int>(gridPath);
                DebugLog($"Path queued - head not at tile center. Queued path has {queuedPath.Count} waypoints");
                return;
            }

            // Apply path immediately
            ApplyNewPath(gridPath);
        }

        private void ApplyNewPath(List<Vector2Int> gridPath)
        {
            currentPath = new List<Vector2Int>(gridPath);
            currentWaypointIndex = 0;

            // Calculate initial direction to first waypoint
            if (currentPath.Count > 0)
            {
                Vector2Int headCoord = bodyController.Segments[0].Coordinate;
                currentDirection = CalculateDirectionToWaypoint(headCoord, currentPath[0]);
                DebugLog($"New path applied with {currentPath.Count} waypoints. Initial direction: {currentDirection}");
            }

            // Clear queued path since we just applied a new one
            queuedPath = null;
        }


        public Vector3 ClampHeadPosition(Vector3 intendedPosition)
        {
            if (bodyController?.Segments == null || bodyController.Segments.Count == 0)
                return intendedPosition;

            Vector3 currentHeadPos = bodyController.Segments[0].transform.position;
            Vector2Int currentTileCoord = bodyController.OccupiedTileController.WorldToGridPosition(currentHeadPos);

            // Check if head is at tile center
            CheckTileCenterAlignment(currentHeadPos, currentTileCoord);

            // Update direction if at tile center
            if (IsAtTileCenter)
            {
                UpdateDirectionAtTileCenter(currentTileCoord);
            }

            // If no path or direction, snap to current tile center
            if (currentPath.Count == 0 || currentDirection == Vector2Int.zero)
            {
                if (GameMap.TryGetTileAt(currentTileCoord, out GameTile currentTile))
                {
                    return currentTile.transform.position; // Snap to tile center
                }
                return currentHeadPos;
            }

            // Apply grid constraint to intended movement
            return ApplyGridConstraintToIntendedPosition(currentHeadPos, intendedPosition, currentTileCoord);
        }

        private Vector3 ApplyGridConstraintToIntendedPosition(Vector3 currentPos, Vector3 intendedPos, Vector2Int currentTileCoord)
        {
            // Get current tile center
            if (!GameMap.TryGetTileAt(currentTileCoord, out GameTile currentTile))
                return intendedPos;

            Vector3 currentTileCenter = currentTile.transform.position;

            // Calculate next tile position based on current direction
            Vector2Int nextTileCoord = currentTileCoord + currentDirection;
            if (!GameMap.TryGetTileAt(nextTileCoord, out GameTile nextTile))
            {
                return currentTileCenter; // Can't move, stay at center
            }

            Vector3 nextTileCenter = nextTile.transform.position;

            // Calculate movement towards next tile center
            Vector3 movementDelta = intendedPos - currentPos;
            float intendedSpeed = movementDelta.magnitude;

            // Use MoveTowards for precise movement along grid line
            Vector3 precisePosition = Vector3.MoveTowards(currentPos, nextTileCenter, intendedSpeed);

            DebugLog($"Grid constraint: {currentPos} -> intended {intendedPos} -> clamped {precisePosition}");

            return precisePosition;
        }
        private void CheckTileCenterAlignment(Vector3 headPos, Vector2Int tileCoord)
        {
            if (!GameMap.TryGetTileAt(tileCoord, out GameTile currentTile))
            {
                IsAtTileCenter = false;
                return;
            }

            Vector3 tileCenter = currentTile.transform.position;
            float distanceToCenter = Vector3.Distance(headPos, tileCenter);

            bool wasAtCenter = IsAtTileCenter;
            IsAtTileCenter = distanceToCenter <= tileCenterThreshold;

            // Log transition to tile center
            if (!wasAtCenter && IsAtTileCenter)
            {
                DebugLog($"Head reached tile center: {tileCoord}");
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

            if (currentPath.Count == 0) return;

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
                currentDirection = Vector2Int.zero;
                DebugLog("Reached end of path");
            }
        }

        private Vector2Int CalculateDirectionToWaypoint(Vector2Int from, Vector2Int to)
        {
            Vector2Int diff = to - from;

            // Return primary direction (prioritize horizontal movement)
            if (Mathf.Abs(diff.x) > 0)
            {
                return new Vector2Int(diff.x > 0 ? 1 : -1, 0);
            }
            else if (Mathf.Abs(diff.y) > 0)
            {
                return new Vector2Int(0, diff.y > 0 ? 1 : -1);
            }

            return Vector2Int.zero;
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GridHeadClamper] {message}");
            }
        }

        // Public methods for debugging
        public Vector2Int GetCurrentDirection() => currentDirection;
        public int GetCurrentWaypointIndex() => currentWaypointIndex;
        public int GetPathLength() => currentPath.Count;
        public bool HasQueuedPath() => queuedPath != null;
    }
}