using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: MAKE GAMEMANAGER(Game.cs) use turns
//GameManager in charge of actually starting a game -> team formation, click events during game, deaths etc

//inherits from Monobehavior to take advantage of unity inbuilt coroutines inplementaions
public class TurnManager : MonoBehaviour
{
    public static TurnManager instance = null;
    private int currentTurnTeamIndex = 0;

    private List<Character> movableCharactersOnTurn;

    private int turnCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    public void StartNextTurn()
    {
        StartNewTurn((currentTurnTeamIndex + 1) % TeamsManager.instance.numberOfTeams);
    }

    public void StartNewTurn(int teamIndex)
    {
        List<Character> characterList = TeamsManager.instance.GetTeamByIndex(teamIndex).members;
    }
    public void StartNewTurn(List<Character> charactersToMoveThisTurn)
    {
        turnCount++;
        movableCharactersOnTurn.Clear();
        movableCharactersOnTurn = charactersToMoveThisTurn;
    }

    public void EndCharacterTurn(Character character)
    {
        movableCharactersOnTurn.Remove(character);
        if (movableCharactersOnTurn.Count <= 0) StartNextTurn();
    }

    public bool IsStillCharacterTurn(Character character)
    {
        return movableCharactersOnTurn.Contains(character);
    }

}