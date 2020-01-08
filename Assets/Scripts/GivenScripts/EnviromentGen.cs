using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnviromentGen : MonoBehaviour
{
    private EnvironmentTile[][] mMap;
    private List<EnvironmentTile> mAll;

    private const float tileSize = 1f;
    private const float tileHeight = 2.5f;

    [SerializeField] private Vector2Int mapSizeInRegions = new Vector2Int (10, 10);
    [SerializeField] private int cellsPerMapRegion = 5;
    [SerializeField] private Vector2Int trueMapSize;

    [SerializeField] private Vector2 globalHeightMinMax = new Vector2 (0, 150);
    [SerializeField] private Vector2 regionHeightMinMax = new Vector2 (0, 2);

    [SerializeField] private int heightMapAverageMaskRange = 3; //Decreases height variance in the micro scale.

    [SerializeField] private int regionMapAverageMaskRange = 1; //Decreases height variance in the macro scale. 

    private float[, ] globalHeightMap;
    private float[, ] regionHeightMap;

    public GameObject tempObject;

    public EnvironmentTile Start { get; private set; }

    private void Awake ()
    {
        mAll = new List<EnvironmentTile> ();
        Generate ();
    }

    private void Generate ()
    {
        trueMapSize = mapSizeInRegions * cellsPerMapRegion;
        regionHeightMap = IncreaseResolutionOf2DArray (AverageNearby (CreateRandom2DArray (mapSizeInRegions, regionHeightMinMax.x, regionHeightMinMax.y), regionMapAverageMaskRange), cellsPerMapRegion);
        globalHeightMap = CreateRandom2DArray (trueMapSize, globalHeightMinMax.x, globalHeightMinMax.y);
        for (int y = 0; y < trueMapSize.y; y++)
        {
            for (int x = 0; x < trueMapSize.x; x++)
            {
                globalHeightMap[x, y] *= regionHeightMap[x, y];
            }
        }
        globalHeightMap = AverageNearby (globalHeightMap, heightMapAverageMaskRange);
        globalHeightMap = AverageNearby (globalHeightMap, 1); //Average once more to smooth things out/prevent large diffrences.
        for (int y = 0; y < trueMapSize.y; y++)
        {
            for (int x = 0; x < trueMapSize.x; x++)
            {
                Instantiate (tempObject, new Vector3 (x * tileSize, globalHeightMap[x, y] * 0.1f, y * tileSize), Quaternion.identity);
            }
        }
    }

    private float[, ] IncreaseResolutionOf2DArray (float[, ] arr, int multiplier)
    {
        float[, ] result = new float[arr.GetLength (0) * multiplier, arr.GetLength (1) * multiplier];
        float val;
        for (int y = 0; y < arr.GetLength (1); y++)
        {
            for (int x = 0; x < arr.GetLength (0); x++)
            {
                val = arr[x, y];
                for (int dy = 0; dy < multiplier; dy++)
                {
                    for (int dx = 0; dx < multiplier; dx++)
                    {
                        result[x * multiplier + dx, y * multiplier + dy] = val;
                    }
                }
            }
        }
        return result;
    }

    private float[, ] CreateRandom2DArray (Vector2Int size, float min, float max)
    {
        float[, ] result = new float[size.x, size.y];
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                result[x, y] = Random.Range (min, max);
            }
        }
        return result;
    }

    private float[, ] AverageNearby (float[, ] arr, int range)
    {
        float[, ] result = new float[arr.GetLength (0), arr.GetLength (1)]; //
        int count;
        int cx, cy;
        for (int y = 0; y < arr.GetLength (1); y++)
        {
            for (int x = 0; x < arr.GetLength (0); x++)
            {
                count = 0;
                for (int dy = -range; dy <= range; dy++)
                {
                    for (int dx = -range; dx <= range; dx++)
                    {
                        cx = x + dx;
                        cy = y + dy;

                        if (cx < 0 || cy < 0 || cx >= arr.GetLength (0) || cy >= arr.GetLength (1)) continue;

                        result[x, y] += arr[cx, cy];
                        count++;
                    }
                }
                result[x, y] /= count;
            }
        }
        return result;
    }
    private void SetupConnections ()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < trueMapSize.x; ++x)
        {
            for (int y = 0; y < trueMapSize.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.connections = new List<IPathfinderNode> ();
                if (x > 0)
                {
                    tile.connections.Add (mMap[x - 1][y]);
                }

                if (x < trueMapSize.x - 1)
                {
                    tile.connections.Add (mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.connections.Add (mMap[x][y - 1]);
                }

                if (y < trueMapSize.y - 1)
                {
                    tile.connections.Add (mMap[x][y + 1]);
                }
            }
        }
    }

    public void GenerateWorld ()
    {
        Generate ();
        SetupConnections ();
    }

    public void CleanUpWorld ()
    {
        if (mMap != null)
        {
            for (int x = 0; x < trueMapSize.x; ++x)
            {
                for (int y = 0; y < trueMapSize.y; ++y)
                {
                    Destroy (mMap[x][y].gameObject);
                }
            }
        }
    }

    public List<EnvironmentTile> Solve (EnvironmentTile begin, EnvironmentTile destination)
    {
        Debug.Log (begin + " | " + destination);
        return Pathfinder.Solve (mAll, begin, destination, (a, b) => Distance (a, b), (a, b) => Heuristic (a, b)).Cast<EnvironmentTile> ().ToList ();
    }

    private float Distance (IPathfinderNode a, IPathfinderNode b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        IPathfinderNode directConnection = a.connections.Find (c => c == b);
        if (directConnection != null)
        {
            result = tileSize;
        }
        return result;
    }

    private float Heuristic (IPathfinderNode a, IPathfinderNode b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance (a.position, b.position);
    }
}