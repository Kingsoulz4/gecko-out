using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
namespace Geckout.PathFinding
{
    public class ASPathFinding
    {
        ASGrid grid;

        public List<List<ASNode>> path = new List<List<ASNode>>();

        public ASPathFinding(ASGrid _grid)
        {
            grid = _grid;
        }

        public void RequestFindPath(Vector2Int[] startPoints, Vector2Int endPoint) {
            for (int i = 0; i < startPoints.Length; i++) {
                //Thread newThread = new Thread(()=> {
                   
                //});
                //newThread.Start();
                FindPath(startPoints[i], endPoint, (List<ASNode> _path) => {
                    path.Add(_path);
                });
            }
        }

        public void FindPath(Vector2Int startPos, Vector2Int endPos, Action<List<ASNode>> successCallback) {
            ASNode startNode = grid.GetNodes()[startPos.x, startPos.y];
            ASNode endNode = grid.GetNodes()[endPos.x, endPos.y];

            Heap<ASNode> openSet = new Heap<ASNode>(grid.GridArea);
            HashSet<ASNode> closedSet = new HashSet<ASNode>();

            openSet.Add(startNode);
            while (openSet.Count > 0) {
                ASNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);
                if (currentNode == endNode) {
                    successCallback(Retrace(startNode, endNode));
                    return;
                }

                List<ASNode> currentNodeNeighbors = grid.GetNeighBors(currentNode);
                foreach (var node in currentNodeNeighbors) {
                    if (!node.Walkable || closedSet.Contains(node)) {
                        continue;
                    }

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, node) /*+ (int)(node.movementPenalty * 255)*/;
                    if (newMovementCostToNeighbor < node.gCost || !openSet.Contains(node)) {
                        node.gCost = newMovementCostToNeighbor;
                        node.hCost = GetDistance(node, endNode);

                        node.parrent = currentNode;

                        if (!openSet.Contains(node)) {
                            openSet.Add(node);
                        }
                    }
                }  
            }
        }

        List<ASNode> Retrace(ASNode startNode, ASNode endNode) {
            List<ASNode> _path = new List<ASNode>();
            ASNode currentNode = endNode;

            while (currentNode != startNode) {
                _path.Add(currentNode);
                currentNode = currentNode.parrent;
            }

            _path.Reverse();
            return _path;
        }

        public void Reset() {
            path.Clear();
        }

        public int GetDistance(ASNode nodeA, ASNode nodeB) {
            int dstX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
            int dstY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
            if (dstX > dstY) {
                return 14 * dstY + 10 * (dstX - dstY);
            }
            return 14 * dstX + 10 * (dstY - dstX);

        }

    }
}
