using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentProjectorManager : ProjectorTextureCreator
{
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
        RefreshGridLinesProjection();
        RefreshMovementRangeProjection(null, Vector2Int.zero);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) RefreshGridLinesProjection();
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
        moveRangeProjector.aspectRatio = EnvironmentTerrainGenerator.trueCellSize.x / EnvironmentTerrainGenerator.trueCellSize.y;
    }
}
