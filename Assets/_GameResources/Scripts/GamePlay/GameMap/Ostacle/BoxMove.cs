using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Geckout
{
    

    public class BoxMove : MonoBehaviour
    {
        [SerializeField] private LayerMask moveBoxLayer;
        [SerializeField] private GameObject m_tileCornerPrefab;
        [SerializeField] private GameObject m_tileEdgePrefab;
        [SerializeField] private GameObject m_tileCenterPrefab;

        private WayDirection wayDirection = WayDirection.Horizontal;
        private MovableBoxData movableBoxData;

        public void Init(MovableBoxData movableBoxData)
        {
            SpawnTiles();
        }

        void SpawnTiles()
        {
            var boxSize = movableBoxData.boxSize;
            float cellSize = 1;
            Vector3 centerOffset = new Vector3(
                (boxSize.x - 1) * cellSize * 0.5f,
                (boxSize.y - 1) * cellSize * 0.5f,
                0
            );

            for (int x = 0; x < boxSize.x; x++)
            {
                for (int y = 0; y < boxSize.y; y++)
                {
                    Vector2Int c = new Vector2Int(x, y);

                    // matrix coordinate ? world position
                    Vector3 pos = new Vector3(c.x * cellSize, c.y * cellSize, 0);
                    pos -= centerOffset; // center grid

                    GameObject obj;
#if UNITY_EDITOR
                    obj = ((GameObject)PrefabUtility.InstantiatePrefab(m_tileCenterPrefab, transform));
#else
                    obj = Instantiate(prefab, _tilesContainer);
#endif

                    obj.transform.localPosition = pos;
                    obj.transform.localScale = Vector3.one * 1;
                    obj.name = $"Tile_{c.x}_{c.y}";

                }
            }

            SpawnBorders(boxSize, 1, 1, centerOffset);

        }

        private void SpawnBorders(Vector2Int gridSize, float cellSize, float cubeSize, Vector3 centerOffset)
        {
            // Corners
            var wallCornerPrefab = m_tileCornerPrefab;
            var wallEdgePrefab = m_tileEdgePrefab;

            var angleCornerBottomLeft = new Vector3(270, -90, 90);
            PlaceBorder(wallCornerPrefab, new Vector2Int(-1, -1), cellSize, cubeSize, centerOffset, "Corner_BottomLeft", angleCornerBottomLeft);
            var angleCornerBottomRight = new Vector3(0, -90, 90);
            PlaceBorder(wallCornerPrefab, new Vector2Int(gridSize.x, -1), cellSize, cubeSize, centerOffset, "Corner_BottomRight", angleCornerBottomRight);
            var angleCornerTopLeft = new Vector3(0, 90, -90);
            PlaceBorder(wallCornerPrefab, new Vector2Int(-1, gridSize.y), cellSize, cubeSize, centerOffset, "Corner_TopLeft", angleCornerTopLeft);
            var angleCornerTopRight = new Vector3(90, -90, 90);
            PlaceBorder(wallCornerPrefab, new Vector2Int(gridSize.x, gridSize.y), cellSize, cubeSize, centerOffset, "Corner_TopRight", angleCornerTopRight);

            // Bottom edge
            var angleBottomEdge = new Vector3(180, 90, -90);
            for (int x = 0; x < gridSize.x; x++)
                PlaceBorder(wallEdgePrefab, new Vector2Int(x, -1), cellSize, cubeSize, centerOffset, $"Wall_Bottom_{x}", angleBottomEdge);

            // Top edge
            var angleTopEdge = new Vector3(0, 90, -90);
            for (int x = 0; x < gridSize.x; x++)
                PlaceBorder(wallEdgePrefab, new Vector2Int(x, gridSize.y), cellSize, cubeSize, centerOffset, $"Wall_Top_{x}", angleTopEdge);

            // Left edge
            var angleLeftEdge = new Vector3(270, -90, 90);
            for (int y = 0; y < gridSize.y; y++)
                PlaceBorder(wallEdgePrefab, new Vector2Int(-1, y), cellSize, cubeSize, centerOffset, $"Wall_Left_{y}", angleLeftEdge);

            // Right edge
            var angleRightEdge = new Vector3(90, -90, 90);
            for (int y = 0; y < gridSize.y; y++)
                PlaceBorder(wallEdgePrefab, new Vector2Int(gridSize.x, y), cellSize, cubeSize, centerOffset, $"Wall_Right_{y}", angleRightEdge);
        }

        private void PlaceBorder(GameObject prefab, Vector2Int c, float cellSize, float cubeSize, Vector3 centerOffset, string name, Vector3 localRotation)
        {
            if (prefab == null) return;

            Vector3 pos = new Vector3(c.x * cellSize, c.y * cellSize, 0) - centerOffset;

            GameObject obj;
#if UNITY_EDITOR
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
#else
            obj = Instantiate(prefab.gameObject, m_wallContainer);
#endif
            obj.name = name;
            obj.transform.localPosition = pos;
            obj.transform.localScale = Vector3.one * cubeSize;
            obj.transform.localRotation = Quaternion.Euler(localRotation);
        }

        private void Move()
        {
            // check way direction
            switch (wayDirection)
            {
                case WayDirection.Horizontal:
                    // move left or right
                    break;
                case WayDirection.Vertical:
                    // move up or down
                    break;
                case WayDirection.All:
                    // move in any direction
                    break;
                default:
                    break;
            }
        }
    }
}
