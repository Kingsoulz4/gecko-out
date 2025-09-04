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
        [SerializeField] private Transform m_listDogContainer;

        private HashSet<GameTile> listSelectedTile = new();

        public List<GameTile> ListSelectedTile { get => listSelectedTile.ToList(); }

        public GameLevelData GameLevelData => m_gameLevelData;

        public BodyController selectedDog { get; set; }

        public GameMap GameMap => m_gameMap;

        public void SetLevelData(GameLevelData gameLevelData)
        {
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
            Utils.RemoveAllChilds(m_listDogContainer);
            foreach(var dogData in gameLevelData.listDogData)
            {
                SpawnDog(dogData);
            }
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

        public void SpawnDog(DogData dogData)
        {
            var newDog = Instantiate(m_dogPrefab, m_listDogContainer);
            newDog.Initialize(dogData);
        }

        public void GenerateNewDog(DogData dogData)
        {
            m_gameLevelData.listDogData.Add(dogData);
            dogData.listCoordinate = new List<Vector2Int>(listSelectedTile.Select(x => x.Coordinate).ToList());
            SpawnDog(dogData);
        }

        public void DeleteSelectedDog()
        {
            if(selectedDog == null)
            {
                return;
            }    
            Destroy(selectedDog.gameObject);
        }    

        public void ClearAllSelectedTiles()
        {
            listSelectedTile.ToList().ForEach(x => x.SetSelected(false));
            listSelectedTile.Clear();
        }

        public void ChangeTypeSelectedTiles(MapTileType tileType)
        {
            listSelectedTile.ToList().ForEach(x => x.SetTileType(tileType));
        }

        public void RotateSelectedTiles(float angle)
        {
            listSelectedTile.ToList().ForEach(x => x.RotateBy(angle));
        }
    }
}
