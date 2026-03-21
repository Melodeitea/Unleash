using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PuzzleManager : MonoBehaviour
{
	public static PuzzleManager Instance { get; private set; }

	readonly HashSet<string> _solved = new HashSet<string>();

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		// optional: DontDestroyOnLoad(gameObject);
	}

	// Mark a puzzle solved (id must be unique)
	public void MarkSolved(string puzzleId)
	{
		if (string.IsNullOrEmpty(puzzleId)) return;
		if (_solved.Add(puzzleId))
		{
			Debug.Log($"Puzzle solved: {puzzleId}");
		}
	}

	public bool IsSolved(string puzzleId)
	{
		if (string.IsNullOrEmpty(puzzleId)) return false;
		return _solved.Contains(puzzleId);
	}

	public List<string> GetSolvedPuzzleIds()
	{
		return new List<string>(_solved);
	}

	public void SetSolvedList(IEnumerable<string> ids)
	{
		_solved.Clear();
		if (ids == null) return;
		foreach (var id in ids)
		{
			if (!string.IsNullOrEmpty(id))
				_solved.Add(id);
		}
	}
}
