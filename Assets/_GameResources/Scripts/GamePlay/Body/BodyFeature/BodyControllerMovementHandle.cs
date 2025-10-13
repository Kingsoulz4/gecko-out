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
            if (!CanControl) return;

            if (IsMoving) return;

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
                    if (nextTile.IsOccupied && !BodyData.listCoordinate.Contains(newHeadCoord))
                    {
                        return;
                    }
                }
            }

            bool moveForward = true;

            var directions = new List<Vector2Int>() { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up};

            if (controlAnchor == ControlAnchor.Head)
            {
                Vector2Int currentBackwardDir = BodyData.listCoordinate[1] - BodyData.listCoordinate[0];

                if (Segments[0].Coordinate != BodyData.listCoordinate[0])
                {
                    currentBackwardDir = BodyData.listCoordinate[0] - Segments[0].Coordinate; 
                }

                if(currentBackwardDir == Vector2Int.zero)
                {
                    var dir = Segments[1].transform.position - Segments[0].transform.position;
                    currentBackwardDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                        ? Vector2Int.right * (int)Mathf.Sign(dir.x)
                        : Vector2Int.up * (int)Mathf.Sign(dir.y);
  
                }

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

                if (Segments[^1].Coordinate != BodyData.listCoordinate[^1])
                {
                    currentBackwardDir = BodyData.listCoordinate[^1] - Segments[^1].Coordinate;
                }

                if (currentBackwardDir == Vector2Int.zero)
                {
                    var dir = Segments[^2].transform.position - Segments[^1].transform.position;
                    currentBackwardDir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                        ? Vector2Int.right * (int)Mathf.Sign(dir.x)
                        : Vector2Int.up * (int)Mathf.Sign(dir.y);

                }

                moveForward = currentBackwardDir != direction;


                if (moveForward)
                {
                    AddCoordinate(newHeadCoord, controlAnchor);
                }
                else if (!moveForward && newHeadCoord != BodyData.listCoordinate[^1])
                {
                    var head = BodyData.listCoordinate[0];
                    var afterHead = BodyData.listCoordinate[1];
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

            Debug.Log($"Move Short Distance X Here from head {controlAnchor == ControlAnchor.Head} move forward {moveForward}");

            moveShortDistanceCoroutine = StartCoroutine(IEMoveShortDistance(amplitude, newHeadCoord, direction, controlAnchor,moveForward));
        }

        IEnumerator IEMoveShortDistance(float amplitude, Vector2Int newHeadCoord, Vector2Int direct, ControlAnchor controlAnchor, bool moveForward = true)
        {
            if (IsMoving) yield break;
            //isMoving = true;

            bool isHead = controlAnchor == ControlAnchor.Head;
            bool isForward = moveForward;

            if ((lastDirectionShortMove != direct || lastControllerAnchor != controlAnchor))
            {
                //if ((isHead && Segments[0].Coordinate != newHeadCoord) || (!isHead && Segments[^1].Coordinate != newHeadCoord))
                if (lastDirectionShortMove != Vector2Int.zero)
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
            MoveAnchorSegment(amplitude, direction, isHead, isForward);

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

            //isMoving = false;

            occupiedTileController?.UpdateAllSegmentPositions();
        }

        private void MoveAnchorSegment(float amplitude, Vector3 direction, bool isHead, bool isForward)
        {
            int index = isHead ? 0 : Segments.Count - 1;
            int lastIndex = isHead ? Segments.Count - 1 : 0;
            
            Segments[index].transform.position += direction * amplitude;

            var directions = new List<Vector2Int>() { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };

            if (!isForward)
            {
                Vector2Int head = BodyData.listCoordinate[0];
                Vector2Int afterHead = BodyData.listCoordinate[1];
                if(!isHead)
                {
                    head = BodyData.listCoordinate[^1];
                    afterHead = BodyData.listCoordinate[^2];
                }

                var headDir = afterHead - head;

                if (GameMap.TryGetTileAtCoord(Segments[lastIndex].Coordinate + headDir, out var tile) && !tile.IsOccupied)
                {
                    Segments[lastIndex].transform.position += new Vector3(headDir.x, headDir.y) * amplitude;
                }
                else
                {
                    foreach(var dir in directions)
                    {
                        var newCoord = Segments[lastIndex].Coordinate + dir;
                        var newCoordX = (int)Mathf.Clamp(newCoord.x, 0, GameMap.MapSize.x - 1);
                        var newCoordY = (int)Mathf.Clamp(newCoord.y, 0, GameMap.MapSize.x - 1);
                        newCoord = new Vector2Int(newCoordX, newCoordY);
                        if (GameMap.TryGetTileAtCoord(newCoord, out var newTile) && !newTile.IsOccupied)
                        {
                            Segments[lastIndex].transform.position += new Vector3(dir.x, dir.y) * amplitude;
                            break;
                        }
                    }
                }

                
            }

            
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
            for (int i = Segments.Count - 2; i >= 1; i--)
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
            for (int i = 1; i < Segments.Count -1; i++)
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
            if (MoveToPortal.IsEnteringPortal)
                yield break;

            Debug.Log("Update All Segments Pos here");

            var listDefaultCoordinate = BodyData.listCoordinate;
            int coordCount = listDefaultCoordinate.Count;
            float moveTime = 0.08f;
            float elapsed = 0f;

            Vector3[] startPositions = new Vector3[Segments.Count];
            Vector3[] targetPositions = new Vector3[Segments.Count];

            for (int i = 0; i < Segments.Count; i++)
            {
                int unitIndex = i / SubLength;
                int remainder = i % SubLength;

                Vector3 targetPos = Segments[i].transform.position;
                Segments[i].Coordinate = listDefaultCoordinate[Mathf.Clamp(unitIndex, 0, coordCount - 1)];

                if (remainder == 0)
                {
                    if (unitIndex < coordCount && GameMap.TryGetTileAtCoord(listDefaultCoordinate[unitIndex], out var tile))
                    {
                        targetPos = tile.transform.position;
                    }
                }
                else
                {
                    int prevIndex = unitIndex;
                    int nextIndex = Mathf.Min(prevIndex + 1, coordCount - 1);

                    if (GameMap.TryGetTileAtCoord(listDefaultCoordinate[prevIndex], out var tilePrev) &&
                        GameMap.TryGetTileAtCoord(listDefaultCoordinate[nextIndex], out var tileNext))
                    {
                        float t = (float)remainder / SubLength;
                        targetPos = Vector3.Lerp(tilePrev.transform.position, tileNext.transform.position, t);
                    }
                }

                startPositions[i] = Segments[i].transform.position;
                targetPositions[i] = targetPos;
            }

            while (elapsed < moveTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveTime);

                for (int i = 0; i < Segments.Count; i++)
                {
                    Segments[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                }

                yield return null; 
            }

            for (int i = 0; i < Segments.Count; i++)
                Segments[i].transform.position = targetPositions[i];

            lastDirectionShortMove = Vector2Int.zero;
            occupiedTileController?.UpdateAllSegmentPositions();
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

        private IEnumerator IEMoveToPortal(Portal portal)
        {
            yield return IEUpdateSegmentsPos();

            if(controlAnchor == ControlAnchor.Head)
            {
                UpStraightSubSegments();
            }
            else
            {
                ReverseSubSegments();
            }

            var orderedSegments = GetOrderedSegments();
            GameMap.TryGetTileAtCoord(orderedSegments[0].Coordinate, out var tileNearPortal);
            var deltaTimeEachMove = 0.05f;
            
            for(int i=0; i<orderedSegments.Count; i++)
            {
                var segment = orderedSegments[i];
                bool isLast = i == orderedSegments.Count - 1;
                var coord = orderedSegments[i].Coordinate;
                var tweenMove = DOTween.Sequence();
                for(int j=i-1; j>= 0; j--)
                {
                    if (GameMap.TryGetTileAtCoord(orderedSegments[j].Coordinate, out var tile) && Vector3.Distance(orderedSegments[i].transform.position, tile.transform.position) >= 0.5f)
                    {
                        tweenMove.Append(orderedSegments[i].transform.DOMove(tile.transform.position, deltaTimeEachMove).SetEase(Ease.Linear));
                        
                    }
                }

                tweenMove.Append(orderedSegments[i].transform.DOMove(portal.transform.position, deltaTimeEachMove).SetEase(Ease.Linear));
                tweenMove.OnComplete(() =>
                {
                    StartCoroutine(MoveToPortal.AnimateSegmentDown(segment, portal.transform.position, isLast));
                });
            }

            yield return new WaitForSeconds(deltaTimeEachMove * BodyData.listCoordinate.Count);
        }

    }
}
