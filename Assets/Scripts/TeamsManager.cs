using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamsManager
{
    private static readonly TeamsManager instance = new TeamsManager ();

    public static TeamsManager Instance
    {
        get
        {
            return instance;
        }
    }

    public int numberOfTeams = 0;
    private List<Team> teams = new List<Team> ();
    private HashSet<int> teamsIds = new HashSet<int> ();

    /// <summary>
    /// Adds a team for TeamManager to be aware of, assigning it a unique id
    /// </summary>
    /// <param name="team">Team to add</param>
    public void AddTeam (Team team)
    {
        if (team == null || teamsIds.Contains (team.id)) return;

        int newId = 0;
        while (teamsIds.Contains (newId)) newId++;
        team.id = newId;

        teamsIds.Add (newId);
        teams.Insert (newId, team);
        numberOfTeams++;
    }
    public Team GetTeamByIndex (int index)
    {
        return teams[index];
    }

}