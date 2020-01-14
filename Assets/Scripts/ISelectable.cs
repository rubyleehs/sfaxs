using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable
{
    void NotifySelectionManager();
    void OnSelect();
    void OnDeselect();
}
