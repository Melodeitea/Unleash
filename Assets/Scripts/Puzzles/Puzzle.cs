using UnityEngine;
using System;

[DisallowMultipleComponent]
public class Puzzle : MonoBehaviour
{
	[Tooltip("Unique identifier for this puzzle (used for saving).")]
	public string puzzleId;

	public bool isSolved;

	public event Action OnSolved;

	void Start()
	{
		// reflect saved state at start
		if (!string.IsNullOrEmpty(puzzleId) && PuzzleManager.Instance != null)
		{
			isSolved = PuzzleManager.Instance.IsSolved(puzzleId);
			if (isSolved)
				OnApplySolvedState();
		}
	}

	// Call this when puzzle conditions are met
	public virtual void Solve()
	{
		if (isSolved) return;
		isSolved = true;

		if (!string.IsNullOrEmpty(puzzleId) && PuzzleManager.Instance != null)
			PuzzleManager.Instance.MarkSolved(puzzleId);

		OnSolved?.Invoke();
		OnApplySolvedState();
	}

	// Override to play effects, open door etc.
	protected virtual void OnApplySolvedState()
	{
		// default: nothing. Derived puzzle types should override.
	}
}
