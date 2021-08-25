using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public Color anchorColor = Color.red;
    public Color controlPointColor = Color.white;
    public Color curveColor = Color.green;
    public Color handlesColor = Color.black;
    public Color selectedSegmentColor = Color.yellow;

    public float anchorDiameter = 0.1f;
    public float controlPointDiameter = 0.1f;
    public float curveWidth = 2f;

    public bool showControlPoints = false;

    public void createPath()
    {
        path = new Path(transform.position);
    }

    private void Reset()
    {
        createPath();
    }
}
