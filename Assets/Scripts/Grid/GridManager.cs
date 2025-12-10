

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAI.Day02_AStar.Grid
{
    public class GridManager : MonoBehaviour //Both unity navmesh and this gives a walkable surface, obstacle avoidance, path a series of points, movement along a path,
                                             //Navmesh gives a polygon mesh that is more smooth and continious as to the tile approach, and it is automatic from geometry.
                                             //Navmesh is also more optimized, but it is harder to change or modify as it is a black box we can use in projects.

                                             //Unity navmesh for a continious world with smooth paths, better performance and less effort.
                                             //A* griud when it is tile based, per tile rules, visualize/rebuild easily, map changes often and more control over algorithm.

                                             //For a large map too many a* calls will not work perforamnce wise. With huge grids means thousands of nodes constantly closed and opened.
                                             //Many repeated cycles that are nearly identical
                                             //If a new wall is placed all units must recalculate path
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 1f;

        [Header("Prefabs & Materials")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Material walkableMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material pathMaterial;
        private Node[,] nodes;
        private Dictionary<GameObject, Node> tileToNode = new();

        private Node startNode;
        private Node goalNode;

        [SerializeField] private Material startMaterial;
        [SerializeField] private Material goalMaterial;


        // Input action for click 
        private InputAction clickAction;
        private InputAction rightClickAction;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;


        private void Awake()
        {
            GenerateGrid();
        }

        private void OnEnable()
        {
            clickAction = new InputAction(
                name: "Click",
                type: InputActionType.Button,
                binding: "<Mouse>/leftButton"
            );
            clickAction.performed += OnClickPerformed;
            clickAction.Enable();

            rightClickAction = new InputAction(name: "RightClick",
                type: InputActionType.Button,
                binding: "<Mouse>/rightButton"
                );
            rightClickAction.performed += OnRightClickPerformed;
            rightClickAction.Enable();
        }

        private void OnDisable()
        {
            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }

            if(rightClickAction != null)
            {
                rightClickAction.performed -= OnRightClickPerformed;
                rightClickAction.Disable();
            }
        }

        private void GenerateGrid()
        {
            nodes = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPos = new Vector3(x * cellSize, 0f,
                    y * cellSize);
                    GameObject tileGO = Instantiate(tilePrefab,
                    worldPos, Quaternion.identity, transform);
                    tileGO.name = $"Tile_{x}_{y}";

                    Node node = new Node(x, y, true, tileGO);
                    nodes[x, y] = node;
                    tileToNode[tileGO] = node;

                    SetTileMaterial(node, walkableMaterial);
                }
            }
        }

        private void OnClickPerformed(InputAction.CallbackContext
        ctx)
        {
            HandleMouseClick();
        }

        private void OnRightClickPerformed(InputAction.CallbackContext ctx)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                if(tileToNode.TryGetValue(clicked, out Node node))
                {
                    SelectStartOrGoal(node);

                    if(startNode != null && goalNode != null)
                    {
                        RunAStar();
                    }
                }
            }

        }

        private void SelectStartOrGoal(Node node)
        {
            if (!node.walkable)
                return;

            if(startNode == null)
            {
                startNode = node;
                SetTileMaterial(node, startMaterial);
                return;
            }

            if(goalNode == null)
            {
                goalNode = node;
                SetTileMaterial(node, goalMaterial);
                return;
            }

            ResetStartAndGoal();
            startNode = node;
            SetTileMaterial(node, startMaterial);
        }

        private void ResetStartAndGoal()
        {
            if(startNode != null)
            {
                SetTileMaterial(startNode, walkableMaterial);
            }
            if(goalNode != null )
            {
                SetTileMaterial(goalNode, walkableMaterial);

                startNode = null;
                goalNode = null;
            }
            ResetTileMaterials();
        }

        private void ResetTileMaterials()
        {
            foreach(Node node in nodes)
            {
                SetTileMaterial(node, node.walkable ? walkableMaterial : wallMaterial);
            }
        }


        private void HandleMouseClick()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray =
            cam.ScreenPointToRay(Mouse.current.position.ReadValue()); //Converts screen position to 3D Ray
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                Debug.Log(hit.collider.gameObject.name);
                if (tileToNode.TryGetValue(clicked, out Node node))
                {
                    bool newWalkable = !node.walkable;
                    SetWalkable(node, newWalkable);
                }
            }
        }

        public Node GetNode(int x, int y) //In this grid a node is a single cell that is either walkable or not, an edge is a connection or possible movement between them.
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            return nodes[x, y]; //If i tried to access a ndoe outside of the array an OutOfRangeException, the if statement above prevents that from happening as it checks heigth and width against the array.
        }


        public Node GetNodeFromWorldPosition(Vector3 worldPos) //Scaling grid coordinates into world coordniates. X coord of grid = world x position, y coord = world z
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.z / cellSize);
            return GetNode(x, y);
        }

        public IEnumerable<Node> GetNeighbours(Node node, bool
        allowDiagonals = false)
        {
            int x = node.x;
            int y = node.y;
            // 4-neighbour 
            yield return GetNode(x + 1, y);
            yield return GetNode(x - 1, y);
            yield return GetNode(x, y + 1);
            yield return GetNode(x, y - 1);

            if (allowDiagonals)
            {
                yield return GetNode(x + 1, y + 1);
                yield return GetNode(x - 1, y + 1);
                yield return GetNode(x + 1, y - 1);
                yield return GetNode(x - 1, y - 1);
            }
        }

        public void SetWalkable(Node node, bool walkable) //Turning a tile into a wall means making it non-walkable removing all edeges connecting it to neighbours. Basically removing it from the graph.
        {
            node.walkable = walkable;
            SetTileMaterial(node, walkable ? walkableMaterial :
            wallMaterial);
        }

        private void SetTileMaterial(Node node, Material mat) //Good for seeing which tiles the algorithm can traverse and which are blocked
        {
            var renderer = node.tile.GetComponent<MeshRenderer>();
            if (renderer != null && mat != null)
            {
                renderer.material = mat;
            }
        }

        private float GetHeuristic(Node first, Node second)
        {
            return(Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y));
        }


        public List<Node> FindPath(Node startNode, Node goalNode)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Node node = nodes[x, y];
                    node.gCost = float.PositiveInfinity;
                    node.hCost = 0f;
                    node.parent = null;
                }
            }

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.gCost = 0;
            startNode.hCost = GetHeuristic(startNode, goalNode);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node current = openSet[0];

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < current.fCost || (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    {
                        current = openSet[i];
                    }
                }

                if (current == goalNode)
                {
                    return RebuildPath(goalNode);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (Node neighbour in GetNeighbours(current))
                {
                    if (neighbour == null || !neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    float stepCost = 1f;
                    float tempG = current.gCost + stepCost; //g(n) calculation

                    if (tempG < neighbour.gCost) //If the new is lower it means we found a shorter apth and should update
                    {
                        neighbour.parent = current;
                        neighbour.gCost = tempG;
                        neighbour.hCost = GetHeuristic(neighbour, goalNode); //h(n) calculated here
                                                                            //If we use heuristic overestimates it becomes inadmissable as it breaks astar often.
                                                                           //If h(n) = 0 for all nodes it would be dijkstra's algorithm
                                                                           //If g(n) is ignored the goal direction is rushed as it doesn't care about path cost, can find inefficient paths.

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        public void RunAStar()
        {
            if (startNode == null || goalMaterial == null)
                return;

            ResetTileMaterials();
            List<Node> path = FindPath(startNode, goalNode);
            if(path == null) return;

            foreach(Node node in path)
            {
                SetTileMaterial(node, pathMaterial);
            }
        }


        private List<Node> RebuildPath(Node finalNode)
        {
            List<Node> path = new List<Node>();
            Node current = finalNode;

            while (current != null)
            {
                path.Add(current);
                current = current.parent;
            }

            path.Reverse();
            return path;
        }

    }
}
