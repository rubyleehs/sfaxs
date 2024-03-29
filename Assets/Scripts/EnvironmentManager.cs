﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public EnvironmentTerrainGenerator terrainGenerator;

    [Header("Statics")]
    public static EnvironmentManager instance;
    public static Vector3 trueOrigin;
    public static Vector2 trueCellSize;
    public static float trueWaterLevel;
    public static float trueSnowLineLevel;
    public static float trueCoastLineLevel;
    public static EnvironmentNode[,] nodeMap;
    public static List<EnvironmentNode> allNodes = new List<EnvironmentNode>();

    [Header("Path Line")]
    public LineRenderer pathRenderer;
    public float pathlineDeltaHeight = 5;

    [Header("Grid Lines")]
    public bool showGridLines = true;
    public GameObject gridLinesProjectorPrefab;
    public Vector2 gridProjectorOffset = Vector2.zero;
    private Projector gridProjector;

    [Header("Move Range")]
    public Color moveRangeColor = Color.white;
    public Projector moveRangeProjector;
    public Texture2D moveRangeTexture;

    [Header("Misc")]
    public Transform projectorsParent;
    public float projectorsHeight = 10;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void GenerateTerrain(int randomSeed, bool animateTerrainGeneration)
    {
        terrainGenerator.Generate(randomSeed, animateTerrainGeneration);

        RefreshGridLinesProjection();
        RefreshMovementRangeProjection(null, Vector2Int.zero);
    }

    /// <summary>
    /// Refreshes the grid lines that is projected onto the terrain mesh
    /// </summary>
    public void RefreshGridLinesProjection()
    {
        if (gridProjector == null)
        {
            gridProjector = Instantiate(gridLinesProjectorPrefab, Vector3.zero, Quaternion.Euler(Vector3.right * 90), projectorsParent).GetComponent<Projector>();
        }
        gridProjector.enabled = showGridLines;
        gridProjector.transform.position = new Vector3(gridProjectorOffset.x, projectorsHeight, gridProjectorOffset.y) + new Vector3(trueCellSize.x * 0.5f, 0, trueCellSize.y * 0.5f);
        gridProjector.orthographicSize = trueCellSize.y * 0.5f;
        gridProjector.aspectRatio = trueCellSize.x / trueCellSize.y;
    }
    public void RefreshMovementRangeProjection(bool[,] map, Vector2Int center)
    {
        if (map == null)
        {
            moveRangeProjector.enabled = false;
            return;
        }
        else moveRangeProjector.enabled = true;
        ProjectorTextureCreator.UpdateTexture(ref moveRangeTexture, map, (a) => a, moveRangeColor, Vector2Int.one);
        Vector3 temp = nodeMap[center.x, center.y].position;
        moveRangeProjector.transform.position = new Vector3(temp.x, projectorsHeight, temp.z);
        moveRangeProjector.orthographicSize = (map.GetLength(1) + 2) * 0.5f * trueCellSize.y;
        moveRangeProjector.aspectRatio = ((map.GetLength(0) + 2) * trueCellSize.x) / ((map.GetLength(1) + 2) * trueCellSize.y);
    }

    public void RefreshMovementRangeProjection(HashSet<EnvironmentNode> nodes, EnvironmentNode center)
    {
        int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;
        int temp;
        foreach (EnvironmentNode n in nodes)
        {
            temp = n.indexPosition.x - center.indexPosition.x;
            xMin = Mathf.Min(xMin, temp);
            xMax = Mathf.Max(xMax, temp);
            temp = n.indexPosition.y - center.indexPosition.y;
            yMin = Mathf.Min(yMin, temp);
            yMax = Mathf.Max(yMax, temp);
        }

        Vector2Int movementRangeInTiles = new Vector2Int(Mathf.Max(-xMin, xMax), Mathf.Max(-yMin, yMax));
        Vector2Int delta = movementRangeInTiles - center.indexPosition;
        bool[,] b = new bool[movementRangeInTiles.x * 2 + 1, movementRangeInTiles.y * 2 + 1];
        foreach (EnvironmentNode v in nodes)
        {
            b[v.indexPosition.x + delta.x, v.indexPosition.y + delta.y] = true;
        }

        RefreshMovementRangeProjection(b, center.indexPosition);
    }

    public void RefreshPathLine(List<EnvironmentNode> path, bool willSink = false)
    {
        if (path == null)
        {
            pathRenderer.enabled = false;
            return;
        }
        pathRenderer.enabled = true;
        pathRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            pathRenderer.SetPosition(i, path[i].position + Vector3.up * Mathf.Max(pathlineDeltaHeight, trueWaterLevel - path[i].position.y));
        }

    }

    /// <summary>
    /// Converts a world space vector to it's associated EnviromentNode
    /// </summary>
    public static EnvironmentNode ConvertVectorToNode(Vector3 point)
    {
        point -= trueOrigin;
        int x = (int)(point.x / EnvironmentManager.trueCellSize.x + 0.5f);
        int z = (int)(point.z / EnvironmentManager.trueCellSize.y + 0.5f);

        return EnvironmentManager.nodeMap[x, z];
    }
}
