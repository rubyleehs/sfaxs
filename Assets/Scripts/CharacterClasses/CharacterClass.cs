using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterClass : ScriptableObject
{
    public GameObject model;
    public int hp;
    public int moveRange;
    public HashSet<TerrainType> navigatableTerrain;
}