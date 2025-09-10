using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Geckout
{
    public partial class LevelGameEditTool: LevelController
    {
        private HashSet<GameTile> listSelectedTile = new();
        private List<BodyController> listBody = new();

        public List<GameTile> ListSelectedTile { get => listSelectedTile.ToList(); }

        public BodyController selectedBody { get; set; }

        public Portal selectedPortal
        {
            get
            {
                //var listt = listSelectedTile.ToList();
                var tile = listSelectedTile.ToList().Find(x => x.MapTileData.type == MapTileType.Portal);
                if (listSelectedTile.Count <= 0) return null;
                GameMap.TryGetPortalAtCoord(listSelectedTile.First().MapTileData.coordinate, out var portal);

                if (tile != null)
                {
                    return portal ? portal : null;
                }
                return null;
            }
        }

        public BoxMove SelectedBoxMove
        {
            get; set;
        }

        private ToolEditLevelManager toolEditLevelManager;
        public ToolEditLevelManager ToolEditLevelManager
        {
            get
            {
                if(toolEditLevelManager == null)
                {
                    toolEditLevelManager = FindObjectOfType<ToolEditLevelManager>();
                }
                return toolEditLevelManager;
            }
        }

        private void Update()
        {
            if (!LevelManager.Instance.IsEdittingLevel) return;

            if (Input.GetMouseButton(0))
            {
                var screenPoint = Input.mousePosition;
                var ray = RectTransformUtility.ScreenPointToRay(Camera.main, screenPoint);
                if (Physics.Raycast(ray, out var hitInfo, 1000))
                {
                    if (hitInfo.transform.TryGetComponent<GameTile>(out var tile))
                    {
                        SelectTile(tile);
                        return;
                    }

                    if(hitInfo.transform.parent.TryGetComponent<BoxMove>(out var boxMove))
                    {
                        SelectBoxMove(boxMove);
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

                    if (hitInfo.transform.parent.TryGetComponent<BoxMove>(out var boxMove))
                    {
                        SelectedBoxMove.SetSelected(false);
                        SelectedBoxMove = null;
                    }
                }
            }

            

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearAllSelected();
            }
        }

        public void SelectBoxMove(BoxMove boxMove)
        {
            if(SelectedBoxMove != null)
            {
                SelectedBoxMove.SetSelected(false);
            }
            SelectedBoxMove = boxMove;
            SelectedBoxMove.SetSelected(true);
            ToolEditLevelManager.OnClickEditBoxes();
        }

        public void SelectTile(GameTile tile)
        {
            if(tile.MapTileData.type == MapTileType.Portal)
            {
                ClearSelectedBody();
                ToolEditLevelManager.OnClickEditPortals();
            }
            else if(tile.MapTileData.type == MapTileType.Normal)
            {

            }
            else
            {
                ClearSelectedBody();
                ToolEditLevelManager.OnClickEditWalls();
            }

            tile.SetSelected(true);
            listSelectedTile.Add(tile);
        }

        public void SelectDog(BodyController dogBody)
        {
            if (selectedBody != null)
            {
                selectedBody.SetSelected(false);
            }
            ClearAllSelectedTiles();
            selectedBody = dogBody;
            dogBody.SetSelected(true);
            ToolEditLevelManager.OnClickDesignDog();
        }

        public BodyController GenerateNewBody(BodyData dogData)
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
            return SpawnBody(dogData);
        }

        public void DeleteSelectedBody()
        {
            if (selectedBody == null)
            {
                return;
            }
            GameLevelData.listDogData.Remove(selectedBody.BodyData);
            Destroy(selectedBody.gameObject);
        }

        public void ClearAllSelected()
        {
            ClearAllSelectedTiles();
            ClearSelectedBody();
            ClearSelectedMovableBox();
        }

        public void ClearSelectedBody()
        {
            if (selectedBody != null)
            {
                selectedBody.SetSelected(false);
                selectedBody = null;
            }
        }

        public void ClearSelectedMovableBox()
        {
            if(SelectedBoxMove != null) SelectedBoxMove.SetSelected(false);
            SelectedBoxMove = null; 
        }
            

        public void ClearAllSelectedTiles()
        {
            listSelectedTile.ToList().ForEach(x => x.SetSelected(false));
            listSelectedTile.Clear();
        }

        public void AddNewPortal()
        {
            ChangeTypeSelectedTiles(MapTileType.Portal);
            //for (int i = 0; i < listSelectedTile.Count; i++)
            //{
            //    var tileSelected = listSelectedTile.ElementAt(i);
            //    var newPortalData = new PortalData();
            //    newPortalData.Coordinate = new Vector2Int(tileSelected.Coordinate.x, tileSelected.Coordinate.y);
            //    var newPortal = tileSelected.gameObject.AddComponent<Portal>();
            //    GameLevelData.listPortalData.Add(newPortalData);
            //    newPortal.PortalData = newPortalData;
            //}

            for (int i = 0; i < listSelectedTile.Count; i++)
            {
                var tileSelected = listSelectedTile.ElementAt(i);
                var newPortalData = new PortalData();
                newPortalData.Coordinate = new Vector2Int(tileSelected.Coordinate.x, tileSelected.Coordinate.y);
                GameMap.SpawnPortal(newPortalData);
            }
        }

        public void RemoveAllSelectedPortals()
        {
            var listPortal = listSelectedTile.ToList().FindAll(x => x.MapTileData.type == MapTileType.Portal);

            var listPortalObject = new List<Portal>();

            foreach (var portal in listPortal)
            {
                LevelManager.Instance.LevelGame.GameMap.TryGetPortalAtCoord(portal.Coordinate, out var portalObject);
                listPortalObject.Add(portalObject);
            }

            for (int i = 0; i < listPortalObject.Count; i++)
            {
                var portalSelected = listPortalObject[i];
                GameLevelData.listPortalData.Remove(portalSelected.PortalData);
                listPortal[i].SetTileType(MapTileType.Normal);
                Destroy(portalSelected);
            }

        }

        public void ChangeTypeSelectedTiles(MapTileType tileType)
        {
            listSelectedTile.ToList().ForEach(x =>
            {
                if ((!x.IsOccupied && tileType != MapTileType.Normal) || tileType == MapTileType.Normal)
                {
                    x.SetTileType(tileType);
                }

            }
            );
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
