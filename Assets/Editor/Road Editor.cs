using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadCreator))]
public class RoadEditor : Editor
{
    RoadCreator creator;

    private void OnSceneGUI()
    {
        if (creator.autoUpdateRoad && Event.current.type == EventType.Repaint)
        {
            creator.updateRoad();
        }
    }

    private void OnEnable()
    {
        creator = target as RoadCreator;
    }
}
