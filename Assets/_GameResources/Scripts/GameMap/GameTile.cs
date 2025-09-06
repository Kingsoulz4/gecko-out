using System.Collections;
using System.Collections.Generic;
using Geckout.Generals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Geckout
{
    public class GameTile : MonoBehaviour
    {
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] public bool IsOccupied;
        [SerializeField] private Collider tileCollider; // For raycast

       
        [SerializeField] private float restoreDelay =0.05f;

        public Vector2Int Coordinate { get; private set; }
        public Renderer TileRenderer { get => tileRenderer; }

        private Material defaultMaterial;
        private Material moveMaterial;

        // Self management
        private int occupancyCount = 0;
        private Coroutine restoreCoroutine;

        private void Start()
        {
            if (tileCollider == null)
            {
                tileCollider = GetComponent<Collider>();
                if (tileCollider == null)
                {
                    var boxCollider = gameObject.AddComponent<BoxCollider>();
                    boxCollider.size = Vector3.one;
                    tileCollider = boxCollider;
                }
            }
            tileRenderer.enabled = true;

            SetMaterialImmediate(defaultMaterial);
        }

        public void InitializeMaterials(Material oldMat, Material moveMat)
        {
            defaultMaterial = oldMat;
            moveMaterial = moveMat;
        }

        public void SetCoordinate(int x, int y)
        {
            Coordinate = new Vector2Int(x, y);
        }

        #region Occupancy Management

        public void AddOccupant(bool setColor)
        {
            occupancyCount++;

            if (!setColor)
            {
                return;
            }

            // Cancel any pending restore
            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
                restoreCoroutine = null;
            }

            // Apply occupied material immediately
            SetMaterialImmediate(moveMaterial);
        }

        public void RemoveOccupant()
        {
            occupancyCount = Mathf.Max(0, occupancyCount - 1);
            UpdateOccupiedState();

            // If no more occupants, start restore process
            if (occupancyCount == 0)
            {
                StartRestoreProcess();
            }
        }

        public void UpdateOccupiedState()
        {
            bool newOccupiedState = occupancyCount > 0;

            if (IsOccupied != newOccupiedState)
            {
                IsOccupied = newOccupiedState;
            }
        }

        private void StartRestoreProcess()
        {
            // Cancel existing restore if any
            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
            }

            restoreCoroutine = StartCoroutine(DelayedRestore());
        }

        private IEnumerator DelayedRestore()
        {
            yield return new WaitForSeconds(restoreDelay);

            // Double check we're still unoccupied
            if (occupancyCount == 0)
            {
                SetMaterialImmediate(defaultMaterial);
            }

            restoreCoroutine = null;
        }

        #endregion

        #region Material Management

        private void SetMaterialImmediate(Material material)
        {
            if (tileRenderer != null && material != null && tileRenderer.material != material)
            {
                tileRenderer.material = material;
            }
        }

        public void ForceRestoreColor()
        {
            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
                restoreCoroutine = null;
            }
            SetMaterialImmediate(defaultMaterial);
        }

        #endregion

        public GameTile[] GetNeighbourTiles(int range)
        {
            List<GameTile> results = new();
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (GameMap.TryGetTileAt(Coordinate.x + x, Coordinate.y + y, out GameTile tile))
                    {
                        results.Add(tile);
                    }
                }
            }
            return results.ToArray();
        }

        private void OnDrawGizmos()
        {
            if (IsOccupied)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);
            }

            // Draw coordinate text
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            if (GameMap.Instance != null && GameMap.Instance.IsDebug)
            {
                string debugText = $"({Coordinate.x},{Coordinate.y})";
                if (occupancyCount > 0)
                {
                    debugText += $"\nOcc: {occupancyCount}";
                }
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, debugText, style);
            }
#endif
        }

        private void OnDestroy()
        {
            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
            }
        }
    }
}