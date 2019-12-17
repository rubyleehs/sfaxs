using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathfinderNode
{
    List<IPathfinderNode> connections { get; set; }

    //Assuming all pathfindy nodes has a position
    Vector3 position { get; set; }
    bool isAccessible { get; set; }
    IPathfinderNode parent { get; set; } //used for A* parent node, should be moves to pathfinder
    float global { get; set; } //stores Local + current distance from goal.
    float local { get; set; }// stores distance traveled in A*
    bool visited { get; set; }//Visited by A*

    void ResetCalStats();
}
