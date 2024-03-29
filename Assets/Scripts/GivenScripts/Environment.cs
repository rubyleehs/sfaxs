﻿/* using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private List<GameObject> AccessibleTiles; //these are tils to be used when generating
    [SerializeField] private List<GameObject> InaccessibleTiles;
    [SerializeField] private Vector2Int Size;
    [SerializeField] private float AccessiblePercentage;

    private EnvironmentNode[][] mMap;
    private List<EnvironmentNode> mAll; //stores all tiles
    private List<EnvironmentNode> mLastSolution; //Only used for debugging purposes

    private readonly Vector3 NodeSize = Vector3.one * 9.0f;
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    public EnvironmentNode Start { get; private set; }

    private void Awake()
    {
        mAll = new List<EnvironmentNode>();
    }

    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].position, mMap[x][y].connections[n].position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if (!mMap[x][y].isAccessible)
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if (mLastSolution != null && mLastSolution.Contains(mMap[x][y]))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].position, NodeSize);
                }
            }
        }
    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentNode[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3(-(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize));
        bool start = true;

        for (int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentNode[Size.y];
            for (int y = 0; y < Size.y; ++y)
            {
                bool isAccessible = start || Random.value < AccessiblePercentage;
                List<GameObject> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;
                GameObject prefab = tiles[Random.Range(0, tiles.Count)];
                EnvironmentNode tile = Instantiate(prefab, position, Quaternion.identity, transform).GetComponent<EnvironmentNode>();
                tile.position = new Vector3(position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.isAccessible = isAccessible;
                //tile.gameObject.name = string.Format("Tile({0},{1})", x, y);
                mMap[x][y] = tile;
                mAll.Add(tile);

                if (start)
                {
                    Start = tile;
                }

                position.z += TileSize;
                start = false;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentNode tile = mMap[x][y];
                tile.connections = new List<IPathfinderNode>();
                if (x > 0)
                {
                    tile.connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    tile.connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    tile.connections.Add(mMap[x][y + 1]);
                }
            }
        }
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public List<EnvironmentNode> Solve(EnvironmentNode begin, EnvironmentNode destination)
    {
        Debug.Log(begin + " | " + destination);
        mLastSolution = Pathfinder.Solve(mAll, begin, destination, (a, b) => Distance(a, b), (a, b) => Heuristic(a, b));
        return mLastSolution;
    }

    private float Distance(IPathfinderNode a, IPathfinderNode b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        IPathfinderNode directConnection = a.connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    private float Heuristic(IPathfinderNode a, IPathfinderNode b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.position, b.position);
    }

} */