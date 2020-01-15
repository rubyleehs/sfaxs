using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//TODO. SPAWN PLAYER
public class Character : MonoBehaviour, IPointerClickHandler, ISelectable
{
    public CharacterClass characterClass;
    public EnvironmentNode currentNode { get; set; }
    public int teamId { get; set; } = -1;
    public int currentHp { get; set; }

    public HashSet<EnvironmentNode> nodesInRange;

    public void InitCharacter(EnvironmentNode startNode, int teamId = -1)
    {
        if (characterClass.unnavigableTerrain == null) characterClass.SetupCharacterClass();
        currentNode = startNode;
        this.teamId = teamId;
        currentHp = characterClass.hp;

        transform.position = currentNode.position;

        NotifySelectionManager();
    }


    private IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            Vector3 temp = destination - position;
            temp.y = 0;
            transform.rotation = Quaternion.LookRotation(temp, Vector3.up);

            temp = transform.position;
            float t = 0.0f;
            float movePeriod = Vector3.Distance(position, destination) / characterClass.moveSpeed;

            while (t < movePeriod)
            {
                t += Time.deltaTime;
                temp = Vector3.Lerp(position, destination, t / movePeriod);
                transform.position = temp;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo(List<EnvironmentNode> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = currentNode.position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].position;
                yield return DoMove(position, next);
                currentNode = route[count];
                position = next;
            }
        }
    }

    public void GoTo(List<EnvironmentNode> route)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route));

        nodesInRange = null;
    }

    public void TryGoTo(EnvironmentNode n)
    {
        if (nodesInRange == null) nodesInRange = Pathfinder.IsInRange(EnvironmentManager.allNodes, currentNode, characterClass.moveRange, PathfindingD);
        if (nodesInRange.Contains(n)) GoTo(Pathfinder.Solve(EnvironmentManager.allNodes, currentNode, n, PathfindingD, PathfindingH));

        SelectionManager.instance.Deselect();
    }

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
    public void OnPointerClick(PointerEventData eventData)
    {
        SelectionManager.instance.Select(this);
    }
    public void NotifySelectionManager()
    {
        SelectionManager.instance.AddToSelection(this);
    }
    public void OnSelect()
    {
        Debug.Log("Selected!");
        if (nodesInRange == null) nodesInRange = Pathfinder.IsInRange(EnvironmentManager.allNodes, currentNode, characterClass.moveRange, PathfindingD);
        EnvironmentManager.instance.RefreshMovementRangeProjection(nodesInRange, currentNode);
    }
    public void OnDeselect()
    {
        EnvironmentManager.instance.RefreshMovementRangeProjection(null, currentNode.indexPosition);
        Debug.Log("Deselected!");
    }
}