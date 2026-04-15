using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[Serializable]
public class PlacedObjectData
{
	public string prefabPath;
	public Vector3 position;
	public Vector3 euler;
	public Vector3 scale;
	public string puzzleId;
	public bool isSolved;
}

[Serializable]
public class LayoutData
{
	public string name;
	public List<PlacedObjectData> objects = new List<PlacedObjectData>();
}

public class LevelDesignerWindow : EditorWindow
{
	const string LayoutsFolder = "Assets/LevelLayouts";

	GameObject _prefab;
	bool _placeMode;
	bool _snapToGrid = true;
	float _gridSize = 1f;
	Vector2 _scroll;
	string _layoutName = "NewLayout";

	// runtime list of placed instances created via this tool
	List<GameObject> _placedInstances = new List<GameObject>();

	[MenuItem("Tools/Level Designer")]
	public static void ShowWindow()
	{
		var wnd = GetWindow<LevelDesignerWindow>();
		wnd.titleContent = new GUIContent("Level Designer");
		wnd.Focus();
	}

	void OnEnable()
	{
		SceneView.duringSceneGui += OnSceneGUI;
		RefreshPlacedInstances();
		Directory.CreateDirectory(LayoutsFolder);
	}

	void OnDisable()
	{
		SceneView.duringSceneGui -= OnSceneGUI;
	}

	void OnGUI()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
		_prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);

		EditorGUILayout.BeginHorizontal();
		_placeMode = GUILayout.Toggle(_placeMode, "Place Mode", "Button");
		if (GUILayout.Button("Clear Placed"))
		{
			ClearPlacedInstances();
		}
		EditorGUILayout.EndHorizontal();

		_snapToGrid = EditorGUILayout.Toggle("Snap To Grid", _snapToGrid);
		_gridSize = EditorGUILayout.FloatField("Grid Size", Mathf.Max(0.01f, _gridSize));

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
		_layoutName = EditorGUILayout.TextField("Layout Name", _layoutName);

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Save Layout"))
		{
			SaveLayout();
		}
		if (GUILayout.Button("Load Layout"))
		{
			string path = EditorUtility.OpenFilePanel("Load Layout JSON", Application.dataPath, "json");
			if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
			{
				path = "Assets" + path.Substring(Application.dataPath.Length);
				LoadLayout(path);
			}
			else if (!string.IsNullOrEmpty(path))
			{
				EditorUtility.DisplayDialog("Invalid path", "Please choose a file inside this project's Assets folder.", "OK");
			}
		}
		EditorGUILayout.EndHorizontal();

		if (GUILayout.Button("Quick Playtest"))
		{
			EditorApplication.isPlaying = true;
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Placed Objects", EditorStyles.boldLabel);
		_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(200));
		for (int i = 0; i < _placedInstances.Count; i++)
		{
			var go = _placedInstances[i];
			if (go == null) continue;
			EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
			EditorGUILayout.ObjectField(go, typeof(GameObject), true);

			// show a 'Ping' button
			if (GUILayout.Button("Ping", GUILayout.Width(40)))
			{
				EditorGUIUtility.PingObject(go);
			}

			// mark / unmark solved if Puzzle exists
			var puzzle = go.GetComponentInChildren<Puzzle>();
			if (puzzle != null)
			{
				bool solved = puzzle.isSolved;
				bool newSolved = GUILayout.Toggle(solved, "Solved", "Button", GUILayout.Width(60));
				if (newSolved != solved)
				{
					if (newSolved)
					{
						puzzle.isSolved = true;
						// call method to apply visuals if present
						puzzle.SendMessage("OnApplySolvedState", SendMessageOptions.DontRequireReceiver);
						if (!string.IsNullOrEmpty(puzzle.puzzleId) && PuzzleManager.Instance != null)
							PuzzleManager.Instance.MarkSolved(puzzle.puzzleId);
					}
					else
					{
						puzzle.isSolved = false;
						// no standard Unsolve flow; rely on component implementation
					}
				}
			}
			else
			{
				GUILayout.Label("", GUILayout.Width(64));
			}

			if (GUILayout.Button("Remove", GUILayout.Width(70)))
			{
				Undo.DestroyObjectImmediate(go);
				_placedInstances.RemoveAt(i);
				i--;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}

	void OnSceneGUI(SceneView sv)
	{
		if (!_placeMode || _prefab == null) return;

		var e = Event.current;
		// place on left mouse down, not when clicking UI
		if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !e.control && !e.shift)
		{
			Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
			{
				Vector3 pos = hit.point;
				PlacePrefabAt(pos);
				e.Use();
			}
			else
			{
				// fallback to plane at y = 0
				Plane p = new Plane(Vector3.up, Vector3.zero);
				if (p.Raycast(ray, out float enter))
				{
					Vector3 pos = ray.GetPoint(enter);
					PlacePrefabAt(pos);
					e.Use();
				}
			}
		}

		// draw simple instruction
		Handles.BeginGUI();
		GUILayout.BeginArea(new Rect(10, 10, 300, 60), EditorStyles.helpBox);
		GUILayout.Label("Level Designer: Click in Scene to place prefab. Toggle Place Mode to disable.");
		GUILayout.EndArea();
		Handles.EndGUI();
	}

	void PlacePrefabAt(Vector3 pos)
	{
		Vector3 worldPos = pos;
		if (_snapToGrid)
		{
			worldPos.x = Mathf.Round(worldPos.x / _gridSize) * _gridSize;
			worldPos.y = Mathf.Round(worldPos.y / _gridSize) * _gridSize;
			worldPos.z = Mathf.Round(worldPos.z / _gridSize) * _gridSize;
		}

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, EditorSceneManager.GetActiveScene());
		if (instance == null)
		{
			// fallback to simple Instantiate if PrefabUtility fails
			instance = (GameObject)Instantiate(_prefab);
			instance.name = _prefab.name;
			EditorSceneManager.MoveGameObjectToScene(instance, EditorSceneManager.GetActiveScene());
		}

		Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");
		instance.transform.position = worldPos;
		instance.transform.rotation = Quaternion.identity;
		PlacedInstancePostProcess(instance);

		_placedInstances.Add(instance);
		Selection.activeObject = instance;
		EditorSceneManager.MarkAllScenesDirty();
	}

	void PlacedInstancePostProcess(GameObject go)
	{
		// If object has a Puzzle component with a puzzleId, and the PuzzleManager has it solved, apply visuals
		var puzzle = go.GetComponentInChildren<Puzzle>();
		if (puzzle != null && !string.IsNullOrEmpty(puzzle.puzzleId) && PuzzleManager.Instance != null)
		{
			if (PuzzleManager.Instance.IsSolved(puzzle.puzzleId))
			{
				puzzle.isSolved = true;
				puzzle.SendMessage("OnApplySolvedState", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	void ClearPlacedInstances()
	{
		for (int i = _placedInstances.Count - 1; i >= 0; i--)
		{
			var go = _placedInstances[i];
			if (go != null)
				Undo.DestroyObjectImmediate(go);
		}
		_placedInstances.Clear();
	}

	void RefreshPlacedInstances()
	{
		_placedInstances.Clear();
		// Heuristic: treat objects with PrefabAssetType != NotAPrefab or that contain a component called 'PlacedByLevelDesigner' as ours.
		// Simpler approach: find all root objects and add those that have a tag "LevelDesignerPlaced" or name match — but since we don't require tag, we'll keep runtime tracking.
		// On enable we only track existing selected objects that have a Puzzle (best-effort)
		var all = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (var go in all)
		{
			if (PrefabUtility.IsPartOfAnyPrefab(go))
			{
				// do not auto-populate to avoid duplicating editor actions
			}
		}
	}

	void SaveLayout()
	{
		if (string.IsNullOrEmpty(_layoutName)) _layoutName = "NewLayout";
		var layout = new LayoutData { name = _layoutName };

		// find instances we created: use the tracked list
		foreach (var go in _placedInstances)
		{
			if (go == null) continue;
			var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
			if (string.IsNullOrEmpty(prefabPath))
				prefabPath = "";

			var pd = new PlacedObjectData
			{
				prefabPath = prefabPath,
				position = go.transform.position,
				euler = go.transform.eulerAngles,
				scale = go.transform.localScale
			};
			var puzzle = go.GetComponentInChildren<Puzzle>();
			if (puzzle != null)
			{
				pd.puzzleId = puzzle.puzzleId;
				pd.isSolved = puzzle.isSolved;
			}
			layout.objects.Add(pd);
		}

		string json = JsonUtility.ToJson(layout, true);
		string folder = LayoutsFolder;
		if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
		string path = Path.Combine(folder, _layoutName + ".json");
		File.WriteAllText(path, json);
		AssetDatabase.ImportAsset(path);
		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Layout saved", $"Layout saved to {path}", "OK");
	}

	void LoadLayout(string assetPath)
	{
		if (!assetPath.StartsWith("Assets"))
		{
			EditorUtility.DisplayDialog("Load error", "Layout must be inside Assets folder.", "OK");
			return;
		}

		string json = File.ReadAllText(assetPath);
		var layout = JsonUtility.FromJson<LayoutData>(json);
		if (layout == null)
		{
			EditorUtility.DisplayDialog("Load error", "Failed to parse layout JSON.", "OK");
			return;
		}

		ClearPlacedInstances();

		foreach (var pd in layout.objects)
		{
			GameObject prefab = null;
			if (!string.IsNullOrEmpty(pd.prefabPath))
			{
				prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pd.prefabPath);
			}
			if (prefab == null)
			{
				Debug.LogWarning($"Prefab not found at path '{pd.prefabPath}'. Creating empty GameObject instead.");
				var go = new GameObject("MissingPrefab");
				go.transform.position = pd.position;
				go.transform.eulerAngles = pd.euler;
				go.transform.localScale = pd.scale;
				Undo.RegisterCreatedObjectUndo(go, "Instantiate missing");
				_placedInstances.Add(go);
			}
			else
			{
				GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, EditorSceneManager.GetActiveScene());
				if (instance == null)
				{
					instance = (GameObject)Instantiate(prefab);
					instance.name = prefab.name;
					EditorSceneManager.MoveGameObjectToScene(instance, EditorSceneManager.GetActiveScene());
				}
				Undo.RegisterCreatedObjectUndo(instance, "Instantiate prefab from layout");
				instance.transform.position = pd.position;
				instance.transform.eulerAngles = pd.euler;
				instance.transform.localScale = pd.scale;

				// apply puzzle solved state if present
				var puzzle = instance.GetComponentInChildren<Puzzle>();
				if (puzzle != null)
				{
					if (!string.IsNullOrEmpty(pd.puzzleId))
						puzzle.puzzleId = pd.puzzleId;
					puzzle.isSolved = pd.isSolved;
					if (pd.isSolved)
					{
						puzzle.SendMessage("OnApplySolvedState", SendMessageOptions.DontRequireReceiver);
						if (PuzzleManager.Instance != null && !string.IsNullOrEmpty(puzzle.puzzleId))
							PuzzleManager.Instance.MarkSolved(puzzle.puzzleId);
					}
				}

				_placedInstances.Add(instance);
			}
		}

		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		EditorUtility.DisplayDialog("Layout loaded", $"Loaded layout '{layout.name}'", "OK");
	}
}