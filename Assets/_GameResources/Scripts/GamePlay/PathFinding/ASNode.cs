using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Geckout.PathFinding
{
    public class ASNode: IHeapItem<ASNode>
    {
        public bool Walkable;
        public Vector2Int Position;

        public int gCost;
        public int hCost;

        public float movementPenalty;

        public ASNode parrent;

        int heapIndex;

        public ASNode(bool walkable, Vector2Int pos)
        {
            Walkable = walkable;
            Position = pos;
        }

        public int fCost {
            get
            {

                return gCost + hCost;
            }
        }

        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }

            set
            {
                heapIndex = value;
            }
        }

        public int CompareTo(ASNode other)
        {
            int compare = fCost.CompareTo(other.fCost);
            if (compare == 0) {
                compare = hCost.CompareTo(other.hCost);
            }
            return -compare;
        }
    }
}