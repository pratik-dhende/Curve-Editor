using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 0.1f;
    public float roadWidth = 1f;
    public bool autoUpdateRoad;

    public float tiling = 5f;

    public void updateRoad()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.getEvenlySpacedPoints(spacing);
        GetComponent<MeshFilter>().mesh = createRoadMesh(points, path.PathClosed);

        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    Mesh createRoadMesh(Vector2[] points, bool isPathColosed)
    {
        Vector3[] vertices = new Vector3[2 * points.Length];
        int numTriangles = (isPathColosed) ? (2 * (points.Length - 1)) + 2 : 2 * (points.Length - 1);
        int[] triangleIndexes = new int[numTriangles * 3];
        Vector2[] uvs = new Vector2[vertices.Length];

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1 || isPathColosed)
            {
                forward += points[(i + 1) % points.Length] - points[(i) % points.Length];
            }
            if (i > 0 || isPathColosed)
            {
                forward += points[(i) % points.Length] - points[(i - 1 + points.Length) % points.Length];
            }
            forward.Normalize();

            Vector2 left = new Vector2(-forward.y, forward.x);
            Vector2 right = -left;

            vertices[vertIndex] = points[i] + (left * roadWidth * 0.5f);
            vertices[vertIndex + 1] = points[i] + (right * roadWidth * 0.5f);

            float percentCompletion = i / (float)(points.Length - 1);
            float v = Mathf.Abs(2 * percentCompletion - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if (i < points.Length - 1 || isPathColosed)
            {
                triangleIndexes[triIndex] = vertIndex;
                triangleIndexes[triIndex + 1] = (vertIndex + 2) % vertices.Length;
                triangleIndexes[triIndex + 2] = vertIndex + 1;
                triangleIndexes[triIndex + 3] = vertIndex + 1;
                triangleIndexes[triIndex + 4] = (vertIndex + 2) % vertices.Length;
                triangleIndexes[triIndex + 5] = (vertIndex + 3) % vertices.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangleIndexes;
        mesh.uv = uvs;

        return mesh;
    }
}
