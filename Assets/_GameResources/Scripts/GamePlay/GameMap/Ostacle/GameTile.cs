using AYellowpaper.SerializedCollections;
using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Geckout
{

    public class GameTile : MonoBehaviour
    {
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] public bool IsOccupied;
        [SerializeField] private Collider tileCollider; // For raycast
        [SerializeField] private Outline m_outLine;
        [SerializeField] private SerializedDictionary<MapTileType, GameObject> m_tilesTypeDisplay;
        [SerializeField] private GameObject m_displayObject;
        [SerializeField] private GameObject hammer;

        private bool IsObstacle
        {
            get;
            set;
        }

        public MapTileData MapTileData { get; set; } = new();

        public Vector2Int Coordinate
        {
            get
            {
                return MapTileData.coordinate;
            }

            set { }
        }

        private void Awake()
        {
            SetTileType(MapTileType.Normal);
        }

        [SerializeField] private float restoreDelay = 0.05f;

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
            if (tileRenderer != null)
            {
                tileRenderer.enabled = true;
            }
            SetSelected(false);

            SetMaterialImmediate(defaultMaterial);
        }

        public void InitializeMaterials(Material oldMat, Material moveMat)
        {
            defaultMaterial = oldMat;
            moveMaterial = moveMat;
        }

        public void Initialize(MapTileData tileData)
        {
            MapTileData = tileData;
            SetTileType(tileData.type);
        }

        public void ShowHammer(bool show)
        {
            if (hammer != null)
            {
                hammer.SetActive(show);
            }
        }

        public void HideWall()
        {
            //IsOccupied = false;
            //m_tilesTypeDisplay[MapTileData.type].SetActive(false);
            SetTileType(MapTileType.Normal);
            hammer.SetActive(false);
            GameMap.Instance.TilesWall.Remove(this);
            GameMap.Instance.UpdateVisualAllBorderWalls();
            UpdateNeighborTiles();
        }

        public void UpdateNeighborTiles()
        {
            var dirs = new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
            foreach(var dir in dirs)
            {
                GameMap.TryGetTileAtCoord(Coordinate + dir, out var tile);
                if (tile != null && tile.MapTileData.type != MapTileType.Normal && tile.MapTileData.type != MapTileType.Portal)
                {
                    tile.UpdateVisualTile();
                }
            }
        }    

        public void UpdateVisualTile()
        {
            var tile = this;

            GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.up, out var tileUp);
            bool up = !(tileUp == null || (tileUp.MapTileData.type != MapTileType.Normal && tileUp.MapTileData.type != MapTileType.Portal));
            GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.down, out var tileDown);
            bool down = !(tileDown == null || (tileDown.MapTileData.type != MapTileType.Normal && tileDown.MapTileData.type != MapTileType.Portal));
            GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.left, out var tileLeft);
            bool left = !(tileLeft == null || (tileLeft.MapTileData.type != MapTileType.Normal && tileLeft.MapTileData.type != MapTileType.Portal));
            GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.right, out var tileRight);
            bool right = !(tileRight == null || (tileRight.MapTileData.type != MapTileType.Normal && tileRight.MapTileData.type != MapTileType.Portal));

            var rot = Vector3Int.zero;

            // Example logic: you need to replace these rules with your 6 types
            if (up && down && left && right)
            {
                tile.SetTileType(MapTileType.Wall4Side); // cross
                rot = new Vector3Int(0, 90, -90);
            }
            else if ((up && down && left) || (up && down && right) ||
                     (up && left && right) || (down && left && right))
            {
                tile.SetTileType(MapTileType.Wall3Side); // cross
                if (!up) rot = new Vector3Int(0, 90, -90);
                if (!down) rot = new Vector3Int(180, 90, -90);
                if (!left) rot = new Vector3Int(-90, 90, -90);
                if (!right) rot = new Vector3Int(90, 90, -90);
            }
            else if ((up && down) || (left && right))
            {
                tile.SetTileType(MapTileType.Wall2Side);
                if (left && right) rot = new Vector3Int(0, 90, -90);
                else rot = new Vector3Int(90, 90, -90);
            }
            else if ((up && right) || (right && down) || (down && left) || (left && up))
            {
                tile.SetTileType(MapTileType.WallCornerInside); // corner
                if (up && right) rot = new Vector3Int(180, 90, -90);
                if (right && down) rot = new Vector3Int(-90, 90, -90);
                if (down && left) rot = new Vector3Int(0, 90, -90);
                if (left && up) rot = new Vector3Int(90, 90, -90);
            }
            else if (up || down || left || right)
            {
                tile.SetTileType(MapTileType.Wall1Side); // dead end
                if (up) rot = new Vector3Int(180, 90, -90);
                if (down) rot = new Vector3Int(0, 90, -90);
                if (left) rot = new Vector3Int(90, 90, -90);
                if (right) rot = new Vector3Int(-90, 90, -90);
            }
            else
            {
                tile.SetTileType(MapTileType.WallCenter); // single block
                rot = new Vector3Int(0, 90, -90);
            }

            tile.RotateTo(rot);
        }    

        public void SetTileType(MapTileType tileType)
        {
            if (m_tilesTypeDisplay == null || m_tilesTypeDisplay.Count <= 0)
            {
                return;
            }
            //m_tileTypeDisplay.Values.ToList().ForEach(x => x.gameObject.SetActive(false));

            if (MapTileData.type != MapTileType.Normal)
            {
                m_displayObject.SetActive(false);
            }
            MapTileData.type = tileType;
            IsOccupied = tileType != MapTileType.Normal;
            IsObstacle = tileType != MapTileType.Normal;
            if (m_tilesTypeDisplay.ContainsKey(tileType))
            {
                m_displayObject = m_tilesTypeDisplay[tileType];
                m_displayObject.SetActive(true);
                m_displayObject.transform.localRotation = Quaternion.Euler(MapTileData.rotation);
            }

            if (m_tilesTypeDisplay.ContainsKey(MapTileType.Normal))
            {
                m_tilesTypeDisplay[MapTileType.Normal].SetActive(true);
            }

        }

        public void ChangeVisualToNormalTile()
        {
            m_displayObject.SetActive(false);
        }

        public void RotateBy(int deltaAngle)
        {
            if (m_displayObject == null)
            {
                return;
            }
            var currentRotation = m_displayObject.transform.localRotation.eulerAngles;
            var angleRotated = (int)(currentRotation.x + deltaAngle);
            if (angleRotated % 90 != 0)
            {
                angleRotated = (angleRotated / 90) * 90;
            }
            var rotatateAngle = new Vector3Int(angleRotated, 90, -90);
            m_displayObject.transform.localRotation = Quaternion.Euler(rotatateAngle);
            MapTileData.rotation = rotatateAngle;
        }

        public void RotateTo(Vector3Int angle)
        {
            if (m_displayObject != null)
                m_displayObject.transform.localRotation = Quaternion.Euler(angle);
            MapTileData.rotation = angle;
        }

        public void SetSelected(bool selected)
        {
            if (m_outLine != null)
            {
                m_outLine.enabled = selected;
            }
        }
        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }
        public void SetCoordinate(int x, int y)
        {
            MapTileData.coordinate = new Vector2Int(x, y);
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

            // If no more occupants, start restore process
            if (occupancyCount == 0)
            {
                StartRestoreProcess();
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

        //public GameTile[] GetNeighbourTiles(int range)
        //{
        //    List<GameTile> results = new();
        //    for (int x = -range; x <= range; x++)
        //    {
        //        for (int y = -range; y <= range; y++)
        //        {
        //            if (x == 0 && y == 0) continue;
        //            if (GameMap.TryGetTileAt(Coordinate.x + x, Coordinate.y + y, out GameTile tile))
        //            {
        //                results.Add(tile);
        //            }
        //        }
        //    }
        //    return results.ToArray();
        //}

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
                //if (occupancyCount > 0)
                //{
                //    debugText += $"\nOcc: {occupancyCount}";
                //}
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