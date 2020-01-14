using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO. SPAWN PLAYER
public class Character : MonoBehaviour
{
    public CharacterClass characterClass;
    public EnvironmentTile currentPosition { get; set; }
    public int teamId { get; set; } = -1;
    public int currentHp { get; set; }

    private void Awake()
    {
        //currentHp = characterClass.hp;
    }
    private IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;

            while (t < characterClass.movePeriod)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / characterClass.movePeriod);
                transform.position = p;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = currentPosition.position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].position;
                yield return DoMove(position, next);
                currentPosition = route[count];
                position = next;
            }
        }
    }

    public void GoTo(List<EnvironmentTile> route)
    {
        Debug.Log(route.Count);
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route));
    }

    //Should do Instantiation by game manager
    /*
    public static void InstantiateNewCharacter (CharacterClass characterClass, EnvironmentTile position, Team team)
    {
        Character character = new Character ();
        character.characterClass = characterClass;
        character.currentPosition = position;
        character.teamId = team.id;

        Instantiate
    }
    */

    public float PathfindingD(EnvironmentNode n1, EnvironmentNode n2)
    {
        float cost = Vector2.Distance(n1.indexPosition, n2.indexPosition);
        if (!n2.terrain.Contains(TerrainType.Water)) cost += (n2.effortWeightage - n1.effortWeightage) * characterClass.inclinedMovementEffortMultiplier;
        foreach (TerrainType terrainType in n2.terrain)
        {
            if (characterClass.unnavigableTerrain.Contains(terrainType)) return float.MaxValue;
            if (characterClass.navigatableTerrainWeightage.ContainsKey(terrainType)) cost *= characterClass.navigatableTerrainWeightage[terrainType];
        }
        return cost;
    }
    public float PathfindingH(EnvironmentNode n1, EnvironmentNode n2)
    {
        return Mathf.Sqrt(Vector2.SqrMagnitude(n1.indexPosition - n2.indexPosition) + Mathf.Pow(n2.effortWeightage - n1.effortWeightage, 2));
    }

}