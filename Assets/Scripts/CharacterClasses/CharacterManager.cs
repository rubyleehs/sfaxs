/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;
    public List<CharacterClass> availableCharacterClasses;
    public static Dictionary<string, CharacterClass> characterClasses;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        characterClasses = new Dictionary<string, CharacterClass>();
        foreach (CharacterClass c in availableCharacterClasses){
            characterClasses.Add(c.name, c);
        }
    }

    public static Character CreateCharacter(string characterClass,){

    }
}



 */