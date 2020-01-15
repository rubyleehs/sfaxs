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
    private Camera mainCam;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        mainCam = Camera.main;
        EnvironmentManager.instance.GenerateTerrain(randomSeed, doGenerationAnimation);

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
            if (objectHit == terrainGenerator.transform)
            {
                if (SelectionManager.instance.currentlySelected != null)
                {
                    Character selectedCharacter = SelectionManager.instance.currentlySelected as Character;

                    if (selectedCharacter != null)
                    {
                        EnvironmentManager.instance.RefreshPathLine(Pathfinder.Solve(EnvironmentManager.allNodes, selectedCharacter.currentNode, EnvironmentManager.ConvertVectorToNode(hit.point), selectedCharacter.PathfindingD, selectedCharacter.PathfindingH));
                        if (Input.GetButtonDown("Fire1")) selectedCharacter.TryGoTo(EnvironmentManager.ConvertVectorToNode(hit.point));
                    }
                }
            }
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            EnvironmentManager.instance.RefreshMovementRangeProjection(null, Vector2Int.zero); //click Manager???
            EnvironmentManager.instance.RefreshPathLine(null);
        }
    }
}