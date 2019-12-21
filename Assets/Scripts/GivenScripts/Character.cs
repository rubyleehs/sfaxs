using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Changed to abstract + formatting. Added teamId property
public class Character : MonoBehaviour
{
    [SerializeField] private float singleNodeMoveTime = 0.5f;
    public CharacterClass characterClass;

    public EnvironmentTile currentPosition { get; set; }
    public int teamId { get; set; } = -1;
    public int currentHp { get; set; }

    private void Awake ()
    {
        currentHp = characterClass.hp;
    }
    private IEnumerator DoMove (Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation (destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;

            while (t < singleNodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp (position, destination, t / singleNodeMoveTime);
                transform.position = p;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo (List<EnvironmentTile> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = currentPosition.position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].position;
                yield return DoMove (position, next);
                currentPosition = route[count];
                position = next;
            }
        }
    }

    public void GoTo (List<EnvironmentTile> route)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines ();
        StartCoroutine (DoGoTo (route));
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
}