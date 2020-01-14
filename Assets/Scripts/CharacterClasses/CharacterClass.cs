using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterClass")]
public class CharacterClass : ScriptableObject
{
    public int hp;
    public float moveRange, movePeriod = 0.5f;
    public float inclinedMovementEffortMultiplier;
    public Dictionary<TerrainType, float> navigatableTerrainWeightage;
    public HashSet<TerrainType> unnavigableTerrain;
}