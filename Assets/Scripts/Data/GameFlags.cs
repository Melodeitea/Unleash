using System.Collections.Generic;
using UnityEngine;

public class GameFlags : MonoBehaviour
{
	public static GameFlags Instance { get; private set; }

	[SerializeField]
	private List<string> activeFlags = new();

	private readonly HashSet<string> flagSet = new();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		SyncRuntime();
	}

	// ------------------------
	// CORE
	// ------------------------

	public void SetFlag(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
			return;

		if (flagSet.Add(key))
		{
			activeFlags.Add(key);
			Debug.Log($"[GameFlags] Set: {key}");
		}
	}

	public bool GetFlag(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
			return false;

		return flagSet.Contains(key);
	}

	public bool HasFlag(string key) => GetFlag(key);

	public void ClearFlag(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
			return;

		if (flagSet.Remove(key))
		{
			activeFlags.Remove(key);
			Debug.Log($"[GameFlags] Cleared: {key}");
		}
	}

	// ------------------------
	// SAVE INTEGRATION ONLY
	// ------------------------

	public void LoadFromSave(List<string> flags)
	{
		activeFlags.Clear();
		flagSet.Clear();

		if (flags == null)
			return;

		foreach (var f in flags)
		{
			if (string.IsNullOrWhiteSpace(f))
				continue;

			activeFlags.Add(f);
			flagSet.Add(f);
		}

		Debug.Log($"[GameFlags] Loaded {activeFlags.Count} flags from save");
	}

	public List<string> ExportFlags()
	{
		return new List<string>(activeFlags);
	}

	// ------------------------
	// INTERNAL
	// ------------------------

	private void SyncRuntime()
	{
		flagSet.Clear();

		foreach (var f in activeFlags)
		{
			if (!string.IsNullOrWhiteSpace(f))
				flagSet.Add(f);
		}
	}

	public void ClearAll()
	{
		activeFlags.Clear();
		flagSet.Clear();
	}
}