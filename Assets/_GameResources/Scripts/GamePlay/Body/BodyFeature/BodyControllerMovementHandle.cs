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

        private ControlAnchor lastControllerAnchor;

        public void MoveShortDistance(float amplitude, Vector2Int direction, Vector2Int newHeadCoord, ControlAnchor controlAnchor)
        {
            if (moveShortDistanceCoroutine != null)
            {
                StopCoroutine(moveShortDistanceCoroutine);
                moveShortDistanceCoroutine = null;
            }
            if ((controlAnchor == ControlAnchor.Head && newHeadCoord != BodyData.listCoordinate[0]) 
                || (controlAnchor == ControlAnchor.Tail && newHeadCoord != BodyData.listCoordinate[^1]))
            {
                if (GameMap.TryGetTileAtCoord(newHeadCoord, out var nextTile))
                {
                    //if(nextTile.MapTileData.type != Data.MapTileType.Normal && nextTile.MapTileData.type != Data.MapTileType.Portal)

                    if (nextTile.IsOccupied)
                    {
                        return;
                    }
                }
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

            moveShortDistanceCoroutine = StartCoroutine(IEMoveShortDistance(amplitude, newHeadCoord, direction, controlAnchor,moveForward));
        }

        IEnumerator IEMoveShortDistance(float amplitude, Vector2Int newHeadCoord, Vector2Int direct, ControlAnchor controlAnchor, bool moveForward = true)
        {
            bool isHead = controlAnchor == ControlAnchor.Head;
            bool isForward = moveForward;

            if ((lastDirectionShortMove != direct || lastControllerAnchor != controlAnchor))
            {
                //if ((isHead && Segments[0].Coordinate != newHeadCoord) || (!isHead && Segments[^1].Coordinate != newHeadCoord))
                {
                    yield return IEUpdateSegmentsPos();
                }
                lastDirectionShortMove = direct;
                lastControllerAnchor = controlAnchor;
                if (isHead)
                {
                    if (isForward) UpStraightSubSegments();
                    else ReverseSubSegments();
                }
                else
                {
                    if (isForward) ReverseSubSegments();
                    else UpStraightSubSegments();
                }
            }

            Vector3 direction = new Vector3(direct.x, direct.y);
            Debug.Log($"Init Dir point short distance: {direction}");


            // 1️⃣ Move the anchor segment (head or tail)
            MoveAnchorSegment(amplitude, direction, isHead);

            // 3️⃣ Process movement propagation
            if (isHead)
            {
                if (isForward)
                    yield return MoveSegmentsFromHeadForward(amplitude, direction);
                else
                    yield return MoveSegmentsFromHeadBackward(amplitude, direction);
            }
            else
            {
                if (isForward)
                    yield return MoveSegmentsFromTailForward(amplitude, direction);
                else
                    yield return MoveSegmentsFromTailBackward(amplitude, direction);
            }

            yield return new WaitForSeconds(0.01f);
            occupiedTileController?.UpdateAllSegmentPositions();
        }

        private void MoveAnchorSegment(float amplitude, Vector3 direction, bool isHead)
        {
            int index = isHead ? 0 : Segments.Count - 1;
            Segments[index].transform.position = Vector3.Lerp(Segments[index].transform.position, Segments[index].transform.position + direction * amplitude, 50f * Time.deltaTime);
        }

        private IEnumerator MoveSegmentsFromHeadForward(float amplitude, Vector3 direction)
        {
            Debug.Log("Move Short Forward Head");
            for (int i = 1; i < Segments.Count; i++)
                MoveSegmentWithAmplitude(i, -1, amplitude, direction);
            yield break;
        }

        private IEnumerator MoveSegmentsFromHeadBackward(float amplitude, Vector3 direction)
        {
            Debug.Log("Move Short Backward Head");
            for (int i = Segments.Count - 2; i >= 0; i--)
                MoveSegmentWithAmplitude(i, +1, amplitude, direction);
            yield break;
        }

        private IEnumerator MoveSegmentsFromTailForward(float amplitude, Vector3 direction)
        {
            Debug.Log("Move Short Forward Tail");
            for (int i = Segments.Count - 2; i >= 0; i--)
                MoveSegmentWithAmplitude(i, +1, amplitude, direction);
            yield break;
        }

        private IEnumerator MoveSegmentsFromTailBackward(float amplitude, Vector3 direction)
        {
            Debug.Log("Move Short Backward Tail");
            for (int i = 1; i < Segments.Count; i++)
                MoveSegmentWithAmplitude(i, -1, amplitude, direction);
            yield break;
        }

        private void MoveSegmentWithAmplitude(int i, int coordOffset, float amplitude, Vector3 direction)
        {
            var segment = Segments[i];
            var currentCoord = segment.Coordinate;

            int indexOfCoord = BodyData.listCoordinate.IndexOf(currentCoord);
            int nextIndex = Mathf.Clamp(indexOfCoord + coordOffset, 0, BodyData.listCoordinate.Count - 1);
            var nextCoord = BodyData.listCoordinate[nextIndex];

            if(nextCoord == currentCoord)
            {
                Debug.Log("Current Coord Same");
                var nextCoordX = (int)Mathf.Clamp(currentCoord.x + direction.x,0, GameMap.MapSize.x -1);
                var nextCoordY = (int)Mathf.Clamp(currentCoord.y + direction.y, 0, GameMap.MapSize.y -1);
                nextCoord = new Vector2Int(nextCoordX, nextCoordY);
                Debug.Log($"New Coord {nextCoord}");
            }    

            if (GameMap.TryGetTileAtCoord(segment.Coordinate, out var tile))
            {
                if (Vector3.Distance(segment.transform.position, tile.transform.position) <= 0.05f)
                {
                    segment.transform.position = tile.transform.position;
                    segment.Coordinate = nextCoord;
                    Debug.Log($"Changed To New Coord {nextCoord}");
                }
            }

            if (GameMap.TryGetTileAtCoord(segment.Coordinate, out tile))
            {
                var dir = tile.transform.position - segment.transform.position;
                if (dir != Vector3.zero)
                {
                    dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                        ? Vector3.right * Mathf.Sign(dir.x)
                        : Vector3.up * Mathf.Sign(dir.y);

                    //segment.transform.position = Vector3.Lerp(segment.transform.position, segment.transform.position + dir * amplitude, 50f* Time.deltaTime);
                    segment.transform.position += dir * amplitude;

                }
            }
        }


        public void UpdateAllSegmentPos()
        {
            if (MoveToPortal.IsEnteringPortal) return;

            if (coroutineUpdateAllSegmentsPos != null)
            {
                StopCoroutine(coroutineUpdateAllSegmentsPos);
                coroutineUpdateAllSegmentsPos = null;
            }

            coroutineUpdateAllSegmentsPos = StartCoroutine(IEUpdateSegmentsPos());
        }

        private IEnumerator IEUpdateSegmentsPos()
        {
            if (MoveToPortal.IsEnteringPortal) yield break;

            Debug.Log("Update All Segments Pos here");
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

                Segments[i].transform.DOMove(targetPos, 0.08f).SetEase(Ease.Linear);
            }
            yield return new WaitForSeconds(0.06f);

            //AddAnchorSample(GetOrderedSegments()[0].transform.position);

            lastDirectionShortMove = Vector2Int.zero;
        }

        private void ReverseSubSegments()
        {
            UpStraightSubSegments();

            for (int i = Segments.Count -1; i >= 0; i--)
            {
                var listDefaultCoordinate = BodyData.listCoordinate;

                int unitIndex = Mathf.CeilToInt((float)i / subLength);

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
                Segments[0].Coordinate = coord;
            }
            else if(cAnchor == ControlAnchor.Tail && IsAdjacent(coord, BodyData.listCoordinate[^1]))
            {
                BodyData.listCoordinate.Add(coord);
                removedCoord = BodyData.listCoordinate[0];
                BodyData.listCoordinate.Remove(removedCoord);
                Segments[^1].Coordinate = coord;
            }

            if (GameMap.TryGetTileAtCoord(removedCoord, out var tile))
            {
                tile.SetOccupied(false);
                tile.RemoveOccupant();

                if (GameMap.TryGetTileAtCoord(coord, out var newTile))
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

        //private IEnumerator IEMoveToPortal()
        //{
        //    var orderedSegments = GetOrderedSegments();
        //    GameMap.TryGetTileAtCoord(orderedSegments[0].Coordinate, out var tileNearPortal);
        //    var pos
        //}

    }
}
