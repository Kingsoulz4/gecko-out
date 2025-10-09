using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public partial class BodyController 
    {
        private Coroutine moveShortDistanceCoroutine;

        public void MoveShortDistance(float amplitude, Vector3 direction, bool moveForward = true)
        {
            if (moveShortDistanceCoroutine != null)
            {
                StopCoroutine(moveShortDistanceCoroutine);
                moveShortDistanceCoroutine = null;
            }
            moveShortDistanceCoroutine = StartCoroutine(IEMoveShortDistance(amplitude, direction, moveForward));
        }

        IEnumerator IEMoveShortDistance(float amplitude, Vector3 direction, bool moveForward = true)
        {
            List<Vector3> dirs = new List<Vector3>() { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            if (moveForward)
            {
                Debug.Log($"Init Dir point short distance: {direction}");
                var lastPartPoint = Segments[0].transform.position;
                //lastPartPoint = new Vector3(lastPartPoint.x, lastPartPoint.y, lastPartPoint.z);
                Segments[0].transform.position += amplitude * direction;

                for (int i = 1; i < Segments.Count; i++)
                {
                    var dir = (lastPartPoint - Segments[i].transform.position).normalized;
                    if (!dirs.Contains(dir))
                    {
                        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                        {
                            dir = new Vector3(dir.x, 0, 0);
                        }
                        else
                        {
                            dir = new Vector3(0, dir.y, 0);
                        }
                    }
                    Debug.Log($"Dir point short distance: {dir}");
                    lastPartPoint = Segments[i].transform.position;
                    //lastPartPoint = new Vector3(lastPartPoint.x, lastPartPoint.y, lastPartPoint.z);
                    Segments[i].transform.position += dir * amplitude;
                }
                yield return new WaitForSeconds(0.1f);
            }

            occupiedTileController?.UpdateAllSegmentPositions();

        }

    }
}
