using System.Collections.Generic;
using UnityEngine;

public class GameFlags : MonoBehaviour
{
	public static GameFlags Instance { get; private set; }

	[SerializeField] private List<string> activeFlags = new();

	private HashSet<string> flagSet = new();

	private const string SAVE_KEY = "game_flags";

	// ------------------------
	// COMMON FLAGS (define here to avoid typos)
	// ------------------------
	public const string FINGERPRINT_CONFIRMED = "fingerprint_confirmed";
	public const string TIMELINE_ASSEMBLED = "timeline_assembled";
	public const string CONTRACT_UNLOCKED = "contract_unlocked";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		// Sync HashSet with serialized list
		flagSet = new HashSet<string>(activeFlags);
	}

	// ------------------------
	// CORE METHODS
	// ------------------------

	public void SetFlag(string key)
	{
		if (string.IsNullOrEmpty(key)) return;

		if (flagSet.Add(key))
		{
			activeFlags.Add(key);
			Debug.Log($"[GameFlags] Set: {key}");
		}
	}

	public bool GetFlag(string key)
	{
		if (string.IsNullOrEmpty(key)) return false;

		return flagSet.Contains(key);
	}

	public void ClearFlag(string key)
	{
		if (flagSet.Remove(key))
		{
			activeFlags.Remove(key);
			Debug.Log($"[GameFlags] Cleared: {key}");
		}
	}

	// ------------------------
	// SAVE / LOAD
	// ------------------------

	public void SaveFlags()
	{
		string data = string.Join(",", activeFlags);
		PlayerPrefs.SetString(SAVE_KEY, data);
		PlayerPrefs.Save();

		Debug.Log("[GameFlags] Saved.");
	}

	public void LoadFlags()
	{
		activeFlags.Clear();
		flagSet.Clear();

		string data = PlayerPrefs.GetString(SAVE_KEY, "");

		if (string.IsNullOrEmpty(data))
			return;

		string[] flags = data.Split(',');

		foreach (string f in flags)
		{
			if (!string.IsNullOrWhiteSpace(f))
			{
				activeFlags.Add(f);
				flagSet.Add(f);
			}
		}

		Debug.Log("[GameFlags] Loaded.");
	}

	// ------------------------
	// DEBUG
	// ------------------------

	public void ClearAll()
	{
		activeFlags.Clear();
		flagSet.Clear();
		PlayerPrefs.DeleteKey(SAVE_KEY);

		Debug.Log("[GameFlags] All flags cleared.");
	}
}