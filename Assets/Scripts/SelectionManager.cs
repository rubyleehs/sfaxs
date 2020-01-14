using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;
    public List<ISelectable> selection;

    public ISelectable currentlySelected;

    private int? selectionIndex;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        selection = new List<ISelectable>();
    }

    public void Select(ISelectable o)
    {
        if (currentlySelected == o) Deselect();
        else
        {
            Deselect();
            currentlySelected = o;
            currentlySelected.OnSelect();
        }
    }

    public void Deselect()
    {
        if (currentlySelected != null) currentlySelected.OnDeselect();
        currentlySelected = null;
        selectionIndex = null;
    }

    public ISelectable SelectNext()
    {
        if (currentlySelected != null && !selectionIndex.HasValue) selectionIndex = selection.FindIndex((a) => a == currentlySelected);
        else return SelectIndex(0);

        return SelectIndex(selectionIndex.Value + 1);
    }

    public ISelectable SelectPrev()
    {
        if (currentlySelected != null && !selectionIndex.HasValue) selectionIndex = selection.FindIndex((a) => a == currentlySelected);
        else return SelectIndex(0);

        return SelectIndex(selectionIndex.Value - 1);
    }

    public ISelectable SelectIndex(int index)
    {
        int val = Mathf.Abs(index % selection.Count);
        Select(selection[val]);
        selectionIndex = val;

        return currentlySelected;
    }

    public void AddToSelection(ISelectable o)
    {
        if (!selection.Contains(o)) selection.Add(o);
    }
}
