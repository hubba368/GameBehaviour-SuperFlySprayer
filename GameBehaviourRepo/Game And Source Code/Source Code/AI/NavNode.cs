using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI
{
    public class NavNode
    {
        public bool walkable;           // bool to check if a node is pathable
        public Vector2 worldPosition;   // position of the node in the scene
        public Vector2 gridPosition;    // position of the node in the grid
        public int gCost;
        public int hCost;
        public NavNode parent;          // the current parent of the node

        public NavNode(bool w, Vector2 worldPos, Vector2 gridPos)
        {
            walkable = w;
            worldPosition = worldPos;
            gridPosition = gridPos;
        }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
    }

}
