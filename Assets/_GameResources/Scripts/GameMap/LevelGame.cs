using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Geckout
{
    public class LevelGame : MonoBehaviour
    {
        [SerializeField] private GameLevelData m_gameLevelData;
        [SerializeField] private GameMap m_gameMap;
        [SerializeField] private BodyController m_dogPrefab;

        private HashSet<GameTile> listSelectedTile = new();

        public GameLevelData GameLevelData => m_gameLevelData;

        public void SetLevelData(GameLevelData gameLevelData)
        {
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);   
#endif
        }

        private void Update()
        {
            if(Input.GetMouseButtonUp(0))
            {
                var screenPoint = Input.mousePosition;
                var ray = RectTransformUtility.ScreenPointToRay(Camera.main, screenPoint);
                if(Physics.Raycast(ray, out var hitInfo,1000))
                {
                    if(hitInfo.transform.TryGetComponent<GameTile>(out var tile))
                    {
                        tile.SetSelected(true);
                        listSelectedTile.Add(tile);
                    }
                }
            }

            if(Input.GetMouseButtonUp(1))
            {
                var screenPoint = Input.mousePosition;
                var ray = RectTransformUtility.ScreenPointToRay(Camera.main, screenPoint);
                if (Physics.Raycast(ray, out var hitInfo, 1000))
                {
                    if (hitInfo.transform.TryGetComponent<GameTile>(out var tile))
                    {
                        tile.SetSelected(false);
                        listSelectedTile.Remove(tile);
                    }
                }
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                ClearAllSelectedTiles();
            }
        }

        public void ClearAllSelectedTiles()
        {
            listSelectedTile.ToList().ForEach(x => x.SetSelected(false));
            listSelectedTile.Clear();
        }
    }
}
