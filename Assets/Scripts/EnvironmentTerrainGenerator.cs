using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnvironmentTerrainGenerator : MonoBehaviour
{
    public int randomSeed;

    [Header("Environment Grid Physical Attributes")]
    public Vector2Int gridSizeInCells = Vector2Int.one * 50;
    public Vector2Int cellResolution = Vector2Int.one;
    public Vector2 cellSize = Vector2.one;
    public Vector3 origin = Vector3.zero;


    [Header("Environment Terrain Proc-Gen Attributes - Height")]
    public float heightMultiplier = 5;
    public float h_frequency = 0.2f; //Delta between each perlin noise sampling location
    [Range(1, 8)]
    public int h_octaves = 1; //Number of perlin noise layers
    [Range(1f, 4f)]
    public float h_lacunarity = 2f; //Multiplier for perlin noise frequency for each noise layer
    [Range(0f, 1f)]
    public float h_persistence = 0.5f; //Multiplier for how significant each layer should apply to the layer above

    [Header("Environment Terrain Proc-Gen Attributes - Temperature")]
    public float t_frequency = 0.2f; //Delta between each perlin noise sampling location
    [Range(1, 8)]
    public int t_octaves = 1; //Number of perlin noise layers
    [Range(1f, 4f)]
    public float t_lacunarity = 2f; //Multiplier for perlin noise frequency for each noise layer
    [Range(0f, 1f)]
    public float t_persistence = 0.5f; //Multiplier for how significant each layer should apply to the layer above

    [Header("Environment Terrain Proc-Gen Attributes - Ground Colors")]

    public float gradientHeightWeightage = 0.7f;
    public float gradientTemperatureWeightage = 0.3f;
    public Gradient groundGradient;

    [Header("Water")]
    public float waterLevel = 4.5f;
    public Material waterMat;
    private Transform waterQuad;

    [Header("Terrain Mesh Attributes")]
    private Vector2Int gridResolution;
    private Mesh mesh;
    private float[] heightMap; //will be multiplied by heightMultiplier to get vertex y pos
    private float[] temperatureMap; //will be multiplied by temperatureMultiplier to get vertex true temperature

    private Vector3[] vertices;
    private Vector3[] normals;
    private Color[] colors;

    [Header("Grid Lines")]
    public bool showGridLines = true;
    public GameObject gridLinesProjectorPrefab;
    public Vector3 gridProjectorOffset = new Vector3(0, 15, 0);
    private Projector gridProjector;

    [Header("Pathfinding Nodes")]
    private EnvironmentNode[,] tileMap;

    [Header("Misc")]
    public bool doGenerationAnimation = true;
    public float terrainAnimDuration = 2.5f;
    public float waterAnimDuration = 2.5f;

    private void OnEnable()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Environment Surface Mesh";
            GetComponent<MeshFilter>().mesh = mesh;
        }
        Generate(doGenerationAnimation);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A)) Generate(doGenerationAnimation);
    }

    public void Generate(bool animate)
    {
        Random.InitState(randomSeed);
        CreateGridMesh();
        CreateHeightMap();
        CreateTemperatureMap();
        CreateWater();
        RefreshGridLines();
        if (animate)
        {
            StartCoroutine(AnimateTerrainHeight(terrainAnimDuration));
            StartCoroutine(AnimateWaterHeight(waterAnimDuration));
        }
        else ApplyHeightAndTemperatureMap();
        CreateTileMap(true);
    }

    /// <summary>
    /// Creates the Grid mesh.
    /// </summary>
    private void CreateGridMesh()
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
                colors[v] = Color.black;
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

    /// <summary>
    /// Creates the temperature map using perlin noise.
    /// </summary>
    private void CreateTemperatureMap()
    {
        Vector2 perlinOffset = new Vector2(Random.value, Random.value) * 1000;
        temperatureMap = new float[vertices.Length];
        for (int v = 0, y = 0; y <= gridResolution.y; y++)
        {
            for (int x = 0; x <= gridResolution.x; x++, v++)
            {
                temperatureMap[v] = GetAdaptedPerlinNoiseValue(new Vector2(x, y) + perlinOffset, t_frequency, t_octaves, t_lacunarity, t_persistence);
            }
        }
    }


    /// <summary>
    /// Creates the height map using perlin noise.
    /// </summary>
    private void CreateHeightMap()
    {
        Vector2 perlinOffset = new Vector2(Random.value, Random.value) * 1000;
        heightMap = new float[vertices.Length];
        for (int v = 0, y = 0; y <= gridResolution.y; y++)
        {
            for (int x = 0; x <= gridResolution.x; x++, v++)
            {
                heightMap[v] = GetAdaptedPerlinNoiseValue(new Vector2(x, y) + perlinOffset, h_frequency, h_octaves, h_lacunarity, h_persistence);
            }
        }
    }

    /// <summary>
    /// Applies the height and temperature map onto the mesh and updates the mesh.
    /// </summary>
    private void ApplyHeightAndTemperatureMap()
    {
        for (int v = 0, y = 0; y <= gridResolution.y; y++)
        {
            for (int x = 0; x <= gridResolution.x; x++, v++)
            {
                vertices[v].y = heightMap[v] * heightMultiplier;
                colors[v] = EvaluateGroundColor(v);
            }
        }
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Animates the height map being applies to the mesh.
    /// </summary>
    /// <param name="duration">Duration of the animation. </param>
    private IEnumerator AnimateTerrainHeight(float duration)
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
                    colors[v] = EvaluateGroundColor(v);
                }
            }
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            yield return new WaitForEndOfFrame();
        }

        ApplyHeightAndTemperatureMap();//Just in case
    }


    /// <summary>
    /// Creates the water quad and positions it at waterLevel.
    /// </summary>
    private void CreateWater()
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

    /// <summary>
    /// Moves the water quad position to y = 0 and animates it rising to water level.
    /// </summary>
    /// <param name="duration">Duration of the animation. </param>
    private IEnumerator AnimateWaterHeight(float duration)
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
    private float GetAdaptedPerlinNoiseValue(Vector2 position, float frequency, int octaves, float lacunarity, float persistence)
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

    private void CreateTileMap(bool connectDiagonals)
    {
        tileMap = new EnvironmentNode[gridSizeInCells.x, gridSizeInCells.y];

        EnvironmentNode temp;
        for (int y = 0; y < gridSizeInCells.y; y++)
        {
            for (int x = 0; x < gridSizeInCells.x; x++)
            {
                tileMap[x, y] = new EnvironmentNode(new Vector3(x * cellSize.x, ApproxCellYPosition(x, y), y * cellSize.y));//Set terrain type here too?
                temp = tileMap[x, y];
                if (x > 0)
                {
                    temp.connections.Add(tileMap[x - 1, y]);
                    tileMap[x - 1, y].connections.Add(temp);
                }
                if (y > 0)
                {
                    temp.connections.Add(tileMap[x, y - 1]);
                    tileMap[x, y - 1].connections.Add(temp);
                }
                if (connectDiagonals)
                {
                    if (y > 0)
                    {
                        if (x < gridSizeInCells.x - 1)
                        {
                            temp.connections.Add(tileMap[x + 1, y - 1]);
                            tileMap[x + 1, y - 1].connections.Add(temp);
                        }
                        if (x > 0)
                        {
                            temp.connections.Add(tileMap[x - 1, y - 1]);
                            tileMap[x - 1, y - 1].connections.Add(temp);
                        }
                    }
                }

            }
        }
    }

    /// <summary>
    /// Approximate the Y posiiton of the center the cell at coordinate (x,y). Currently is an approximate which gets worse the higher the cell resolution is.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float ApproxCellYPosition(int x, int y)
    {
        Vector2Int size = gridResolution + Vector2Int.one;
        return (heightMap[y * size.x * cellResolution.y + x * cellResolution.x] + heightMap[(y + 1) * size.x * cellResolution.y + (x + 1) * cellResolution.x]) * 0.5f * heightMultiplier;
    }

    private Color EvaluateGroundColor(int vertex)
    {
        return groundGradient.Evaluate(heightMap[vertex] * gradientHeightWeightage + temperatureMap[vertex] * gradientTemperatureWeightage);
    }

    private void OnDrawGizmos()
    {
        if (tileMap != null)
        {
            for (int y = 0; y < gridSizeInCells.y; y++)
            {
                for (int x = 0; x < gridSizeInCells.x; x++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(tileMap[x, y].position, 0.1f);

                }
            }
        }
    }
}
