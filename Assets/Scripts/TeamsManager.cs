using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Does not need to inherit from Monobehavior, but i figured it be easeir to maintain if all managers can be editable from scene
public class TeamsManager : MonoBehaviour
{
    public static TeamsManager instance;
    public int numberOfTeams = 0;
    private List<Team> teams = new List<Team>();
    private HashSet<int> teamsIds = new HashSet<int>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    /// <summary>
    /// Adds a team for TeamManager to be aware of, assigning it a unique id
    /// </summary>
    /// <param name="team">Team to add</param>
    public void AddTeam(Team team)
    {
        if (team == null || teamsIds.Contains(team.id)) return;

        int newId = 0;
        while (teamsIds.Contains(newId)) newId++;
        team.id = newId;

        teamsIds.Add(newId);
        teams.Insert(newId, team);
        numberOfTeams++;
    }
    public Team GetTeamByIndex(int index)
    {
        return teams[index];
    }

}