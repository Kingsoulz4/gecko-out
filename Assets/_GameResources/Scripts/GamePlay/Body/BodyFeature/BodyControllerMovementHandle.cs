using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BodyController 
    {
        private Coroutine moveShortDistanceCoroutine;

        public void MoveShortDistance(float amplitude, Vector2Int direction, Vector2Int newHeadCoord, bool moveForward = true)
        {
            if (moveShortDistanceCoroutine != null)
            {
                StopCoroutine(moveShortDistanceCoroutine);
                moveShortDistanceCoroutine = null;
            }

            if (newHeadCoord != Segments[0].Coordinate)
            {
                var oldHeadCoord = Segments[0].Coordinate;
                Segments[0].Coordinate = newHeadCoord;
                for (int i = 1; i < Segments.Count; i++)
                {
                    var currentCoord = Segments[i].Coordinate;
                    Segments[i].Coordinate = oldHeadCoord;
                    oldHeadCoord = currentCoord;
                }
            }

            moveShortDistanceCoroutine = StartCoroutine(IEMoveShortDistance(amplitude,new Vector3(direction.x, direction.y), moveForward));
        }

        IEnumerator IEMoveShortDistance(float amplitude, Vector3 direction, bool moveForward = true)
        {
            List<Vector3> dirs = new List<Vector3>() { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            if (moveForward)
            {
                Debug.Log($"Init Dir point short distance: {direction}");
                var lastPartPoint = Segments[0].transform.position;
                Segments[0].transform.position += amplitude * direction;

                for (int i = 1; i < Segments.Count; i++)
                {
                    var temp = (Segments[i - 1].Coordinate - Segments[i].Coordinate);
                    Vector3 dir = new Vector3(temp.x, temp.y);
                    Debug.Log($"Dir: {dir}");
                    if (dir == Vector3.zero)
                    {
                        Debug.Log("Dir Zero Here");
                        dir = Segments[i - 1].transform.position - Segments[i].transform.position;
                        if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                        {
                            dir = Vector3.right * (dir.x/ Mathf.Abs(dir.x));
                        }
                        else
                        {
                            dir = Vector3.up * (dir.y / Mathf.Abs(dir.y));
                        }
                            
                            
                        Debug.Log($"New Dir: {dir}");
                    }
                    Debug.Log($"Dir point short distance: {dir}");
                    lastPartPoint = new Vector3(lastPartPoint.x, lastPartPoint.y, lastPartPoint.z);
                    Segments[i].transform.position = lastPartPoint;
                }
                yield return new WaitForSeconds(0.1f);
            }

            occupiedTileController?.UpdateAllSegmentPositions();

        }

        //public void 

    }
}
