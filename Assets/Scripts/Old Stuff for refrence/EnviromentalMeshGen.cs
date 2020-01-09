using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentalMeshGen : GridMeshGen
{
    [SerializeField] private Vector2Int mapResolutionInRegions = new Vector2Int(10, 10);
    [SerializeField] private int cellsPerMapRegion = 5;
    private Vector2Int mapResolution;
    [SerializeField] private Vector3 cellSize = new Vector3(1, 0.1f, 1);

    [SerializeField] private Vector2 globalHeightMinMax = new Vector2(0, 150);
    [SerializeField] private Vector2 regionHeightMinMax = new Vector2(0, 2);

    [SerializeField] private int heightMapAverageMaskRange = 3; //Decreases height variance in the micro scale.
    [SerializeField] private int regionMapAverageMaskRange = 1; //Decreases height variance in the macro scale. 

    [SerializeField] private Vector3 gridProjectorOffset;

    private float[,] globalHeightMap; //height map are for the corners of a cell, its a 2d array so it is easeir to implement more stuff in future. coverted to 1d later on
    private float[,] regionHeightMap;
    private Vector3 origin = Vector3.zero;
    private Projector gridProjector;



    private void Awake()
    {
        GenerateMesh();
        ReconfigureGridProjector();
    }

    /// <summary>
    /// Generates the ground landscape mesh according to the variables set from the inspector.
    /// </summary>
    private void GenerateMesh()
    {
        mapResolution = mapResolutionInRegions * cellsPerMapRegion;
        regionHeightMap = IncreaseResolutionOf2DArray(AverageNearby(CreateRandom2DArray(mapResolutionInRegions, regionHeightMinMax.x, regionHeightMinMax.y), regionMapAverageMaskRange), cellsPerMapRegion);
        globalHeightMap = CreateRandom2DArray(mapResolution, globalHeightMinMax.x, globalHeightMinMax.y);
        for (int y = 0; y < mapResolution.y; y++)
        {
            for (int x = 0; x < mapResolution.x; x++)
            {
                globalHeightMap[x, y] *= regionHeightMap[x, y];
            }
        }
        globalHeightMap = AverageNearby(globalHeightMap, heightMapAverageMaskRange);
        globalHeightMap = AverageNearby(globalHeightMap, 1); //Average once more to smooth things out/prevent large diffrences.


        base.GenerateMesh(globalHeightMap, cellSize, origin);
    }

    /// <summary>
    /// Reconfigures the projector in charge or drawing grid lines onto the landscape mesh according to the variables set from inspector.
    /// </summary>
    private void ReconfigureGridProjector()
    {
        if (gridProjector == null) gridProjector = this.GetComponentInChildren<Projector>();
        gridProjector.transform.localPosition = gridProjectorOffset;
        gridProjector.orthographicSize = cellSize.z * 0.5f;
        gridProjector.aspectRatio = cellSize.x / cellSize.z;
    }

    /// <summary>
    /// Increases the Resolution of a 2d array by a multiplier
    /// </summary>
    /// <param name="arr">2d array to increase. </param> 
    /// <param name="multiplier">Multiplier per axis. </param> 
    /// <returns>2d array with each axis size multiplied by multiplier. </returns>
    private float[,] IncreaseResolutionOf2DArray(float[,] arr, int multiplier)
    {
        float[,] result = new float[arr.GetLength(0) * multiplier, arr.GetLength(1) * multiplier];
        float val;
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
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

    /// <param name="size">Size of the output array. </param>
    /// <param name="min">Inclusive min for range of random values. </param>
    /// <param name="max">Inclusive max for range of random values. </param>
    /// <returns>2d array of size size with random values ranging from min to max. </returns>
    private float[,] CreateRandom2DArray(Vector2Int size, float min, float max)
    {
        float[,] result = new float[size.x, size.y];
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                result[x, y] = Random.Range(min, max);
            }
        }
        return result;
    }

    /// <summary>
    /// Averages surrounding values from arr
    /// </summary>
    /// <param name="arr">2d array of values to use. </param> 
    /// <param name="range">How far from center should the values be considered into the average. </param>
    /// <returns>2d array of averaged values. </returns>
    private float[,] AverageNearby(float[,] arr, int range)
    {
        float[,] result = new float[arr.GetLength(0), arr.GetLength(1)]; //
        int count;
        int cx, cy;
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
            {
                count = 0;
                for (int dy = -range; dy <= range; dy++)
                {
                    for (int dx = -range; dx <= range; dx++)
                    {
                        cx = x + dx;
                        cy = y + dy;

                        if (cx < 0 || cy < 0 || cx >= arr.GetLength(0) || cy >= arr.GetLength(1)) continue;

                        result[x, y] += arr[cx, cy];
                        count++;
                    }
                }
                result[x, y] /= count;
            }
        }
        return result;
    }

    /* private T[] FlattenTo1DArray<T>(ref T[,] arr)
    {
        T[] result = new T[arr.GetLength(0) * arr.GetLength(1)];

        for (int y = 0, i = 0; y < arr.GetLength(1); y++)
        {
            for (int x = 0; x < arr.GetLength(0); x++, i++)
            {
                result[i] = arr[x, y];
            }
        }
        return result;
    }
 */

}
