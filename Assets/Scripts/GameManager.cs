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
    public static int randomSeed;
    public int I_randomSeed;
    public bool animateTerrainGeneration = false;
    public Character character;
    public Vector2Int startPos;

    public float movementRange = 10;
    public float inclinedMovementEffortMultiplier;
    private Camera mainCam;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        randomSeed = I_randomSeed;
        mainCam = Camera.main;

        EnvironmentManager.instance.terrainGenerator.Generate(GameManager.randomSeed, animateTerrainGeneration);
        EnvironmentManager.instance.RefreshGridLinesProjection();
        EnvironmentManager.instance.RefreshMovementRangeProjection(null, Vector2Int.zero);

        Character selectedCharacter = Instantiate(character, transform);
        selectedCharacter.InitCharacter(EnvironmentManager.nodeMap[startPos.x, startPos.y]);
        TeamsManager.instance.AddTeam(new Team(new List<Character>() { selectedCharacter }));
        selectedCharacter = null;
    }

    private void Update()
    {
        //Debug.Log(GameManager.instance);
        //if (Input.GetKeyDown(KeyCode.A)) terrainGenerator.Generate(randomSeed, doGenerationAnimation);
        Click();
    }

    private void Click()
    {
        RaycastHit hit;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            if (objectHit == EnvironmentManager.instance.terrainGenerator.transform)
            {
                if (SelectionManager.instance.currentlySelected != null)
                {
                    Character selectedCharacter = SelectionManager.instance.currentlySelected as Character;
                    if (selectedCharacter != null)
                    {
                        List<EnvironmentNode> l = Pathfinder.Solve(EnvironmentManager.allNodes, selectedCharacter.currentNode, EnvironmentManager.ConvertVectorToNode(hit.point), character.PathfindingD, character.PathfindingH);
                        EnvironmentManager.instance.RefreshPathLine(l, selectedCharacter.characterClass.willSink);
                        if (Input.GetButtonDown("Fire1")) selectedCharacter.TryGoTo(EnvironmentManager.ConvertVectorToNode(hit.point));
                    }
                }
            }
        }
        else
        {
            EnvironmentManager.instance.RefreshMovementRangeProjection(null, Vector2Int.zero); //click Manager???
            EnvironmentManager.instance.RefreshPathLine(null);
        }
    }
}