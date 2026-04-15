using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

/// <summary>
/// Simple build-mode EditorWindow: toolbar, draw-wall mode (click to place vertices), grid snapping, preview polyline,
/// finalize to create a Wall GameObject using WallComponent; vertex edit mode with position handles.
/// </summary>
public class BuildModeWindow : EditorWindow
{
    const string TOOL_TITLE = "Build Mode";
    float toolbarHeight = 22f;

    enum Mode { Select, Eyedropper, MoveRoom, DrawWall, EditVertices }
    Mode currentMode = Mode.Select;

    // draw-mode state
    List<Vector3> currentPoints = new List<Vector3>();
    bool snapToGrid = true;
    float gridSize = 1f;
    bool useSceneOrientation = true;

    Vector2 scroll;

    [MenuItem("Tools/Build Mode")]
    public static void ShowWindow()
    {
        GetWindow<BuildModeWindow>(false, TOOL_TITLE, true).Show();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Toggle(currentMode == Mode.Select, "Arrow", EditorStyles.toolbarButton)) currentMode = Mode.Select;
        if (GUILayout.Toggle(currentMode == Mode.Eyedropper, "Eyedropper", EditorStyles.toolbarButton)) currentMode = Mode.Eyedropper;
        if (GUILayout.Toggle(currentMode == Mode.MoveRoom, "Crossed Arrows", EditorStyles.toolbarButton)) currentMode = Mode.MoveRoom;
        if (GUILayout.Toggle(currentMode == Mode.DrawWall, "Draw Wall", EditorStyles.toolbarButton)) currentMode = Mode.DrawWall;
        if (GUILayout.Toggle(currentMode == Mode.EditVertices, "Edit Vertices", EditorStyles.toolbarButton)) currentMode = Mode.EditVertices;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Draw Wall Settings", EditorStyles.boldLabel);
        snapToGrid = EditorGUILayout.Toggle("Snap To Grid", snapToGrid);
        gridSize = EditorGUILayout.FloatField("Grid Size", Mathf.Max(0.01f, gridSize));
        useSceneOrientation = EditorGUILayout.Toggle("Use Scene Orientation (XZ plane)", useSceneOrientation);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Finish Wall") && currentPoints.Count >= 2)
        {
            FinalizeWall();
        }
        if (GUILayout.Button("Clear Points"))
        {
            currentPoints.Clear();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Pick Selected Wall"))
        {
            // copy selected wall's points to currentPoints if it has WallComponent
            var sel = Selection.activeGameObject;
            if (sel != null)
            {
                var w = sel.GetComponent<WallComponent>();
                if (w != null)
                {
                    currentPoints = new List<Vector3>(w.worldPoints);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Points: " + currentPoints.Count, EditorStyles.miniLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(80));
        for (int i = 0; i < currentPoints.Count; i++)
        {
            EditorGUILayout.Vector3Field($"P{i}", currentPoints[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    void OnSceneGUI(SceneView sv)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        // place point on XZ plane at y=0 or on hit collider
        if (currentMode == Mode.DrawWall)
        {
            // show existing polyline
            if (currentPoints.Count > 0)
            {
                Handles.color = Color.green;
                for (int i = 0; i < currentPoints.Count - 1; i++)
                {
                    Handles.DrawLine(currentPoints[i], currentPoints[i + 1]);
                }
            }

            // preview point under mouse
            Vector3 previewPos;
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                previewPos = hit.point;
            }
            else
            {
                Plane p = new Plane(Vector3.up, Vector3.zero);
                p.Raycast(ray, out float enter);
                previewPos = ray.GetPoint(enter);
            }

            if (snapToGrid) previewPos = Snap(previewPos, gridSize);

            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(previewPos, Vector3.up, gridSize * 0.1f);

            if (currentPoints.Count > 0)
            {
                Handles.color = Color.cyan;
                Handles.DrawLine(currentPoints[currentPoints.Count - 1], previewPos);
            }

            // click to add point (left mouse down)
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !e.control && !e.shift)
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Add Wall Point");

                currentPoints.Add(previewPos);
                e.Use();
                SceneView.RepaintAll();
            }
        }

        // vertex edit mode: if a wall selected, draw handles for each vertex and allow dragging
        if (currentMode == Mode.EditVertices)
        {
            GameObject sel = Selection.activeGameObject;
            if (sel != null)
            {
                var w = sel.GetComponent<WallComponent>();
                if (w != null)
                {
                    // present handles for each world point
                    for (int i = 0; i < w.worldPoints.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 p = w.worldPoints[i];
                        Vector3 newP = Handles.PositionHandle(p, Quaternion.identity);
                        if (snapToGrid) newP = Snap(newP, gridSize);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(w, "Move Wall Vertex");
                            w.worldPoints[i] = newP;
                            w.UpdateMeshFromWorldPoints();
                            EditorSceneManager.MarkSceneDirty(w.gameObject.scene);
                        }

                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0, p, Quaternion.identity, gridSize * 0.15f, EventType.Repaint);
                    }
                }
            }
        }

        // minimal Select / MoveRoom / Eyedropper behaviors are left as placeholders for future extension.
    }

    Vector3 Snap(Vector3 v, float grid)
    {
        return new Vector3(
            Mathf.Round(v.x / grid) * grid,
            Mathf.Round(v.y / grid) * grid,
            Mathf.Round(v.z / grid) * grid
        );
    }

    void FinalizeWall()
    {
        if (currentPoints.Count < 2) return;

        GameObject go = new GameObject("Wall");
        Undo.RegisterCreatedObjectUndo(go, "Create Wall");

        var wc = go.AddComponent<WallComponent>();

        wc.worldPoints = new List<Vector3>(currentPoints);
        wc.height = 2.5f;
        wc.thickness = 0.2f;

        wc.Rebuild();

        EditorSceneManager.MarkSceneDirty(go.scene);

        Selection.activeGameObject = go;

        currentPoints.Clear();
        SceneView.RepaintAll();
    }
}