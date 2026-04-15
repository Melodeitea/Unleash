using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class WallComponent : MonoBehaviour
{
    public List<Vector3> worldPoints = new List<Vector3>();
    public float height = 2.5f;
    public float thickness = 0.2f;

    List<GameObject> spawnedSegments = new List<GameObject>();

#if UNITY_EDITOR
    bool rebuildQueued = false;
#endif

    void OnEnable()
    {
        Rebuild();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!rebuildQueued)
        {
            rebuildQueued = true;
            EditorApplication.delayCall += () =>
            {
                rebuildQueued = false;
                if (this != null)
                    Rebuild();
            };
        }
#endif
    }

    public void Rebuild()
    {
        ClearSegments();

        if (worldPoints == null || worldPoints.Count < 2)
            return;

        for (int i = 0; i < worldPoints.Count - 1; i++)
        {
            Vector3 a = worldPoints[i];
            Vector3 b = worldPoints[i + 1];

            Vector3 dir = b - a;
            dir.y = 0;

            float length = dir.magnitude;
            if (length < 0.001f) continue;

            Vector3 mid = (a + b) * 0.5f;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "WallSegment";
            cube.transform.SetParent(transform);

#if UNITY_EDITOR
            if (cube.GetComponent<Collider>() != null)
                DestroyImmediate(cube.GetComponent<Collider>());
#else
            Destroy(cube.GetComponent<Collider>());
#endif

            cube.transform.position = mid;
            cube.transform.rotation = Quaternion.LookRotation(dir);
            cube.transform.localScale = new Vector3(thickness, height, length);

            spawnedSegments.Add(cube);
        }

        ApplyBottomLeftPivot();
    }

    void ClearSegments()
    {
        for (int i = spawnedSegments.Count - 1; i >= 0; i--)
        {
            if (spawnedSegments[i] != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(spawnedSegments[i]);
#else
                Destroy(spawnedSegments[i]);
#endif
            }
        }
        spawnedSegments.Clear();
    }

    Vector3 ComputeBottomLeft()
    {
        if (worldPoints == null || worldPoints.Count == 0)
            return transform.position;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        foreach (var p in worldPoints)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.z < minZ) minZ = p.z;
        }

        return new Vector3(minX, minY, minZ);
    }

    void ApplyBottomLeftPivot()
    {
        Vector3 pivot = ComputeBottomLeft();

        Vector3 delta = pivot - transform.position;
        transform.position = pivot;

        foreach (var seg in spawnedSegments)
        {
            if (seg != null)
                seg.transform.position -= delta;
        }
    }

    public void UpdateMeshFromWorldPoints()
    {
        Rebuild();
    }
}