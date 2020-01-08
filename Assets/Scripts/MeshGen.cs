using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMeshGen : MonoBehaviour
{
    protected Vector2Int meshResolution;
    protected Vector3 cellSize;

    private Vector3[] meshVertices;
    private Mesh mesh;

    protected void GenerateMesh(float[,] meshVerticesHeight, Vector3 cellSize, Vector3 bottomRightCorner)
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        this.meshResolution = new Vector2Int(meshVerticesHeight.GetLength(0), meshVerticesHeight.GetLength(1));
        this.cellSize = cellSize;
        meshVertices = new Vector3[(meshResolution.x) * (meshResolution.y)];
        Vector2[] uv = new Vector2[meshVertices.Length];
        for (int i = 0, y = 0; y < meshResolution.y; y++)
        {
            for (int x = 0; x < meshResolution.x; x++, i++)
            {
                meshVertices[i] = bottomRightCorner + new Vector3(x * cellSize.x, 0, y * cellSize.z);
                uv[i] = new Vector2((float)x / meshResolution.x, (float)y / meshResolution.y); //Create UVs for mesh
            }
        }
        if (meshVerticesHeight != null)
        {
            for (int i = 0, y = 0; y < meshResolution.y; y++)
            {
                for (int x = 0; x < meshResolution.x; x++, i++)
                {
                    meshVertices[i] += Vector3.up * meshVerticesHeight[x, y] * cellSize.y;
                }
            }
        }

        //Create triangles for mesh
        int[] triangles = new int[(meshResolution.x - 1) * (meshResolution.y - 1) * 6];

        for (int ti = 0, vi = 0, y = 0; y < meshResolution.y - 1; y++, vi++)
        {
            for (int x = 0; x < meshResolution.x - 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + meshResolution.x;
                triangles[ti + 5] = vi + meshResolution.x + 1;
            }
        }

        //Debug.Log(meshVertices.Length + " | " + triangles.Length);
        mesh.vertices = meshVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (meshVertices == null)
        {
            Debug.Log("no vertices found");
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < meshVertices.Length; i++)
        {
            Gizmos.DrawSphere(meshVertices[i], 0.1f);
        }
    }
}