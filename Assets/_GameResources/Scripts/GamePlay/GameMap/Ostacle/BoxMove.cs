using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    

    public class BoxMove : MonoBehaviour
    {
        [SerializeField] private LayerMask moveBoxLayer;
        private WayDirection wayDirection = WayDirection.Horizontal;

        public void Init()
        {

        }

        private void Move()
        {
            // check way direction
            switch (wayDirection)
            {
                case WayDirection.Horizontal:
                    // move left or right
                    break;
                case WayDirection.Vertical:
                    // move up or down
                    break;
                case WayDirection.All:
                    // move in any direction
                    break;
                default:
                    break;
            }
        }
    }
}
