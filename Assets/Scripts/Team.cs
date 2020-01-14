using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public int id = -1;
    public List<Character> members;
    public HashSet<int> enemyTeamsId;

    public Team(List<Character> members = null, HashSet<int> enemyTeamsId = null)
    {
        this.members = members ?? new List<Character>();
        this.enemyTeamsId = enemyTeamsId ?? new HashSet<int>();
        TeamsManager.instance.AddTeam(this);

        foreach (Character c in members)
        {
            c.teamId = id;
        }
    }

    public void AddMember(Character member)
    {
        members.Add(member);
        member.teamId = id;
    }

    public void AddEnemy(Team team, bool bothWays)
    {
        enemyTeamsId.Add(team.id);
        if (bothWays) team.enemyTeamsId.Add(this.id);
    }
}