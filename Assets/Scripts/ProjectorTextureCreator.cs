using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProjectorTextureCreator
{
    public static void UpdateTexture<T>(ref Texture2D tex, T[,] arr, Func<T, bool> func, Color c, Vector2Int cellSize)
    {
        UpdateTexture(ref tex, arr, (T a) => func(a) ? 0 : -1, new Color[1] { c }, cellSize);
    }
    public static void UpdateTexture<T>(ref Texture2D tex, T[,] arr, Func<T, int> func, Color[] c, Vector2Int cellSize)
    {
        Color[,] colors = new Color[arr.GetLength(0) + 2, arr.GetLength(1) + 2];
        int temp;
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
            {
                temp = func(arr[x, y]);
                if (temp >= 0 && temp < c.Length) colors[x + 1, y + 1] = c[temp];
            }
        }
        UpdateTexture(ref tex, colors, cellSize);
    }

    public static void UpdateTexture(ref Texture2D tex, Color[,] c, Vector2Int cellSize)
    {
        int sizeX = c.GetLength(0) * cellSize.x, sizeY = c.GetLength(1) * cellSize.y;
        tex.Resize(sizeX, sizeY, TextureFormat.ARGB32, false);
        tex.SetPixels(0, 0, sizeX, sizeY, Flatten2DArray(c, cellSize), 0);
        tex.Apply();
    }

    private static T[] Flatten2DArray<T>(T[,] arr, Vector2Int cellSize)
    {
        T[] result = new T[arr.GetLength(0) * cellSize.x * arr.GetLength(1) * cellSize.y];
        T val;
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
            {
                val = arr[x, y];
                for (int dy = 0; dy < cellSize.y; dy++)
                {
                    for (int dx = 0; dx < cellSize.x; dx++)
                    {
                        result[x * cellSize.x + dx + y * arr.GetLength(0) * cellSize.x * cellSize.y + dy * arr.GetLength(0) * cellSize.x] = val;
                    }
                }
            }
        }
        return result;
    }
}
