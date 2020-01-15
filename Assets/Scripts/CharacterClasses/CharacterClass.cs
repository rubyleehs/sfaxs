using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterClass")]
public class CharacterClass : ScriptableObject
{
    public int hp;
    public float moveRange, moveSpeed = 0.5f;
    public float inclinedMovementEffortMultiplier;
    public bool willSink;

    [SerializeField] private List<TerrainFloatStrut> I_navigatableTerrainWeightages; //Only so it is editable from unity inspector, no input validation
    [SerializeField] private List<TerrainType> I_unnavigatableTerrain;

    public Dictionary<TerrainType, float> navigatableTerrainWeightage;
    public HashSet<TerrainType> unnavigableTerrain;

    public void SetupCharacterClass()
    {
        navigatableTerrainWeightage = new Dictionary<TerrainType, float>();
        foreach (TerrainFloatStrut i in I_navigatableTerrainWeightages)
        {
            navigatableTerrainWeightage.Add(i.type, i.value);
        }

        unnavigableTerrain = new HashSet<TerrainType>();
        foreach (TerrainType i in I_unnavigatableTerrain)
        {
            unnavigableTerrain.Add(i);
        }
    }
}

[System.Serializable]
public struct TerrainFloatStrut
{
    public TerrainType type;
    public float value;
}