using System;
using System.Collections;
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

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }

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

        public void ChangeColor(Color color)
        {
            if (tileRenderer != null)
            {
                tileRenderer.material.color = color;
            }
        }

        // Highlight tile for path visualization
        public void SetPathHighlight(bool highlight)
        {
            if (highlight)
            {
                ChangeColor(Color.yellow);
            }
            else
            {
                ChangeColor(Color.white); // Default color
            }
        }

        private void OnValidate()
        {
            tileRenderer.enabled = true;
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
            //if(GameMap.Instance.IsDebug)
            //UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
            //    $"({Coordinate.x},{Coordinate.y})", style);
#endif
        }
    }
}