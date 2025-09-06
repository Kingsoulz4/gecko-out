using System;
using System.Collections;
﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Geckout.Data;
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
        [SerializeField] private Outline m_outLine;
        [SerializeField] private SerializedDictionary<MapTileType, GameObject> m_tilesTypeDisplay;
        [SerializeField] private GameObject m_displayObject;

        private LevelGame levelGame;

        private LevelGame LevelGame
        {
            get
            {
                if (levelGame == null) levelGame = GetComponentInParent<LevelGame>();
                return levelGame;
            }
        }

        public MapTileData MapTileData { get; set; } = new();

        public Vector2Int Coordinate {
            get 
            {
                return MapTileData.coordinate;
            }
            
            set
            {

            }
        }

        private void Awake()
        {
            SetTileType(MapTileType.Normal);
        }
       
        [SerializeField] private float restoreDelay =0.05f;

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

        public void SetTileType(MapTileType tileType)
        {
            if(m_tilesTypeDisplay == null || m_tilesTypeDisplay.Count <= 0)
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
            m_displayObject = m_tilesTypeDisplay[tileType];
            m_displayObject.SetActive(true);
            m_displayObject.transform.localRotation = Quaternion.Euler(MapTileData.rotation);
            m_tilesTypeDisplay[MapTileType.Normal].SetActive(true);
            if(tileType == MapTileType.Portal)
            {
                var portal = gameObject.AddComponent<Portal>();
                portal.PortalData = LevelGame.GameLevelData.listPortalData.Find(x => x.Coordinate == MapTileData.coordinate);
            }
        }

        public void ChangeVisualToNormalTile()
        {
            m_displayObject.SetActive(false);
        }    

        public void RotateBy(int deltaAngle)
        {
            if(m_displayObject == null)
            {
                return;
            }
            var currentRotation = m_displayObject.transform.localRotation.eulerAngles;
            var angleRotated = (int)(currentRotation.x + deltaAngle);
            if(angleRotated % 90 != 0)
            {
                angleRotated = (angleRotated / 90) * 90;
            }
            var rotatateAngle = new Vector3Int(angleRotated, 90, -90);
            m_displayObject.transform.localRotation = Quaternion.Euler(rotatateAngle);
            MapTileData.rotation = rotatateAngle;
        }

        public void SetSelected(bool selected)
        {
            if(m_outLine != null)
            { 
                m_outLine.enabled = selected;
            }
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