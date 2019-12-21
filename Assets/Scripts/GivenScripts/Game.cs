using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Character character;
    [SerializeField] private Canvas menu;
    [SerializeField] private Canvas hud;
    [SerializeField] private Transform characterStart;

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Environment mMap;

    private readonly int numberOfRaycastHits = 1;

    void Start ()
    {
        mRaycastHits = new RaycastHit[numberOfRaycastHits];
        mMap = GetComponentInChildren<Environment> ();
        mCharacter = Instantiate (character, transform);
        TeamsManager.Instance.AddTeam (new Team (new List<Character> () { mCharacter }));
        ShowMenu (true);
    }

    private void Update ()
    {
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if (Input.GetMouseButtonDown (0))
        {
            Ray screenClick = mainCamera.ScreenPointToRay (Input.mousePosition);
            int hits = Physics.RaycastNonAlloc (screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile> ();
                if (tile != null)
                {
                    List<EnvironmentTile> route = mMap.Solve (mCharacter.currentPosition, tile);
                    mCharacter.GoTo (route);
                }
            }
        }
    }

    public void ShowMenu (bool show)
    {
        if (menu != null && hud != null)
        {
            menu.enabled = show;
            hud.enabled = !show;

            if (show)
            {
                mCharacter.transform.position = characterStart.position;
                mCharacter.transform.rotation = characterStart.rotation;
                mMap.CleanUpWorld ();
            }
            else
            {
                mCharacter.transform.position = mMap.Start.position;
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.currentPosition = mMap.Start;
            }
        }
    }

    public void Generate ()
    {
        mMap.GenerateWorld ();
    }

    public void Exit ()
    {
#if !UNITY_EDITOR
        Application.Quit ();
#endif
    }
}