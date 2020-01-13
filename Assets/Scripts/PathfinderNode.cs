using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathfinderNode
{
    List<IPathfinderNode> connections { get; set; }

    //Assuming all pathfindy nodes has a position
    Vector3 position { get; set; } //position of node in world space
    Vector2Int indexPosition { get; set; } //index of node. can be used to simplyfy calculations instead of dealing with floats.
    float effortWeightage { get; set; }//used for path finding calculations.
    bool isAccessible { get; set; }
    IPathfinderNode parent { get; set; } //used for A* parent node
    float global { get; set; } //stores Local + current distance from goal.
    float local { get; set; }// stores distance traveled in A*
    bool visited { get; set; }//Visited by A*

    void ResetCalStats();
}
