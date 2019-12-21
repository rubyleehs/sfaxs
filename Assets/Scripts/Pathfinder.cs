using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class to solve anything related to pathfinding.
public class Pathfinder : MonoBehaviour
{
    // Moved and slightly altered from Original Enviroment Class to remove unecessary intimacy with other enviromental stuff
    // and to allow it to be used for other purposes other than path finding within the enviroment
    /// <summary>
    /// Returns a path from a starting node to an ending node using A*
    /// </summary>
    /// <param name="map">Map of all nodes</param> 
    /// <param name="begin">Starting Node</param> 
    /// <param name="destination">Destination Node</param> 
    /// <param name="distance">Function to calculate effort necessary to tavel between 2 connected nodes</param>
    /// <param name="heuristic">Function to estimate effort necessary to travel between any 2 nodes</param> 
    /// <returns>Returns a path from a starting node to an ending node using A*</returns>
    public static List<IPathfinderNode> Solve<T> (List<T> map, IPathfinderNode begin, IPathfinderNode destination, Func<IPathfinderNode, IPathfinderNode, float> distance, Func<IPathfinderNode, IPathfinderNode, float> heuristic) where T : IPathfinderNode
    {
        //Nothing to solve if null, they are the same, or have a direct connection.
        if (begin == null || destination == null)
        {
            Debug.LogWarning ("Cannot find path for invalid nodes");
            return null;
        }
        if (begin == destination || begin.connections.Contains (destination))
        {
            Debug.LogFormat ("Direct Connection: {0} <-> {1} found", begin, destination);
            return new List<IPathfinderNode> { begin, destination };
        }

        List<IPathfinderNode> result = null;
        List<IPathfinderNode> toBeTested = new List<IPathfinderNode> ();

        #region A* pathfinding
        // Set all the state to its starting values
        for (int i = 0; i < map.Count; i++)
        {
            map[i].ResetCalStats ();
        }

        // Setup the start node to be zero away from start and estimate distance to target
        IPathfinderNode currentNode = begin;
        currentNode.local = 0.0f;
        currentNode.global = heuristic (begin, destination);

        // Maintain a list of nodes to be tested and begin with the start node, keep going
        // as long as we still have nodes to test and we haven't reached the destination
        toBeTested.Add (currentNode);

        while (toBeTested.Count > 0 && currentNode != destination)
        {
            // Begin by sorting the list each time by the heuristic
            toBeTested.Sort ((a, b) => (int) (a.global - b.global));

            // Remove any tiles that have already been visited
            toBeTested.RemoveAll (n => n.visited);

            // Check that we still have locations to visit
            if (toBeTested.Count > 0)
            {
                // Mark this note visited and then process it
                currentNode = toBeTested[0];
                currentNode.visited = true;

                // Check each neighbour, if it is accessible and hasn't already been 
                // processed then add it to the list to be tested 
                for (int count = 0; count < currentNode.connections.Count; ++count)
                {
                    IPathfinderNode neighbour = currentNode.connections[count];

                    if (!neighbour.visited && neighbour.isAccessible)
                    {
                        toBeTested.Add (neighbour);
                    }

                    // Calculate the local goal of this location from our current location and 
                    // test if it is lower than the local goal it currently holds, if so then
                    // we can update it to be owned by the current node instead 
                    float possibleLocalGoal = currentNode.local + distance (currentNode, neighbour);
                    if (possibleLocalGoal < neighbour.local)
                    {
                        neighbour.parent = currentNode;
                        neighbour.local = possibleLocalGoal;
                        neighbour.global = neighbour.local + heuristic (neighbour, destination);
                    }
                }
            }
        }

        // Build path if we found one, by checking if the destination was visited, if so then 
        // we have a solution, trace it back through the parents and return the reverse route
        if (destination.visited)
        {
            result = new List<IPathfinderNode> ();
            IPathfinderNode routeNode = destination;

            while (routeNode.parent != null)
            {
                result.Add (routeNode);
                routeNode = routeNode.parent;
            }
            result.Add (routeNode);
            result.Reverse ();

            Debug.LogFormat ("Path Found: {0} steps {1} long", result.Count, destination.local);
        }
        else Debug.LogWarning ("Path Not Found");

        #endregion 

        return result;
    }
}