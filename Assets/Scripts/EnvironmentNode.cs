using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    Water,
    Hill,
    Mountain,
    Trees,
    Boulder //Add as necessary    
}

public class EnvironmentNode : IPathfinderNode
{
    public List<IPathfinderNode> connections { get; set; }
    public Vector3 position { get; set; }
    public bool isAccessible { get; set; }
    public IPathfinderNode parent { get; set; } //used for A* parent node, should be moves to pathfinder
    public float global { get; set; } //stores Local + current distance from goal.
    public float local { get; set; } // stores distance traveled in A*
    public bool visited { get; set; } //Visited by A*

    public HashSet<TerrainType> terrain;

    public EnvironmentNode(Vector3 position, bool isAccessible = true)
    {
        this.position = position;
        this.isAccessible = isAccessible;
        connections = new List<IPathfinderNode>();
        terrain = new HashSet<TerrainType>();
    }

    public void ResetCalStats()
    {
        parent = null;
        global = float.MaxValue;
        local = float.MaxValue;
        visited = false;
    }
}