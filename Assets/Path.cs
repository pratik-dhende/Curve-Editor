using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path {
    [SerializeField]
    public List<Vector2> points;
    bool isPathClosed = false;
    bool isAutoSetControlPoints = true;

    // Constructor
    public Path(Vector2 center)
    {
        points = new List<Vector2>
        {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + (Vector2.right + Vector2.down) * 0.5f,
            center + Vector2.right
        };
    }

    // Getters.
    public int numSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public int numPoints
    {
        get
        {
            return points.Count;
        }
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public bool PathClosed
    {
        get
        {
            return isPathClosed;
        }
        set
        {
            if (isPathClosed != value)
            {
                isPathClosed = value;
                togglePathClosed();
            }
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return isAutoSetControlPoints;
        }
        set
        {
            if (isAutoSetControlPoints != value)
            {
                isAutoSetControlPoints = value;

                if (isAutoSetControlPoints)
                {
                    autoSetAllControlPoints();
                }
            }
        }
    }

    // Path Functions.
    public void addNewSegement(Vector2 anchorPosition)
    {
        points.Add((2 * points[points.Count - 1]) - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPosition) * 0.5f);
        points.Add(anchorPosition);

        if (isAutoSetControlPoints)
        {
            autoSetAffectedControlPoints(points.Count - 1);
        }
    }

    public void splitSegment(Vector2 splitPos, int segmentIndex)
    {
        points.InsertRange((segmentIndex * 3) + 2, new Vector2[] { Vector2.zero, splitPos, Vector2.zero });
        if (isPathClosed)
        {
            autoSetAffectedControlPoints((segmentIndex * 3) + 3);
        }
        else
        {
            autoSetControlPoint((segmentIndex * 3) + 3);
        }
    }

    public void deleteSegment(int anchorIndex)
    {
        if (isAutoSetControlPoints && numSegments >= 2 || numSegments >= 1)
        {
            if (anchorIndex == 0)
            {
                if (isPathClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isPathClosed)
            {
                points.RemoveRange(points.Count - 2, 2);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector2[] getPointsOfSegment(int i)
    {
        return new Vector2[] { points[(3 * i) + 0], points[(3 * i) + 1], points[(3 * i) + 2], points[loopOver((3 * i) + 3)] };
    }

    public Vector2[] getEvenlySpacedPoints(float spacing, float resolution = 1f)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);

        Vector2 previousPoint = points[0];

        float distSinceLastEvenPoint = 0;

        for (int i = 0; i < numSegments; i++)
        {
            Vector2[] segment = getPointsOfSegment(i);

            float netControlLength = Vector2.Distance(segment[0], segment[1]) + Vector2.Distance(segment[1], segment[2]) + Vector2.Distance(segment[2], segment[3]);
            float segmentLength = Vector2.Distance(segment[0], segment[3]) + (netControlLength * 0.5f);

            int divisions = Mathf.CeilToInt(segmentLength * resolution * 10);

            float t = 0;
            while(t <= 1)
            {   
                t += 1f / divisions;

                Vector2 pointOnSegment = Bezier.cubicCurve(segment[0], segment[1], segment[2], segment[3], t);
                distSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnSegment);

                while(distSinceLastEvenPoint >= spacing)
                {
                    float overShootDist = distSinceLastEvenPoint - spacing;

                    Vector2 newEvenlySpacedPoint = pointOnSegment + (previousPoint - pointOnSegment).normalized * overShootDist;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);

                    previousPoint = newEvenlySpacedPoint;
                    distSinceLastEvenPoint = overShootDist;
                }
                previousPoint = pointOnSegment;
            }
        }
        return evenlySpacedPoints.ToArray();
    }

    public void movePoint(int i, Vector2 newPos)
    {
        if (i % 3 == 0 || !isAutoSetControlPoints)
        {
            Vector2 displacement = newPos - points[i];
            points[i] = newPos;
            if (isAutoSetControlPoints)
            {
                autoSetAffectedControlPoints(i);
            }
            else
            {
                if (i % 3 == 0)
                {
                    if (i + 1 < points.Count)
                    {
                        points[i + 1] = points[i + 1] + displacement;
                    }
                    if (i - 1 >= 0 || isPathClosed)
                    {
                        points[loopOver(i - 1)] = points[loopOver(i - 1)] + displacement;
                    }
                }
                else
                {
                    bool isNextPointAnchor = loopOver(i + 1) % 3 == 0;

                    int correspondingControlIndex = isNextPointAnchor ? i + 2 : i - 2;
                    int anchorPointIndex = isNextPointAnchor ? i + 1 : i - 1;

                    if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isPathClosed)
                    {
                        Vector2 correspondingControlPoint = points[loopOver(correspondingControlIndex)];
                        Vector2 anchorPoint = points[loopOver(anchorPointIndex)];

                        float dstBetweenAnchorAndControlPoint = (correspondingControlPoint - anchorPoint).magnitude;
                        Vector2 directionFromAnchorToNewPos = (anchorPoint - newPos).normalized;

                        points[loopOver(correspondingControlIndex)] = anchorPoint + (dstBetweenAnchorAndControlPoint * directionFromAnchorToNewPos);
                    }
                }
            }
        }
    }

    // Button Functions.
    void togglePathClosed()
    {
        if (isPathClosed)
        {
            points.Add((2 * points[points.Count - 1]) - points[points.Count - 2]);
            points.Add((2 * points[0]) - points[1]);

            if (isAutoSetControlPoints)
            {
                autoSetAffectedControlPoints(0);
                autoSetAffectedControlPoints(points.Count - 3);
            }
        }
        else
        {
            points.RemoveRange(points.Count - 2, 2);

            if (isAutoSetControlPoints)
            {
                autoSetFirstAndLastControlPoint();
            }
        }
    }

    void autoSetAffectedControlPoints(int updatedAnchorIndex)
    {
        for(int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {   
            autoSetControlPoint(loopOver(i));
        }
        autoSetFirstAndLastControlPoint();
    }

    void autoSetAllControlPoints()
    {
        for(int i = 0; i < points.Count; i += 3)
        {
            autoSetControlPoint(i);
        }
        autoSetFirstAndLastControlPoint();
    }

    // Utility Functions.
    int loopOver(int i)
    {
        return (i + points.Count) % points.Count;
    }

    void autoSetControlPoint(int i)
    {   
        if ((i - 3 >= 0 && i + 3 < points.Count) || isPathClosed)
        {
            Vector2 nextAnchor = points[loopOver(i + 3)];
            Vector2 previousAnchor = points[loopOver(i - 3)];

            Vector2 currentToNextAnchor = (nextAnchor - points[i]);
            Vector2 currentToPreviousAnchor = (previousAnchor - points[i]);

            Vector2 dirOfControlPoints = (currentToPreviousAnchor.normalized - currentToNextAnchor.normalized).normalized;

            points[loopOver(i - 1)] = points[i] + ((currentToPreviousAnchor.magnitude * 0.5f) * dirOfControlPoints);
            points[loopOver(i + 1)] = points[i] + ((currentToNextAnchor.magnitude * 0.5f) * (-dirOfControlPoints));
        }
    }

    void autoSetFirstAndLastControlPoint()
    {
        if (!isPathClosed)
        {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }
}
