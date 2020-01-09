using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnviromentTerrainGenerator : MonoBehaviour
{
    public int randomSeed;

    [Header("Enviroment Grid Physical Attributes")]
    public Vector2Int gridSizeInCells = Vector2Int.one * 50;
    public Vector2Int cellResolution = Vector2Int.one;
    public Vector2 cellSize = Vector2.one;
    public Vector3 origin = Vector3.zero;


    [Header("Enviroment Terrain Proc-Gen Attributes")]
    public float heightMultiplier = 5;
    public float frequency = 0.2f; //Delta between each perlin noise sampling location
    [Range(1, 8)]
    public int octaves = 1; //Number of perlin noise layers
    [Range(1f, 4f)]
    public float lacunarity = 2f; //Multiplier for perlin noise frequency for each noise layer
    [Range(0f, 1f)]
    public float persistence = 0.5f; //Multiplier for how significant each layer should apply to the layer above
    public Gradient coloring;

    [Header("Water")]
    public float waterLevel = 4.5f;
    public Material waterMat;
    private Transform waterQuad;


    [Header("Terrain Mesh Attributes")]
    private Vector2Int gridResolution;
    private Mesh mesh;
    private float[] heightMap;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Color[] colors;

    [Header("Grid Lines")]
    public bool showGridLines = true;
    public GameObject gridLinesProjectorPrefab;
    public Vector3 gridProjectorOffset = new Vector3(0, 15, 0);
    private Projector gridProjector;

    [Header("Misc")]
    public bool doGenerationAnimation = true;
    public float terrainAnimDuration = 2.5f;
    public float waterAnimDuration = 2.5f;
    private IEnumerator routine;

    private void OnEnable()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Enviroment Surface Mesh";
            GetComponent<MeshFilter>().mesh = mesh;
        }
        //Generate(doGenerationAnimation);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) Generate(doGenerationAnimation);
    }

    private void Generate(bool animate)
    {
        CreateGridMesh();
        CreateHeightMap();
        CreateWater();
        RefreshGridLines();
        if (animate)
        {
            StartCoroutine(AnimateTerrainHeight(terrainAnimDuration * 0.8f));
            StartCoroutine(AnimateWaterHeight(waterAnimDuration));
        }
        else ApplyHeightMap();
    }

    /// <summary>
    /// Creates the terrain mesh using perlin noise
    /// </summary>
    public void CreateGridMesh()
    {
        mesh.Clear();
        gridResolution = gridSizeInCells * cellResolution;

        vertices = new Vector3[(gridResolution.x + 1) * (gridResolution.y + 1)];
        colors = new Color[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        Vector2 cellStepSize = cellSize / cellResolution;
        for (int v = 0, y = 0; y <= gridResolution.y; y++)
        {
            for (int x = 0; x <= gridResolution.x; x++, v++)
            {
                vertices[v] = origin + new Vector3(x * cellStepSize.x, 0, y * cellStepSize.y) - new Vector3(cellSize.x, 0, cellSize.y) * 0.5f;
                colors[v] = coloring.Evaluate(0);
                uv[v] = new Vector2(x * cellStepSize.x, y * cellStepSize.y);
            }
        }

        int[] triangles = new int[gridResolution.x * gridResolution.y * 6];
        for (int t = 0, v = 0, y = 0; y < gridResolution.y; y++, v++)
        {
            for (int x = 0; x < gridResolution.x; x++, v++, t += 6)
            {
                triangles[t] = v;
                triangles[t + 1] = v + gridResolution.x + 1;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + gridResolution.x + 1;
                triangles[t + 5] = v + gridResolution.x + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void CreateHeightMap()
    {
        Random.InitState(randomSeed);
        Vector2 perlinOffset = new Vector2(Random.value, Random.value) * 1000;
        heightMap = new float[vertices.Length];
        for (int v = 0, y = 0; y <= gridResolution.y; y++)
        {
            for (int x = 0; x <= gridResolution.x; x++, v++)
            {
                heightMap[v] = GetAdaptedPerlinNoiseValue(new Vector2(x, y) + perlinOffset, frequency, octaves, lacunarity, persistence);
            }
        }
    }

    public void ApplyHeightMap()
    {
        for (int v = 0, y = 0; y <= gridResolution.y; y++)
        {
            for (int x = 0; x <= gridResolution.x; x++, v++)
            {
                vertices[v].y = heightMap[v] * heightMultiplier;
                colors[v] = coloring.Evaluate(heightMap[v]);
            }
        }
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    public IEnumerator AnimateTerrainHeight(float duration)
    {
        if (mesh == null) yield break;
        if (heightMap == null) yield break;

        float smoothProgress = 0, time = Time.time;
        while (smoothProgress < 1)
        {
            smoothProgress = Mathf.SmoothStep(0, 1, (Time.time - time) / duration);
            float val;
            for (int v = 0, y = 0; y <= gridResolution.y; y++)
            {
                for (int x = 0; x <= gridResolution.x; x++, v++)
                {
                    val = Mathf.Lerp(0, heightMap[v], smoothProgress);
                    vertices[v].y = val * heightMultiplier;
                    colors[v] = coloring.Evaluate(val);
                }
            }
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            yield return new WaitForEndOfFrame();
        }

        ApplyHeightMap();//Just in case
    }

    public void CreateWater()
    {
        if (waterQuad == null)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Water";
            quad.GetComponent<MeshRenderer>().material = waterMat;
            waterQuad = quad.transform;
            waterQuad.rotation = Quaternion.Euler(Vector3.right * 90);
            waterQuad.SetParent(this.transform);
        }
        waterQuad.localPosition = new Vector3((gridSizeInCells.x - 1) * cellSize.x * 0.5f, waterLevel, (gridSizeInCells.y - 1) * cellSize.y * 0.5f);
        waterQuad.localScale = (Vector3)(gridSizeInCells * cellSize) + Vector3.forward;
    }

    public IEnumerator AnimateWaterHeight(float duration)
    {
        float smoothProgress = 0, time = Time.time;
        while (smoothProgress < 1)
        {
            smoothProgress = Mathf.SmoothStep(0, 1, (Time.time - time) / duration);
            waterQuad.localPosition = new Vector3(waterQuad.localPosition.x, Mathf.Lerp(0, waterLevel, smoothProgress), waterQuad.localPosition.z);
            yield return new WaitForEndOfFrame();
        }
    }
    /// <summary>
    /// Refreshes the grid lines that is projected onto the terrain mesh
    /// </summary>
    public void RefreshGridLines()
    {
        if (gridProjector == null)
        {
            gridProjector = Instantiate(gridLinesProjectorPrefab, Vector3.zero, Quaternion.Euler(Vector3.right * 90), this.transform).GetComponent<Projector>();
        }
        gridProjector.enabled = showGridLines;
        gridProjector.transform.localPosition = gridProjectorOffset + new Vector3(cellSize.x * 0.5f, 0, cellSize.y * 0.5f);
        gridProjector.orthographicSize = cellSize.y * 0.5f;
        gridProjector.aspectRatio = cellSize.x / cellSize.y;
    }

    /// <summary>
    /// Gets a psudo-random value from the sum of multiple perlin noise values with varying weightages.
    /// </summary>
    /// <param name="position">Position of perlin noise to sample from. </param>
    /// <param name="frequency">Unit size/scale of perlin noise sample positions. </param>
    /// <param name="octaves">Number of perlin noise layers. </param>
    /// <param name="lacunarity">Multiplier for perlin noise frequency for each noise layer. </param>
    /// <param name="persistence">Multiplier for how significant each layer should apply to the layer above. </param>
    /// <returns>Perlin noise float value</returns>
    public float GetAdaptedPerlinNoiseValue(Vector2 position, float frequency, int octaves, float lacunarity, float persistence)
    {
        float sum = Mathf.PerlinNoise(position.x * frequency, position.y * frequency);
        float amplitude = 1f;
        float range = 1f;
        for (int o = 1; o < octaves; o++)
        {
            frequency *= lacunarity;
            amplitude *= persistence;
            range += amplitude;
            sum += Mathf.PerlinNoise(position.x * frequency, position.y * frequency) * amplitude;
        }
        return sum / range;
    }
}
