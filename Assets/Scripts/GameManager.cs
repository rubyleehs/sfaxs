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

    public int steps = 10;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        //terrainGenerator.Generate(randomSeed, doGenerationAnimation);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) terrainGenerator.Generate(randomSeed, doGenerationAnimation);
        if (Input.GetButtonDown("Fire1")) Click();
    }

    private void Click()
    {
        RaycastHit hit;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            if (objectHit == terrainGenerator.transform) ShowMovementRange(terrainGenerator.ConvertVectorToNode(hit.point).indexPosition, steps);
            else projectorManager.RefreshMovementRangeProjection(null, Vector2Int.zero);
        }
        else
        {
            projectorManager.RefreshMovementRangeProjection(null, Vector2Int.zero); //click Manager???
        }
    }

    public void ShowMovementRange(Vector2Int nodeIndex, int steps)
    {
        HashSet<Vector2Int> n1 = new HashSet<Vector2Int>();
        HashSet<Vector2Int> n2 = new HashSet<Vector2Int>();
        EnvironmentNode temp;
        n1.Add(nodeIndex);

        for (int s = 0; s < steps; s++)
        {
            foreach (Vector2Int v in n1)
            {
                temp = EnvironmentTerrainGenerator.nodeMap[v.x, v.y];
                for (int i = 0; i < temp.connections.Count; i++)
                {
                    if (n1.Contains(temp.connections[i].indexPosition)) continue;
                    if (temp.connections[i].isAccessible) n2.Add(temp.connections[i].indexPosition);
                }
            }

            foreach (Vector2Int v in n2)
            {
                n1.Add(v);
            }
            n2.Clear();
        }

        Vector2Int delta = Vector2Int.one * steps - nodeIndex;
        bool[,] b = new bool[steps * 2 + 1, steps * 2 + 1];
        foreach (Vector2Int v in n1)
        {
            b[v.x + delta.x, v.y + delta.y] = true;
        }

        projectorManager.RefreshMovementRangeProjection(b, nodeIndex);
    }


}