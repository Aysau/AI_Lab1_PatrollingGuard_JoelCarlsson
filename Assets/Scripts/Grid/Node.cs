using UnityEngine;
namespace GameAI.Day02_AStar.Grid
{
    public enum TerrainType
    {
        Road,
        Mud,
        Water,
        Wall
    }

    public class Node
    {
        public int x;
        public int y;
        public bool walkable => terrain != TerrainType.Wall;
        public GameObject tile;
        public TerrainType terrain;

        public int movementCost => terrain switch { TerrainType.Road => 1, TerrainType.Mud => 3, TerrainType.Water => 5, TerrainType.Wall => int.MaxValue, _ => throw new System.NotImplementedException() }; //Road = 1, mud = 3, water = 5
        public float gCost;
        public float hCost;
        public Node parent;

        public float fCost => gCost + hCost; //F(n) calculation

        public Node(int x, int y, GameObject tile)
        {
            this.x = x;
            this.y = y;

            this.tile = tile;

            gCost = float.PositiveInfinity;
            hCost = 0f;
            parent = null;
        }
    }
}



