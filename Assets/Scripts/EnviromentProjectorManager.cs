using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentProjectorManager : ProjectorTextureCreator
{
    public static EnviromentProjectorManager instance;

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

        RefreshGridLinesProjection();
        RefreshMovementRangeProjection(null, Vector2Int.zero);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A)) RefreshGridLinesProjection();
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
        gridProjector.transform.position = new Vector3(gridProjectorOffset.x, projectorsHeight, gridProjectorOffset.y) + new Vector3(EnvironmentTerrainGenerator.trueCellSize.x * 0.5f, 0, EnvironmentTerrainGenerator.trueCellSize.y * 0.5f);
        gridProjector.orthographicSize = EnvironmentTerrainGenerator.trueCellSize.y * 0.5f;
        gridProjector.aspectRatio = EnvironmentTerrainGenerator.trueCellSize.x / EnvironmentTerrainGenerator.trueCellSize.y;
    }
    public void RefreshMovementRangeProjection(bool[,] map, Vector2Int center)
    {
        if (map == null)
        {
            moveRangeProjector.enabled = false;
            return;
        }
        else moveRangeProjector.enabled = true;
        UpdateTexture(ref moveRangeTexture, map, (a) => a, moveRangeColor, Vector2Int.one);
        Vector3 temp = EnvironmentTerrainGenerator.nodeMap[center.x, center.y].position;
        moveRangeProjector.transform.position = new Vector3(temp.x, projectorsHeight, temp.z);
        moveRangeProjector.orthographicSize = (map.GetLength(1) + 2) * 0.5f * EnvironmentTerrainGenerator.trueCellSize.y;
        moveRangeProjector.aspectRatio = ((map.GetLength(0) + 2) * EnvironmentTerrainGenerator.trueCellSize.x) / ((map.GetLength(1) + 2) * EnvironmentTerrainGenerator.trueCellSize.y);
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
}
