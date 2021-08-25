using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path path
    {
        get
        {
            return creator.path;
        }
    }

    const float segmentAndMousePosThreshold = 0.01f;

    int selectedSegmentIndex = -1;

    private void OnSceneGUI()
    {
        input();
        draw();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if(GUILayout.Button("Create Path"))
        {
            creator.createPath();
        }

        bool isPathClosed = GUILayout.Toggle(path.PathClosed, "Closed Path");
        if(isPathClosed != path.PathClosed)
        {
            Undo.RecordObject(creator, "Toggle Closed Path");
            path.PathClosed = isPathClosed;
        }

        bool isAutoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if(isAutoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle Auto Set Control Points");
            path.AutoSetControlPoints = isAutoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void OnEnable()
    {
        creator = target as PathCreator;
        if(creator.path == null)
        {
            creator.createPath();
        }
    }

    void input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {   
            if (this.selectedSegmentIndex != -1)
            { 
                Undo.RecordObject(creator, "Split Segment");
                path.splitSegment(mousePos, this.selectedSegmentIndex);
            }
            else if (!path.PathClosed)
            {
                Undo.RecordObject(creator, "Add Segment");
                path.addNewSegement(mousePos);
            }
        }
        
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.shift)
        {
            int selectedAnchorIndex = -1;
            float minAnchorAndMousePosDst = creator.anchorDiameter * 0.5f;

            for(int i = 0; i < path.numPoints; i += 3)
            {
                float dst = Vector2.Distance(mousePos, path[i]);
                if (dst < minAnchorAndMousePosDst)
                {
                    minAnchorAndMousePosDst = dst;
                    selectedAnchorIndex = i;
                }
            }

            if (selectedAnchorIndex != -1)
            {
                Undo.RecordObject(creator, "Delete Segment");
                path.deleteSegment(selectedAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            int selectedSegmentIndex = -1;
            float minSegmentAndMousePosThreshold = segmentAndMousePosThreshold;

            for (int i = 0; i < path.numSegments; i++)
            {
                Vector2[] segment = path.getPointsOfSegment(i);
                float mousePosAndSegmentDistance = HandleUtility.DistancePointBezier(mousePos, segment[0], segment[3], segment[1], segment[2]);

                if (mousePosAndSegmentDistance < minSegmentAndMousePosThreshold)
                {
                    minSegmentAndMousePosThreshold = mousePosAndSegmentDistance;
                    selectedSegmentIndex = i;
                }
            }
            if (selectedSegmentIndex != this.selectedSegmentIndex)
            {
                this.selectedSegmentIndex = selectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        HandleUtility.AddDefaultControl(0);
    }

    void draw()
    {
        for (int i = 0; i < path.numPoints; i++)
        {
            if (i % 3 == 0 || creator.showControlPoints)
            {
                Handles.color = (i % 3 == 0) ? creator.anchorColor : creator.controlPointColor;
                float handleSize = (i % 3 == 0) ? creator.anchorDiameter : creator.controlPointDiameter;
                Vector2 newPos = Handles.FreeMoveHandle(path.points[i], Quaternion.identity, handleSize, Vector3.zero, Handles.CylinderHandleCap);
                if (newPos != path.points[i])
                {
                    Undo.RecordObject(creator, "Move Point");
                    path.movePoint(i, newPos);
                }
            }
        }

        for(int i = 0; i < path.numSegments; i++)
        {
            Vector2[] segments = path.getPointsOfSegment(i);

            if (creator.showControlPoints)
            {
                Handles.color = creator.handlesColor;
                Handles.DrawLine(segments[1], segments[0]);
                Handles.DrawLine(segments[2], segments[3]);
            }

            Color segmentColor = (i == this.selectedSegmentIndex && Event.current.shift) ? creator.selectedSegmentColor : creator.curveColor;
            Handles.DrawBezier(segments[0], segments[3], segments[1], segments[2], segmentColor, null, creator.curveWidth);
        }
    }
}
