using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Geckout
{
    public partial class LevelGameEditTool: LevelGame
    {
        private HashSet<GameTile> listSelectedTile = new();
        private List<BodyController> listBody = new();

        public List<GameTile> ListSelectedTile { get => listSelectedTile.ToList(); }

        public BodyController selectedDog { get; set; }

        public Portal selectedPortal
        {
            get
            {
                //var listt = listSelectedTile.ToList();
                var tile = listSelectedTile.ToList().Find(x => x.MapTileData.type == MapTileType.Portal);

                if (tile != null)
                {
                    return tile.TryGetComponent<Portal>(out var portal) ? portal : null;
                }
                return null;
            }
        }

        private void Update()
        {
            if (!GamePlayManager.Instance.IsEdittingLevel) return;

            if (Input.GetMouseButton(0))
            {
                var screenPoint = Input.mousePosition;
                var ray = RectTransformUtility.ScreenPointToRay(Camera.main, screenPoint);
                if (Physics.Raycast(ray, out var hitInfo, 1000))
                {
                    if (hitInfo.transform.TryGetComponent<GameTile>(out var tile))
                    {
                        tile.SetSelected(true);
                        listSelectedTile.Add(tile);
                        return;
                    }

                    var dogBody = hitInfo.transform.GetComponentInParent<BodyController>();
                    if (dogBody != null)
                    {
                        SelectDog(dogBody);
                    }
                }
            }

            if (Input.GetMouseButtonUp(1))
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

            

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearAllSelected();
            }
        }

        public void SelectDog(BodyController dogBody)
        {
            if (selectedDog != null)
            {
                selectedDog.SetSelected(false);
            }
            selectedDog = dogBody;
            dogBody.SetSelected(true);
        }

        public BodyController GenerateNewDog(DogData dogData)
        {
            if(listSelectedTile.Count < 3)
            {
                throw new System.Exception("Not Enough Tiles Selected");
            }

            m_gameLevelData.listDogData.Add(dogData);

            for(int i=0; i < listSelectedTile.Count; i++)
            {
                var tile = listSelectedTile.ElementAt(i);
                
                if (tile.IsOccupied || tile.MapTileData.type != MapTileType.Normal)
                {
                    throw new System.Exception("Tile Error");
                }

                if(i >= 1)
                {
                    var previousTile = listSelectedTile.ElementAt(i - 1);
                    var sumCoord = Mathf.Abs(tile.Coordinate.x - previousTile.Coordinate.x) + Mathf.Abs(tile.Coordinate.y - previousTile.Coordinate.y);
                    if(sumCoord > 1)
                    {
                        throw new System.Exception("Tile Error");
                    }
                }
            }
                
            dogData.listCoordinate = new List<Vector2Int>(listSelectedTile.Select(x => x.Coordinate).ToList());
            return SpawnDog(dogData);
        }

        public void DeleteSelectedDog()
        {
            if (selectedDog == null)
            {
                return;
            }
            GameLevelData.listDogData.Remove(selectedDog.DogData);
            Destroy(selectedDog.gameObject);
        }

        public void ClearAllSelected()
        {
            ClearAllSelectedTiles();
            if (selectedDog != null)
            {
                selectedDog.SetSelected(false);
                selectedDog = null;
            }
        }

        public void ClearAllSelectedTiles()
        {
            listSelectedTile.ToList().ForEach(x => x.SetSelected(false));
            listSelectedTile.Clear();
        }

        public void AddNewPortal()
        {
            ChangeTypeSelectedTiles(MapTileType.Portal);
            for (int i = 0; i < listSelectedTile.Count; i++)
            {
                var tileSelected = listSelectedTile.ElementAt(i);
                var newPortalData = new PortalData();
                newPortalData.Coordinate = new Vector2Int(tileSelected.Coordinate.x, tileSelected.Coordinate.y);
                var newPortal = tileSelected.gameObject.AddComponent<Portal>();
                GameLevelData.listPortalData.Add(newPortalData);
                newPortal.PortalData = newPortalData;
            }
        }

        public void RemoveAllSelectedPortals()
        {
            var listPortal = listSelectedTile.ToList().FindAll(x => x.MapTileData.type == MapTileType.Portal);
            for (int i = 0; i < listPortal.Count; i++)
            {
                var portalSelected = listPortal[i].GetComponent<Portal>();
                GameLevelData.listPortalData.Remove(portalSelected.PortalData);
                listPortal[i].SetTileType(MapTileType.Normal);
            }

        }

        public void ChangeTypeSelectedTiles(MapTileType tileType)
        {
            listSelectedTile.ToList().ForEach(x => x.SetTileType(tileType));
            if(tileType == MapTileType.Normal)
            {
                ClearAllSelectedTiles();
            }
        }

        public void RotateSelectedTiles(int angle)
        {
            listSelectedTile.ToList().ForEach(x => x.RotateBy(angle));
        }

    }
}
