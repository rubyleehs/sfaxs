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
                Debug.Log(currentNode.indexPosition);
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
        if (nodesInRange == null) nodesInRange = Pathfinder.IsInRange(EnvironmentTerrainGenerator.allNodes, currentNode, characterClass.moveRange, PathfindingD);
        if (nodesInRange.Contains(n)) GoTo(Pathfinder.Solve(EnvironmentTerrainGenerator.allNodes, currentNode, n, PathfindingD, PathfindingH));

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
        if (nodesInRange == null) nodesInRange = Pathfinder.IsInRange(EnvironmentTerrainGenerator.allNodes, currentNode, characterClass.moveRange, PathfindingD);
        EnviromentProjectorManager.instance.RefreshMovementRangeProjection(nodesInRange, currentNode);
    }
    public void OnDeselect()
    {
        EnviromentProjectorManager.instance.RefreshMovementRangeProjection(null, currentNode.indexPosition);
        Debug.Log("Deselected!");
    }
}