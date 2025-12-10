using System.Collections.Generic;
using UnityEngine;

namespace GameAI.Day02_AStar.Grid
{
    public class AgentMover : MonoBehaviour
    {
        public GridManager grid;
        public float moveSpeed = 3f;
        public float repathInterval = 0.5f; //Lower values gives smoother tracking and more respensive but at a higher cost. Could also involve the player or world having to change aka path is invalid.
        public Transform target;

        private List<Node> currentPath;
        private int currentIndex = 0;
        private float repathTimer = 0f;

        private void Update()
        {
            if (grid == null || target == null)
                return;


            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                ComputePathToTarget();
                repathTimer = repathInterval;
            }

            FollowPath();
        }

        private void ComputePathToTarget() //Computing a path using a star, moving along takes a lsit of nodes and moves towards each waypoint, purely movement.
        {
            Node startNode = grid.GetNodeFromWorldPosition(transform.position);
            Node goalNode = grid.GetNodeFromWorldPosition(target.position);

            if (startNode == null || goalNode == null)
                return;

            currentPath = grid.FindPath(startNode, goalNode);  //If the player is unreachable the enemy will not find a new path and just stay still.
            currentIndex = 0;
        }

        private void FollowPath()
        {
            if (currentPath == null || currentPath.Count == 0) //Agent wont crash or move into invalid places, but doesnt recalculate
            {
                return;

            }

            if (currentIndex >= currentPath.Count)
                return;

            Node targetNode = currentPath[currentIndex];
            Vector3 targetPos = NodeToWorldPosition(targetNode);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            float distance = Vector3.Distance(transform.position, targetPos);
            if(distance < 0.1f)
            {
                currentIndex++;
                if(currentIndex >= currentPath.Count)
                {
                    //Reached the end of the path
                }
            }

            
        }

        private Vector3 NodeToWorldPosition(Node node)
        {
            return new Vector3(node.x * grid.CellSize, transform.position.y, node.y * grid.CellSize);
        }
    }
}

