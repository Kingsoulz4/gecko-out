using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public partial class BodyController 
    {
        private Coroutine moveShortDistanceCoroutine;

        public void MoveShortDistance(float amplitude, Vector2Int direction, Vector2Int newHeadCoord, ControlAnchor controlAnchor)
        {
            if (moveShortDistanceCoroutine != null)
            {
                StopCoroutine(moveShortDistanceCoroutine);
                moveShortDistanceCoroutine = null;
            }

            bool moveForward = true;

            if (controlAnchor == ControlAnchor.Head)
            {
                var currentBackwardDir = BodyData.listCoordinate[1] - BodyData.listCoordinate[0];
                moveForward = currentBackwardDir != direction;

                if (BodyData.listCoordinate.First() != newHeadCoord)
                {
                    if (moveForward)
                    {
                        BodyData.listCoordinate.Insert(0, newHeadCoord);
                        BodyData.listCoordinate.RemoveAt(BodyData.listCoordinate.Count - 1);
                        Segments[0].Coordinate = newHeadCoord;
                    }
                    else
                    {
                        BodyData.listCoordinate.Add(newHeadCoord);
                        BodyData.listCoordinate.RemoveAt(0);
                        Segments[0].Coordinate = BodyData.listCoordinate.First();
                    }
                }
            }
            else if(controlAnchor == ControlAnchor.Tail)
            {
                var currentBackwardDir = BodyData.listCoordinate[^2] - BodyData.listCoordinate[^1];
                moveForward = currentBackwardDir != direction;

                if (BodyData.listCoordinate.First() != newHeadCoord)
                {
                    if (moveForward)
                    {
                        BodyData.listCoordinate.Add(newHeadCoord);
                        BodyData.listCoordinate.RemoveAt(0);
                        Segments[^1].Coordinate = newHeadCoord;
                    }
                    else
                    {
                        BodyData.listCoordinate.Insert(0, newHeadCoord);
                        BodyData.listCoordinate.RemoveAt(BodyData.listCoordinate.Count - 1);
                        Segments[^1].Coordinate = BodyData.listCoordinate.Last();
                    }
                }
            }

            moveShortDistanceCoroutine = StartCoroutine(IEMoveShortDistance(amplitude,new Vector3(direction.x, direction.y), controlAnchor,moveForward));
        }

        IEnumerator IEMoveShortDistance(float amplitude, Vector3 direction, ControlAnchor controlAnchor, bool moveForward = true)
        {
            List<Vector3> dirs = new List<Vector3>() { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            Debug.Log($"Init Dir point short distance: {direction}");

            if (controlAnchor == ControlAnchor.Head)
            {
                if (moveForward)
                {
                    Segments[0].transform.position += direction * amplitude;
                }
                else
                {
                    Segments[^1].transform.position += direction * amplitude;
                }

            }
            else if (controlAnchor == ControlAnchor.Tail)
            {
                if (moveForward)
                {
                    Segments[0].transform.position += direction * amplitude;
                }
                else
                {
                    Segments[^1].transform.position += direction * amplitude;
                }
            }

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


            yield return new WaitForSeconds(0.01f);

            occupiedTileController?.UpdateAllSegmentPositions();

        }

        public void UpdateAllSegmentPos()
        {
            Debug.Log("Update All Segments Pos here");
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
                        Segments[i].transform.position = tile.transform.position;
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
                    Segments[i].transform.position = Vector3.Lerp(tilePreviosCoord.transform.position, tileNextCoord.transform.position, (float)temp / SubLength);
                }
            }
        }

        public void AddCoordinate(Vector2Int coord)
        {
            Vector2Int removedCoord; 
            if (controlAnchor == ControlAnchor.Head)
            {
                BodyData.listCoordinate.Insert(0, coord);
                removedCoord = BodyData.listCoordinate[^1];
                BodyData.listCoordinate.Remove(removedCoord);
            }
            else
            {
                BodyData.listCoordinate.Add(coord);
                removedCoord = BodyData.listCoordinate[0];
                BodyData.listCoordinate.Remove(removedCoord);
            }

            if(GameMap.TryGetTileAtCoord(removedCoord, out var tile))
            {
                tile.SetOccupied(false);
                tile.AddOccupant(true);
            }
        }
            

    }
}
