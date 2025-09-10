using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Geckout.PathFinding {
    public class ASGrid
    {
        ASNode[,] nodes;

        int _width, _height;


        public ASGrid(int width, int height, bool[] cellStates)
        {
            _width = width;
            _height = height;
            nodes = new ASNode[_width, _height];
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    var index = i + j * _width;
                    nodes[i,j] = new ASNode( cellStates[index], new Vector2Int(i, j));
                }
            }
        }

        public List<ASNode> GetNeighBors(ASNode node)
        {
            List<ASNode> neighbors = new List<ASNode>();
            for (int x = -1; x <= 1; x++)
            {

                if (x == 0)
                {
                    continue;
                }
                int neighborsX = node.Position.x + x;
                int neighborsY = node.Position.y;
                if (neighborsX >= 0 && neighborsX <= _width - 1)
                {
                    neighbors.Add(nodes[neighborsX, neighborsY]);
                }

            }

            for (int y = -1; y <= 1; y++)
            {
                if (y == 0)
                {
                    continue;
                }
                int neighborsX = node.Position.x;
                int neighborsY = node.Position.y + y;
                if (neighborsY >= 0 && neighborsY <= _height - 1)
                {
                    neighbors.Add(nodes[neighborsX, neighborsY]);
                }
            }

            return neighbors;
        }

        public ASNode[,] GetNodes()
        {
            return nodes;
        }

        public int GridArea {
            get{
                return _width * _height;
            }
        }
    }
}