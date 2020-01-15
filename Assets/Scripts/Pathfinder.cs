using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class to solve anything related to pathfinding.
public class Pathfinder : MonoBehaviour
{
    //TODO: Optimize. use hash sets and dicts to speed up and so no need to ResetCalStats() everytime before using
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
    public static List<T> Solve<T>(List<T> map, T begin, T destination, Func<T, T, float> distance, Func<T, T, float> heuristic) where T : IPathfinderNode
    {
        //Nothing to solve if null, they are the same, or have a direct connection.
        if (begin == null || destination == null)
        {
            //Debug.LogWarning("Cannot find path for invalid nodes");
            return null;
        }
        if (object.ReferenceEquals(begin, destination) || begin.connections.Contains(destination))
        {
            //Debug.LogFormat("Direct Connection: {0} <-> {1} found", begin, destination);
            return new List<T> { begin, destination };
        }

        List<T> result = null;
        List<T> toBeTested = new List<T>();

        #region A* pathfinding
        // Set all the state to its starting values
        for (int i = 0; i < map.Count; i++)
        {
            map[i].ResetCalStats();
        }

        // Setup the start node to be zero away from start and estimate distance to target
        T currentNode = begin;
        currentNode.local = 0.0f;
        currentNode.global = heuristic(begin, destination);
        //
        // Maintain a list of nodes to be tested and begin with the start node, keep going
        // as long as we still have nodes to test and we haven't reached the destination
        toBeTested.Add(currentNode);

        while (toBeTested.Count > 0 && !object.ReferenceEquals(currentNode, destination))
        {
            // Begin by sorting the list each time by the heuristic
            toBeTested.Sort((a, b) => (int)(a.global - b.global));

            // Remove any tiles that have already been visited
            toBeTested.RemoveAll(n => n.visited);

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
                    T neighbour = (T)currentNode.connections[count];

                    if (!neighbour.visited && neighbour.isAccessible)
                    {
                        toBeTested.Add(neighbour);
                    }

                    // Calculate the local goal of this location from our current location and 
                    // test if it is lower than the local goal it currently holds, if so then
                    // we can update it to be owned by the current node instead 
                    float possibleLocalGoal = currentNode.local + distance(currentNode, neighbour);
                    if (possibleLocalGoal < neighbour.local)
                    {
                        neighbour.parent = currentNode;
                        neighbour.local = possibleLocalGoal;
                        neighbour.global = neighbour.local + heuristic(neighbour, destination);
                    }
                }
            }
        }

        // Build path if we found one, by checking if the destination was visited, if so then 
        // we have a solution, trace it back through the parents and return the reverse route
        if (destination.visited)
        {
            result = new List<T>();
            T routeNode = destination;

            while (routeNode.parent != null)
            {
                result.Add(routeNode);
                routeNode = (T)routeNode.parent;
            }
            result.Add(routeNode);
            result.Reverse();

            //Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.local);
        }
        else Debug.LogWarning("Path Not Found");

        #endregion 

        return result;
    }

    public static HashSet<T> IsInRange<T>(List<T> map, T begin, float range, Func<T, T, float> rangeFunc) where T : IPathfinderNode
    {
        HashSet<T> result = new HashSet<T>(); //{ begin };
        if (range <= 0) return result;

        // Set all the state to its starting values
        for (int i = 0; i < map.Count; i++)
        {
            map[i].ResetCalStats();
        }
        begin.local = 0;
        List<T> toBeChecked = new List<T>() { begin };
        HashSet<T> checkingHash = new HashSet<T>() { begin };
        T current, connection;
        float calculatedValue;
        while (toBeChecked.Count > 0)
        {
            current = toBeChecked[0];
            for (int c = 0; c < current.connections.Count; c++)
            {
                connection = (T)current.connections[c];
                if (!connection.isAccessible) continue;
                calculatedValue = rangeFunc(current, connection) + current.local;
                if (calculatedValue < connection.local)
                {
                    connection.local = calculatedValue;
                    if (current.connections[c].local < range)
                    {
                        if (!checkingHash.Contains(connection))//prevent redudant calculations
                        {
                            result.Add(connection);
                            toBeChecked.Add(connection);
                            checkingHash.Add(connection);
                        }
                    }
                }
            }
            checkingHash.Remove(current);
            toBeChecked.RemoveAt(0);
        }
        return result;
    }
}