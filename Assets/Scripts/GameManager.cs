using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: MAKE GAMEMANAGER(Game.cs) use turns
//GameManager in charge of actually starting a game -> team formation, click events during game, deaths etc

//inherits from Monobehavior to take advantage of unity inbuilt coroutines inplementaions
public class GameManager : MonoBehaviour
{
    public int randomSeed;
    public EnvironmentTerrainGenerator terrainGenerator;
    public EnviromentProjectorManager projectorManager;
    public bool doGenerationAnimation = true;
    public Transform pTransform;

    public float movementRange = 10;
    public float inclinedMovementEffortMultiplier;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        terrainGenerator.Generate(randomSeed, doGenerationAnimation);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A)) terrainGenerator.Generate(randomSeed, doGenerationAnimation);
        if (Input.GetButtonDown("Fire1")) Click();
    }

    private void Click()
    {
        RaycastHit hit;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            if (objectHit == terrainGenerator.transform) ShowMovementRange(terrainGenerator.ConvertVectorToNode(hit.point), movementRange, PathfindingD);
            else projectorManager.RefreshMovementRangeProjection(null, Vector2Int.zero);
        }
        else
        {
            projectorManager.RefreshMovementRangeProjection(null, Vector2Int.zero); //click Manager???
        }
    }

    public void ShowMovementRange(EnvironmentNode node, float movementRange, Func<EnvironmentNode, EnvironmentNode, float> rangeFunc)
    {
        HashSet<EnvironmentNode> r = Pathfinder.IsInRange(EnvironmentTerrainGenerator.allNodes, node, movementRange, rangeFunc);
        int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;
        int temp;
        foreach (EnvironmentNode n in r)
        {
            temp = n.indexPosition.x - node.indexPosition.x;
            xMin = Mathf.Min(xMin, temp);
            xMax = Mathf.Max(xMax, temp);
            temp = n.indexPosition.y - node.indexPosition.y;
            yMin = Mathf.Min(yMin, temp);
            yMax = Mathf.Max(yMax, temp);
        }

        Vector2Int movementRangeInTiles = new Vector2Int(Mathf.Max(-xMin, xMax), Mathf.Max(-yMin, yMax));
        Vector2Int delta = movementRangeInTiles - node.indexPosition;
        bool[,] b = new bool[movementRangeInTiles.x * 2 + 1, movementRangeInTiles.y * 2 + 1];
        foreach (EnvironmentNode v in r)
        {
            b[v.indexPosition.x + delta.x, v.indexPosition.y + delta.y] = true;
        }

        projectorManager.RefreshMovementRangeProjection(b, node.indexPosition);
    }

    //Should be on the character themselves
    public float PathfindingD(EnvironmentNode n1, EnvironmentNode n2)
    {

        float cost; //= Mathf.Sqrt(Vector2.SqrMagnitude(n1.indexPosition - n2.indexPosition) + ((n2.effortWeightage < n1.effortWeightage) ? 0 : Mathf.Pow((n2.effortWeightage - n1.effortWeightage) *uphillMovementWeightageMultiplier, 2)));
        cost = Vector2.Distance(n1.indexPosition, n2.indexPosition);
        if (n2.terrain.Contains(TerrainType.Water)) return cost * 2f;
        cost += (n2.effortWeightage - n1.effortWeightage) * inclinedMovementEffortMultiplier;
        //Debug.Log((n2.effortWeightage - n1.effortWeightage) * uphillMovementWeightageMultiplier);
        if (n2.terrain.Contains(TerrainType.Trees)) cost *= 1.5f;
        return cost;
    }
    public float PathfindingH(EnvironmentNode n1, EnvironmentNode n2)
    {
        return Mathf.Sqrt(Vector2.SqrMagnitude(n1.indexPosition - n2.indexPosition) + Mathf.Pow(n2.effortWeightage - n1.effortWeightage, 2));
    }

}