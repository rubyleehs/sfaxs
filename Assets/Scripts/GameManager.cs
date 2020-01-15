using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: MAKE GAMEMANAGER(Game.cs) use turns
//GameManager in charge of actually starting a game -> team formation, click events during game, deaths etc

//inherits from Monobehavior to take advantage of unity inbuilt coroutines inplementaions
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int randomSeed;
    public EnvironmentTerrainGenerator terrainGenerator;
    public bool doGenerationAnimation = true;
    public Character character;
    public Vector2Int startPos;

    public float movementRange = 10;
    public float inclinedMovementEffortMultiplier;
    private Camera mainCam;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        mainCam = Camera.main;
        terrainGenerator.Generate(randomSeed, doGenerationAnimation);

        Character selectedCharacter = Instantiate(character, transform);
        selectedCharacter.InitCharacter(EnvironmentTerrainGenerator.nodeMap[startPos.x, startPos.y]);
        TeamsManager.instance.AddTeam(new Team(new List<Character>() { selectedCharacter }));
        selectedCharacter = null;
    }

    private void Update()
    {
        //Debug.Log(GameManager.instance);
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
            if (objectHit == terrainGenerator.transform)
            {
                if (SelectionManager.instance.currentlySelected != null)
                {
                    Character selectedCharacter = SelectionManager.instance.currentlySelected as Character;
                    if (selectedCharacter != null) selectedCharacter.TryGoTo(terrainGenerator.ConvertVectorToNode(hit.point));
                }
            }
        }
        else
        {
            EnviromentProjectorManager.instance.RefreshMovementRangeProjection(null, Vector2Int.zero); //click Manager???
        }
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