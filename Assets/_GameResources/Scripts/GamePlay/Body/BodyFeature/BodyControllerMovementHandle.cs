using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public partial class BodyController 
    {
        private Coroutine moveShortDistanceCoroutine;

        private Coroutine coroutineUpdateAllSegmentsPos;

        private Vector2Int lastDirectionShortMove;

        public void MoveShortDistance(float amplitude, Vector2Int direction, Vector2Int newHeadCoord, ControlAnchor controlAnchor)
        {
            if (moveShortDistanceCoroutine != null)
            {
                StopCoroutine(moveShortDistanceCoroutine);
                moveShortDistanceCoroutine = null;
            }

            bool moveForward = true;

            var directions = new List<Vector2Int>() { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up};

            if (controlAnchor == ControlAnchor.Head)
            {
                var currentBackwardDir = BodyData.listCoordinate[1] - BodyData.listCoordinate[0];
                moveForward = currentBackwardDir != direction;

                if (moveForward)
                {
                    AddCoordinate(newHeadCoord, controlAnchor);
                }
                else if(!moveForward && newHeadCoord != BodyData.listCoordinate[0])
                {
                    var tail = BodyData.listCoordinate[^1];
                    var preTail = BodyData.listCoordinate[^2];
                    var tailDir = tail - preTail;
                    var newTailCoord = tail + tailDir;

                    if (GameMap.TryGetTileAtCoord(newTailCoord, out var tile) && !tile.IsOccupied)
                    {
                        AddCoordinate(newTailCoord, ControlAnchor.Tail);
                    }
                    else
                    {
                        foreach (var dir in directions)
                        {
                            newTailCoord = tail + dir;
                            if (GameMap.TryGetTileAtCoord(newTailCoord, out var newTile) && !newTile.IsOccupied)
                            {
                                AddCoordinate(newTailCoord, ControlAnchor.Tail);
                                break;
                            }
                        }
                    }
                }
                    
            }
            else if(controlAnchor == ControlAnchor.Tail)
            {
                var currentBackwardDir = BodyData.listCoordinate[^2] - BodyData.listCoordinate[^1];
                moveForward = currentBackwardDir != direction;

                if (moveForward)
                {
                    AddCoordinate(newHeadCoord, controlAnchor);
                }
                else if (!moveForward && newHeadCoord != BodyData.listCoordinate[^1])
                {
                    var head = BodyData.listCoordinate[^1];
                    var afterHead = BodyData.listCoordinate[^2];
                    var headDir = head - afterHead;
                    var newTailCoord = head + headDir;

                    if (GameMap.TryGetTileAtCoord(newTailCoord, out var tile) && !tile.IsOccupied)
                    {
                        AddCoordinate(newTailCoord, ControlAnchor.Head);
                    }
                    else
                    {
                        foreach (var dir in directions)
                        {
                            newTailCoord = head + dir;
                            if (GameMap.TryGetTileAtCoord(newTailCoord, out var newTile) && !newTile.IsOccupied)
                            {
                                AddCoordinate(newTailCoord, ControlAnchor.Head);
                                break;
                            }
                        }
                    }
                }
            }

            moveShortDistanceCoroutine = StartCoroutine(IEMoveShortDistance(amplitude, direction, controlAnchor,moveForward));
        }

        IEnumerator IEMoveShortDistance(float amplitude, Vector2Int direct, ControlAnchor controlAnchor, bool moveForward = true)
        {
            if (lastDirectionShortMove != direct)
            {
                yield return IEUpdateSegmentsPos();
                lastDirectionShortMove = direct;
            }

            Vector3 direction = new Vector3(direct.x, direct.y);

            List<Vector3> dirs = new List<Vector3>() { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            Debug.Log($"Init Dir point short distance: {direction}");

            if (controlAnchor == ControlAnchor.Head)
            {
                if (moveForward)
                {
                    Debug.Log("Move Short Forward Head");
                    Segments[0].transform.position += direction * amplitude;
                    //UpStraightSubSegments();

                    for (int i = 1; i < Segments.Count; i++)
                    {
                        var currentCoord = Segments[i].Coordinate;
                        var indexOfCoord = BodyData.listCoordinate.IndexOf(currentCoord);
                        var nexCoord = BodyData.listCoordinate[Mathf.Clamp(indexOfCoord - 1, 0, BodyData.listCoordinate.Count - 1)];
                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out var tile))
                        {
                            if (Vector3.Distance(Segments[i].transform.position, tile.transform.position) <= 0.01f)
                            {
                                Segments[i].transform.position = tile.transform.position;
                                Segments[i].Coordinate = nexCoord;
                            }
                        }

                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out tile))
                        {
                            var dir = tile.transform.position - Segments[i].transform.position;
                            if (dir != Vector3.zero)
                            {
                                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                                {
                                    dir = Vector3.right * (dir.x / Mathf.Abs(dir.x));
                                }
                                else
                                {
                                    dir = Vector3.up * (dir.y / Mathf.Abs(dir.y));
                                }

                                Segments[i].transform.position += dir * amplitude;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Move Short Backward Head");
                    Segments[0].transform.position += direction * amplitude;
                    ReverseSubSegments();

                    for (int i = Segments.Count -2; i >= 0; i--)
                    {
                        var currentCoord = Segments[i].Coordinate;
                        var indexOfCoord = BodyData.listCoordinate.IndexOf(currentCoord);
                        var nexCoord = BodyData.listCoordinate[Mathf.Clamp(indexOfCoord + 1, 0, BodyData.listCoordinate.Count - 1)];
                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out var tile))
                        {
                            if (Vector3.Distance(Segments[i].transform.position, tile.transform.position) <= 0.01f)
                            {
                                Segments[i].transform.position = tile.transform.position;
                                Segments[i].Coordinate = nexCoord;
                            }
                        }

                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out tile))
                        {
                            var dir = tile.transform.position - Segments[i].transform.position;
                            if (dir != Vector3.zero)
                            {
                                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                                {
                                    dir = Vector3.right * (dir.x / Mathf.Abs(dir.x));
                                }
                                else
                                {
                                    dir = Vector3.up * (dir.y / Mathf.Abs(dir.y));
                                }

                                Segments[i].transform.position += dir * amplitude;
                            }
                        }
                    }
                }

            }
            else if (controlAnchor == ControlAnchor.Tail)
            {
                if (moveForward)
                {
                    Debug.Log("Move Short Forward Tail");
                    Segments[^1].transform.position += direction * amplitude;
                    ReverseSubSegments();

                    for (int i = Segments.Count - 2; i >= 0; i--)
                    {
                        var currentCoord = Segments[i].Coordinate;
                        var indexOfCoord = BodyData.listCoordinate.IndexOf(currentCoord);
                        var nexCoord = BodyData.listCoordinate[Mathf.Clamp(indexOfCoord + 1, 0, BodyData.listCoordinate.Count - 1)];
                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out var tile))
                        {
                            if (Vector3.Distance(Segments[i].transform.position, tile.transform.position) <= 0.01f)
                            {
                                Segments[i].transform.position = tile.transform.position;
                                Segments[i].Coordinate = nexCoord;
                            }
                        }

                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out tile))
                        {
                            var dir = tile.transform.position - Segments[i].transform.position;
                            if (dir != Vector3.zero)
                            {
                                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                                {
                                    dir = Vector3.right * (dir.x / Mathf.Abs(dir.x));
                                }
                                else
                                {
                                    dir = Vector3.up * (dir.y / Mathf.Abs(dir.y));
                                }

                                Segments[i].transform.position += dir * amplitude;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Move Short Backward Tail");
                    Segments[^1].transform.position += direction * amplitude;
                    //UpdateAllSegmentPos();

                    for (int i = 1; i < Segments.Count; i++)
                    {
                        var currentCoord = Segments[i].Coordinate;
                        var indexOfCoord = BodyData.listCoordinate.IndexOf(currentCoord);
                        var nexCoord = BodyData.listCoordinate[Mathf.Clamp(indexOfCoord - 1, 0, BodyData.listCoordinate.Count - 1)];
                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out var tile))
                        {
                            if (Vector3.Distance(Segments[i].transform.position, tile.transform.position) <= 0.01f)
                            {
                                Segments[i].transform.position = tile.transform.position;
                                Segments[i].Coordinate = nexCoord;
                            }
                        }

                        if (GameMap.TryGetTileAtCoord(Segments[i].Coordinate, out tile))
                        {
                            var dir = tile.transform.position - Segments[i].transform.position;
                            if (dir != Vector3.zero)
                            {
                                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                                {
                                    dir = Vector3.right * (dir.x / Mathf.Abs(dir.x));
                                }
                                else
                                {
                                    dir = Vector3.up * (dir.y / Mathf.Abs(dir.y));
                                }

                                Segments[i].transform.position += dir * amplitude;
                            }
                        }
                    }
                }
            }

            
            yield return new WaitForSeconds(0.01f);

            occupiedTileController?.UpdateAllSegmentPositions();

        }

        public void UpdateAllSegmentPos()
        {
            Debug.Log("Update All Segments Pos here");
            
            if(coroutineUpdateAllSegmentsPos != null)
            {
                StopCoroutine(coroutineUpdateAllSegmentsPos);
                coroutineUpdateAllSegmentsPos = null;
            }

            coroutineUpdateAllSegmentsPos = StartCoroutine(IEUpdateSegmentsPos());
        }

        private IEnumerator IEUpdateSegmentsPos()
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                var listDefaultCoordinate = BodyData.listCoordinate;

                int unitIndex = i / subLength;

                var coordinate = listDefaultCoordinate[Mathf.Clamp(unitIndex, 0, listDefaultCoordinate.Count - 1)];
                Vector3 targetPos = Segments[i].transform.position;
                Segments[i].transform.DOKill();

                if (i % SubLength == 0)
                {
                    if (GameMap.TryGetTileAtCoord(coordinate, out var tile))
                    {
                        Segments[i].Coordinate = coordinate;
                        targetPos = tile.transform.position;
                    }
                }
                else
                {
                    var temp = i % SubLength;
                    var previosCoord = i / SubLength;
                    var nextCoord = previosCoord + 1;
                    GameMap.TryGetTileAtCoord(listDefaultCoordinate[previosCoord], out var tilePreviosCoord);
                    GameMap.TryGetTileAtCoord(listDefaultCoordinate[nextCoord], out var tileNextCoord);
                    Segments[i].Coordinate = coordinate;
                    targetPos = Vector3.Lerp(tilePreviosCoord.transform.position, tileNextCoord.transform.position, (float)temp / SubLength);
                }

                Segments[i].transform.DOMove(targetPos, 0.05f).SetEase(Ease.Linear);
            }
            yield return new WaitForSeconds(0.05f);
        }

        private void ReverseSubSegments()
        {
            UpStraightSubSegments();
            var listCoordInverse = Segments.Select(x => x.Coordinate).Reverse().ToList();
            for (int i = 0; i < Segments.Count; i++)
            {
                if (i % SubLength != 0)
                {
                    Segments[i].Coordinate = listCoordInverse[i];
                }
            }
        }

        private void UpStraightSubSegments()
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                var listDefaultCoordinate = BodyData.listCoordinate;

                int unitIndex = i / subLength;

                var coordinate = listDefaultCoordinate[Mathf.Clamp(unitIndex, 0, listDefaultCoordinate.Count - 1)];

                if (i % SubLength == 0)
                {
                    if (GameMap.TryGetTileAtCoord(coordinate, out var tile))
                    {
                        Segments[i].Coordinate = coordinate;
                    }
                }
                else
                {
                    Segments[i].Coordinate = coordinate;
                }
            }
        }

        public void AddCoordinate(Vector2Int coord, ControlAnchor cAnchor)
        {
            if (BodyData.listCoordinate.Contains(coord)) return;


            Vector2Int removedCoord = Vector2Int.one * -1; 
            if (cAnchor == ControlAnchor.Head && IsAdjacent(coord, BodyData.listCoordinate[0]))
            {
                BodyData.listCoordinate.Insert(0, coord);
                removedCoord = BodyData.listCoordinate[^1];
                BodyData.listCoordinate.Remove(removedCoord);
            }
            else if(cAnchor == ControlAnchor.Tail && IsAdjacent(coord, BodyData.listCoordinate[^1]))
            {
                BodyData.listCoordinate.Add(coord);
                removedCoord = BodyData.listCoordinate[0];
                BodyData.listCoordinate.Remove(removedCoord);
            }

            if(GameMap.TryGetTileAtCoord(removedCoord, out var tile))
            {
                tile.SetOccupied(false);
                tile.RemoveOccupant();

                if(GameMap.TryGetTileAtCoord(coord, out var newTile))
                {
                    newTile.SetOccupied(true);
                    newTile.AddOccupant(true);
                }
            }
            
        }
            
        bool IsAdjacent(Vector2Int coord1, Vector2Int coord2)
        {
            var dirs = new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
            foreach(var dir in dirs)
            {
                if(coord1 + dir == coord2) return true;
            }
            return false;
        }

    }
}
