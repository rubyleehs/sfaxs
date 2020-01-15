using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Statics")]
    public static EnvironmentManager instance;
    public static Vector3 trueOrigin;
    public static Vector2 trueCellSize;
    public static float trueWaterLevel;
    public static float trueSnowLineLevel;
    public static float trueCoastLineLevel;
    public static EnvironmentNode[,] nodeMap;
    public static List<EnvironmentNode> allNodes = new List<EnvironmentNode>();

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
        gridProjector.transform.position = new Vector3(gridProjectorOffset.x, projectorsHeight, gridProjectorOffset.y) + new Vector3(EnvironmentManager.trueCellSize.x * 0.5f, 0, EnvironmentManager.trueCellSize.y * 0.5f);
        gridProjector.orthographicSize = EnvironmentManager.trueCellSize.y * 0.5f;
        gridProjector.aspectRatio = EnvironmentManager.trueCellSize.x / EnvironmentManager.trueCellSize.y;
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
        Vector3 temp = EnvironmentManager.nodeMap[center.x, center.y].position;
        moveRangeProjector.transform.position = new Vector3(temp.x, projectorsHeight, temp.z);
        moveRangeProjector.orthographicSize = (map.GetLength(1) + 2) * 0.5f * EnvironmentManager.trueCellSize.y;
        moveRangeProjector.aspectRatio = ((map.GetLength(0) + 2) * EnvironmentManager.trueCellSize.x) / ((map.GetLength(1) + 2) * EnvironmentManager.trueCellSize.y);
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
