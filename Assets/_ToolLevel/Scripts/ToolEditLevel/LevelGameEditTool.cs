using Geckout.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Geckout
{
    public partial class LevelGameEditTool: LevelController
    {
        private HashSet<GameTile> listSelectedTile = new();
        private List<BodyController> listBody = new();

        public List<GameTile> ListSelectedTile { get => listSelectedTile.ToList(); }

        public BodyController selectedBody { get; set; }

        public Portal SelectedPortal
        {
            get
            {
                if (listSelectedTile.Count <= 0) return null;
                GameMap.TryGetPortalAtCoord(listSelectedTile.First().MapTileData.coordinate, out var portal);

                return portal ? portal : null;

            }
        }

        public BoxMove SelectedBoxMove
        {
            get; set;
        }

        public Crate SelectedCrate
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
                        SelectMovableBox(boxMove);
                    }

                    if (hitInfo.transform.parent.TryGetComponent<Crate>(out var crate))
                    {
                        SelectCrateBox(crate);
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
                        if (SelectedBoxMove != null)
                        {
                            SelectedBoxMove.SetSelected(false);
                            SelectedBoxMove = null;
                        }
                    }
                }
            }

            

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearAllSelected();
            }
        }

        public void SelectMovableBox(BoxMove boxBase) 
        {
            if(SelectedBoxMove != null)
            {
                SelectedBoxMove.SetSelected(false);
            }
           
            SelectedBoxMove = boxBase;
            SelectedBoxMove.SetSelected(true);
            ToolEditLevelManager.OnClickEditBoxes();
        }

        public void SelectCrateBox(Crate crate)
        {
            if (SelectedCrate != null)
            {
                SelectedCrate.SetSelected(false);
            }

            SelectedCrate = crate;
            SelectedCrate.SetSelected(true);
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

            m_gameLevelData.listDogData.Add(dogData);
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

        public void DeleteSelectedBoxes()
        {
            if(SelectedCrate != null)
            {
                GameLevelData.listCrateData.Remove(SelectedCrate.Data);
                Destroy(SelectedCrate.gameObject);
            }

            if(SelectedBoxMove != null)
            {
                GameLevelData.listMovableBoxData.Remove(SelectedBoxMove.Data);
                Destroy(SelectedBoxMove.gameObject);
            }
        }

        public void DeleteSelectedPortals()
        {
            for(int i =0; i<listSelectedTile.Count; i++)
            {
                var tile = listSelectedTile.ElementAt(i);
                GameMap.TryGetPortalAtCoord(tile.Coordinate, out var portal);
                if (portal != null)
                {
                    GameLevelData.listPortalData.Remove(portal.PortalData);
                    tile.SetTileType(MapTileType.Normal);
                    Destroy(portal.gameObject);
                }
            }
        }

        public void ClearAllSelected()
        {
            ClearAllSelectedTiles();
            ClearSelectedBody();
            ClearSelectedMovableBoxes();
            ClearSelectedCrates();
        }

        public void ClearSelectedBody()
        {
            if (selectedBody != null)
            {
                selectedBody.SetSelected(false);
                selectedBody = null;
            }
        }

        public void ClearSelectedMovableBoxes()
        {
            if(SelectedBoxMove != null) SelectedBoxMove.SetSelected(false);
            SelectedBoxMove = null; 
        }

        public void ClearSelectedCrates()
        {
            if (SelectedCrate != null) SelectedCrate.SetSelected(false);
            SelectedCrate = null;
        }
            

        public void ClearAllSelectedTiles()
        {
            listSelectedTile.ToList().ForEach(x => x.SetSelected(false));
            listSelectedTile.Clear();
        }

        public void AddNewPortal(PortalData portalData)
        {
            
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
                if (tileSelected.IsOccupied) continue;
                var newPortalData = new PortalData(portalData);
                newPortalData.Coordinate = new Vector2Int(tileSelected.Coordinate.x, tileSelected.Coordinate.y);
                tileSelected.IsOccupied = true;
                tileSelected.SetTileType(MapTileType.Portal);
                GameMap.SpawnPortal(newPortalData);
                GameLevelData.listPortalData.Add(newPortalData);
            }
            //ChangeTypeSelectedTiles(MapTileType.Portal);
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

        public void AutoGenerateWallTiles()
        {
            var listCoordTileSelected = ListSelectedTile.Where(x => !x.IsOccupied).ToList();
            foreach (var tile in listCoordTileSelected)
            {
                GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.up, out var tileUp);
                bool up = !(tileUp == null || (tileUp.MapTileData.type != MapTileType.Normal && tileUp.MapTileData.type != MapTileType.Portal)
                    || listCoordTileSelected.Contains(tileUp));
                GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.down, out var tileDown);
                bool down = !(tileDown == null || (tileDown.MapTileData.type != MapTileType.Normal && tileDown.MapTileData.type != MapTileType.Portal)
                    || listCoordTileSelected.Contains(tileDown));
                GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.left, out var tileLeft);
                bool left = !(tileLeft == null || (tileLeft.MapTileData.type != MapTileType.Normal && tileLeft.MapTileData.type != MapTileType.Portal)
                    || listCoordTileSelected.Contains(tileLeft));
                GameMap.TryGetTileAtCoord(tile.Coordinate + Vector2Int.right, out var tileRight);
                bool right = !(tileRight == null || (tileRight.MapTileData.type != MapTileType.Normal && tileRight.MapTileData.type != MapTileType.Portal)
                    || listCoordTileSelected.Contains(tileRight));

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
                    if (!left) rot = new Vector3Int(-90, 90, -90) ;
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



        }

    }
}
