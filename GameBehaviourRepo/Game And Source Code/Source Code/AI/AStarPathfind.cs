using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics; 

namespace AI
{
    public class AStarPathfind : MonoBehaviour
    {

        public List<Vector2> AStarSearch(NavGraph grid, NavNode startPos, NavNode finalPos)
        {
            // Generate a path to the target by using the A* algorithm
            List<NavNode> nodePath = FindPath(grid, startPos, finalPos);
            List<Vector2> finalPath = new List<Vector2>();

            // generate a list of vector2 to be used by each enemy
            if(nodePath != null)
            {
                foreach(NavNode node in nodePath)
                {
                    finalPath.Add(new Vector2(node.worldPosition.x, node.worldPosition.y));
                }
            }

            return finalPath;
        }


        private List<NavNode> FindPath(NavGraph grid, NavNode startPos, NavNode finalPos)
        {
            NavNode startNode = startPos;
            NavNode finalNode = finalPos;

            List<NavNode> openList = new List<NavNode>();       // List of nodes to be checked
            List<NavNode> closedList = new List<NavNode>();     // list of nodes that have been checked

            openList.Add(startNode);                            // add our starting node

            if (openList.Count <= 0)
            {
                return null;
            }

            while(openList.Count > 0)
            {
                NavNode currentNode = openList[0];

                // check each node to make sure we have the one with the lowest cost
                for(int i = 0; i < openList.Count; i++)
                {
                    if(openList[i].fCost < currentNode.fCost || 
                        openList[i].fCost == currentNode.fCost && 
                        openList[i].hCost < currentNode.hCost)
                    {
                        currentNode = openList[i];
                    }
                }
                // add the node to the closed list and remove from open
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if(currentNode == finalNode)
                {
                    //retrace path if we are at the final node
                    return RetracePath(grid, startNode, finalNode);
                }

                // check each neighbour
                foreach(NavNode neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedList.Contains(neighbour))
                    {
                        //unwalkable or we already checked the node go the next
                        continue;
                    }

                    // get the distance to the current neighbour
                    int moveCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                    if(moveCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
                    {
                        // if the distance is shorter or its not in the open list
                        // set the gcost to the distance between current and neighbour nodes
                        neighbour.gCost = moveCostToNeighbour;
                        // set the hCost to the distance from neighbhour to the final node
                        neighbour.hCost = GetDistance(neighbour, finalNode);
                        // set the parent
                        neighbour.parent = currentNode;

                        // if neighbour not in open list, add it
                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            }
            // return null if there is no open list or not path can be made
            //Debug.Log("Unable to make path to target.");
            return null;
        }


        private List<NavNode> RetracePath(NavGraph grid, NavNode startNode, NavNode endNode)
        {
            // retrace the node path we made 
            List<NavNode> path = new List<NavNode>();
            NavNode currentNode = endNode;

            while(currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            // reverse it as it is the wrong way around
            path.Reverse();
            return path;
        }



        private int GetDistance(NavNode nodeA, NavNode nodeB)
        {
            // get the distance from one node to another
            Vector2 distance = new Vector2(Mathf.Abs(nodeA.worldPosition.x - nodeB.worldPosition.x),
                Mathf.Abs(nodeA.worldPosition.y - nodeB.worldPosition.y));

            if (distance.x > distance.y)
            {
                return 14 * (int)distance.y + 10 * ((int)distance.x - (int)distance.y);
            }

            return 14 * (int)distance.x + 10 * ((int)distance.y - (int)distance.x);

        }
    }
}

