using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics;

namespace AI
{
    public class NavGraph : MonoBehaviour
    {
        public NavNode[,] navGrid;              // 2x2 array of nodes - nav graph
        public Vector2 gridSize;                // full size of the nav graph
        public float nodeRadius;                // radius of each node
        public float nodeDiameter;              // size of each node
        
        private float gridSizeX;                // width of the grid - used to find each node in relation to a character
        private float gridSizeY;                // height of the grid

        private AStarPathfind APathfinding;     // object used to pathfind using A* algorithm

        public List<Vector2> path;

        public GameObject[] boxList;

        public void InitGraph()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = (gridSize.x / nodeDiameter);
            gridSizeY = (gridSize.y / nodeDiameter);

            boxList = GameObject.FindGameObjectsWithTag("Ground");


            CreateGrid();
            CheckForUnwalkableNodes();

            APathfinding = new AStarPathfind();
        }


        public List<Vector2> GeneratePath(NavGraph grid, NavNode start, NavNode goal)
        {
            List<Vector2> path = APathfinding.AStarSearch(grid, start, goal);
            if(path == null)
            {
                return null;
            }
            return path;
        }


        void CreateGrid()
        {
            // Generate a new nav graph
            navGrid = new NavNode[(int)gridSizeX, (int)gridSizeY];

            // get the very bottom left of the nav graph in relation to the world position in the scene
            Vector2 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.up * gridSize.y / 2;

            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    // for each node, get its world position in the scene
                    Vector2 worldPoint = worldBottomLeft + Vector2.right * (i * nodeDiameter + nodeRadius) + Vector2.up * (j * nodeDiameter + nodeRadius);
                    // set each node
                    navGrid[i, j] = new NavNode(true, worldPoint, new Vector2(i, j));
                }
            }
        }


        void CheckForUnwalkableNodes()
        {

            for(int i = 5; i < 23; i++)
            {
                navGrid[i, 0].walkable = false;
                navGrid[i, 1].walkable = false;
            }

            ManualSetFirstAndTopRows();

            navGrid[12, 8].walkable = false;
            navGrid[13, 8].walkable = false;
            navGrid[14, 8].walkable = false;
            navGrid[15, 8].walkable = false;


        }

        void ManualSetFirstAndTopRows()
        {
            navGrid[6, 4].walkable = false;
            navGrid[7, 4].walkable = false;
            navGrid[8, 4].walkable = false;
            navGrid[9, 4].walkable = false;
            navGrid[10, 4].walkable = false;
            navGrid[17, 4].walkable = false;
            navGrid[18, 4].walkable = false;
            navGrid[19, 4].walkable = false;
            navGrid[20, 4].walkable = false;
            navGrid[21, 4].walkable = false;

            navGrid[6, 12].walkable = false;
            navGrid[7, 12].walkable = false;
            navGrid[8, 12].walkable = false;
            navGrid[9, 12].walkable = false;
            navGrid[10, 12].walkable = false;
            navGrid[17, 12].walkable = false;
            navGrid[18, 12].walkable = false;
            navGrid[19, 12].walkable = false;
            navGrid[20, 12].walkable = false;
            navGrid[21, 12].walkable = false;

        }

        public NavNode NodeFromWorldPoint(Vector2 worldPos)
        {
            // retrieve a specific node by getting its world position
            float percentX = (worldPos.x + gridSize.x / 2) / gridSize.x;
            float percentY = (worldPos.y + gridSize.y / 2) / gridSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

            return navGrid[x, y];
        }


        public List<NavNode> GetNeighbours(NavNode node)
        {
            // get a list of all adjacent neighbours for a specific node
            List<NavNode> neighbours = new List<NavNode>();

            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    // dont return its own node
                    if(x == 0 && y == 0)
                    {
                        continue;
                    }

                    float checkX = (int)node.gridPosition.x + x;
                    float checkY = (int)node.gridPosition.y + y;

                    // if the position we are checking is not out of bounds or the node we are checking
                    if(checkX >= 0 && checkX < gridSizeX && checkY >=0 && checkY < gridSizeY)
                    {
                        neighbours.Add(navGrid[(int)checkX, (int)checkY]);
                    }
                }
            }

            return neighbours;
        }
    }
}

