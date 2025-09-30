using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class OccupiedTileController : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private BodyController bodyController;

        private Vector2Int[] lastGridPositions;
        private GameTile[] currentOccupiedTiles;
        private Vector2 gridOffset;

        public Vector2Int[] LastGridPositions { get => lastGridPositions; }
        public GameTile[] CurrentOccupiedTiles { get => currentOccupiedTiles;}

        void Start()
        {
            //Init();
        }

        private void OnStartMoveHandle()
        {
        }

        private void OnEndMoveHandle()
        {
            ForceRestoreAll();
        }

        public void Init()
        {
            this.WaitUntil(() => bodyController != null, () =>
            {
                bodyController.OnStartMove += OnStartMoveHandle;
                bodyController.OnEndMove += OnEndMoveHandle;

                int segmentCount = bodyController.Segments.Count;
                lastGridPositions = new Vector2Int[segmentCount];
                currentOccupiedTiles = new GameTile[segmentCount];

                Vector2Int mapSize = GameMap.MapSize;
                gridOffset = new Vector2(mapSize.x - 1, mapSize.y - 1) * 0.5f;

                Debug.Log($"Map size: {mapSize}, calculated offset: {gridOffset}");

                // Initialize với position hiện tại
                for (int i = 0; i < segmentCount; i++)
                {
                    lastGridPositions[i] = Vector2Int.one * int.MinValue;
                }

                UpdateAllSegmentPositions(false);
            });
        }

        public void UpdateAllSegmentPositions(bool isChangeTileColor = true)
        {
            if (!bodyController.CanControl)
            {
                return;
            }
            var orderedSegments = bodyController.GetOrderedSegments();
            var segments = bodyController.Segments;

            for (int i = 0; i < orderedSegments.Count; i++)
            {
                if (i % bodyController.SubLength != 0 && i != 0 && i != segments.Count - 1)
                {
                    continue;
                }

                var segment = orderedSegments[i];
                int rawIndex = segments.IndexOf(segment);

                Vector3 worldPos = segment.transform.position;
                Vector2Int gridPos = GameMap.WorldToGridPosition(worldPos);

                UpdateSegmentTile(rawIndex, gridPos, isChangeTileColor);
                lastGridPositions[rawIndex] = gridPos;
            }
        }

        public void ClearAllOccupied()
        {
            var segments = bodyController.Segments;
            if (segments == null || segments.Count <= 0) return;

            for (int i = 0; i < segments.Count; i++)
            {
                if (currentOccupiedTiles[i] != null && currentOccupiedTiles[i].isActiveAndEnabled)
                {
                    // Remove occupant từ tile - tile sẽ tự restore color
                    currentOccupiedTiles[i].RemoveOccupant();
                    currentOccupiedTiles[i].SetOccupied(false);
                    currentOccupiedTiles[i] = null;
                }
            }
        }

        void UpdateSegmentTile(int segmentIndex, Vector2Int gridPos, bool changeTileColor)
        {
            var segment = bodyController.Segments[segmentIndex];
            GameTile previousTile = currentOccupiedTiles[segmentIndex];

            // Check if position actually changed to avoid unnecessary updates
            if (lastGridPositions[segmentIndex] == gridPos)
            {
                return; // No change, skip update
            }


            // Remove from previous tile
            if (previousTile != null)
            {
                previousTile.RemoveOccupant();
                currentOccupiedTiles[segmentIndex].SetOccupied(false);

            }

            // Add to new tile
            if (GameMap.TryGetTileAtCoord(gridPos, out GameTile newTile))
            {
                currentOccupiedTiles[segmentIndex] = newTile;
                newTile.SetOccupied(true);
                if (!bodyController.MoveToPortal.IsEnteringPortal)
                {
                    newTile.AddOccupant(changeTileColor);
                }
                segment.UpdateCoordinateOnly(gridPos);
            }
            else
            {
                currentOccupiedTiles[segmentIndex] = null;
                Debug.LogWarning($"FAILED: No tile found at grid position {gridPos} for segment {segmentIndex}");
            }
        }

        void OnDestroy()
        {
            ClearAllOccupied();
        }

        public void ForceRestoreAll()
        {
            if (currentOccupiedTiles == null || currentOccupiedTiles.Length < 0)
            {
                return;
            }
            for (int i = 0; i < currentOccupiedTiles.Length; i++)
            {
                if (currentOccupiedTiles[i] != null)
                {
                    currentOccupiedTiles[i].ForceRestoreColor();
                }
            }
        }
    }
}