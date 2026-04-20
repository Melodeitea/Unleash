using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Lightweight puzzle system (editor/runtime).
/// - Define puzzles in the inspector (id + trigger).
/// - Call AddItem / RemoveItem / TryEnterCode to drive puzzles.
/// - When a puzzle is solved this will update internal state and invoke OnPuzzleSolved.
/// - Saving is handled by your SaveSystem via PlayerData which reads GetSolvedPuzzleIds().
/// </summary>
public class PuzzleSystem : MonoBehaviour
{
	[Serializable]
	public enum TriggerType
	{
		RequiresItem,
		RequiresAllItems,
		RequiresAnyItem,
		EnterCode,
		RequiresKey
	}

	[Serializable]
	public class PuzzleDefinition
	{
		public string puzzleId;
		public TriggerType trigger = TriggerType.RequiresItem;
		public string itemId;
		public List<string> itemIds = new();
		public string code;
		[ReadOnly] public bool isSolved = false;
	}

	[Header("Puzzle Definitions")]
	public List<PuzzleDefinition> puzzles = new();

	[Header("Runtime inventory")]
	[SerializeField, Tooltip("Runtime item IDs (keys, items) held by the player / world")]
	List<string> startingItems = new();

	// runtime state
	HashSet<string> _inventory = new();
	HashSet<string> _solved = new();

	public event Action<string> OnPuzzleSolved; // puzzleId

	void Awake()
	{
		_inventory.Clear();
		foreach (var id in startingItems)
		{
			if (!string.IsNullOrEmpty(id)) _inventory.Add(id);
		}

		// If you need to preload solved IDs from a save, call ApplySolvedIds(...) after loading.
		// Update inspector flags from current _solved set.
		foreach (var p in puzzles)
		{
			if (!string.IsNullOrEmpty(p.puzzleId) && _solved.Contains(p.puzzleId))
				p.isSolved = true;
		}

		EvaluateAll();
	}

	// Inventory API
	public bool HasItem(string itemId) => !string.IsNullOrEmpty(itemId) && _inventory.Contains(itemId);

	public void AddItem(string itemId)
	{
		if (string.IsNullOrEmpty(itemId)) return;
		if (_inventory.Add(itemId))
		{
			Debug.Log($"[PuzzleSystem] Item added: {itemId}");
			EvaluateForItem(itemId);
		}
	}

	public bool RemoveItem(string itemId)
	{
		if (string.IsNullOrEmpty(itemId)) return false;
		bool removed = _inventory.Remove(itemId);
		if (removed) Debug.Log($"[PuzzleSystem] Item removed: {itemId}");
		return removed;
	}

	// Code entry API
	public void TryEnterCode(string puzzleId, string codeEntered)
	{
		if (string.IsNullOrEmpty(puzzleId)) return;
		var defs = puzzles.FindAll(p => p.puzzleId == puzzleId);
		foreach (var p in defs)
		{
			if (p.isSolved) continue;
			if (p.trigger != TriggerType.EnterCode) continue;
			if (string.Equals(p.code, codeEntered, StringComparison.Ordinal))
			{
				Solve(p);
			}
			else
			{
				Debug.Log($"[PuzzleSystem] Code mismatch for {p.puzzleId}");
			}
		}
	}

	// Evaluate all puzzles (call at startup or after major inventory changes)
	public void EvaluateAll()
	{
		foreach (var p in puzzles)
		{
			if (p.isSolved) continue;
			EvaluateAndSolveIfMatch(p);
		}
	}

	// Evaluate for a specific inventory item (fast path)
	public void EvaluateForItem(string itemId)
	{
		foreach (var p in puzzles)
		{
			if (p.isSolved) continue;
			if (p.trigger == TriggerType.RequiresItem || p.trigger == TriggerType.RequiresKey)
			{
				if (p.itemId == itemId)
					EvaluateAndSolveIfMatch(p);
			}
			else if (p.trigger == TriggerType.RequiresAnyItem)
			{
				if (p.itemIds != null && p.itemIds.Contains(itemId))
					EvaluateAndSolveIfMatch(p);
			}
			else if (p.trigger == TriggerType.RequiresAllItems)
			{
				if (p.itemIds != null && p.itemIds.Contains(itemId))
					EvaluateAndSolveIfMatch(p);
			}
		}
	}

	// Single-puzzle evaluation
	void EvaluateAndSolveIfMatch(PuzzleDefinition p)
	{
		if (p == null || p.isSolved) return;

		bool match = false;
		switch (p.trigger)
		{
			case TriggerType.RequiresItem:
			case TriggerType.RequiresKey:
				match = HasItem(p.itemId);
				break;
			case TriggerType.RequiresAnyItem:
				if (p.itemIds != null && p.itemIds.Count > 0)
				{
					foreach (var id in p.itemIds) { if (HasItem(id)) { match = true; break; } }
				}
				break;
			case TriggerType.RequiresAllItems:
				if (p.itemIds != null && p.itemIds.Count > 0)
				{
					match = true;
					foreach (var id in p.itemIds) { if (!HasItem(id)) { match = false; break; } }
				}
				break;
			case TriggerType.EnterCode:
				match = false;
				break;
		}

		if (match) Solve(p);
	}

	void Solve(PuzzleDefinition p)
	{
		if (p == null || p.isSolved) return;
		p.isSolved = true;
		if (!string.IsNullOrEmpty(p.puzzleId))
		{
			_solved.Add(p.puzzleId);
		}

		Debug.Log($"[PuzzleSystem] Puzzle solved: {p.puzzleId}");
		OnPuzzleSolved?.Invoke(p.puzzleId);
	}

	bool IsMarkedSolved(string puzzleId)
	{
		if (string.IsNullOrEmpty(puzzleId)) return false;
		return _solved.Contains(puzzleId);
	}

	// Public API: apply solved puzzle IDs (used when loading saved game)
	public void ApplySolvedIds(IEnumerable<string> ids)
	{
		if (ids == null) return;
		foreach (var id in ids)
		{
			if (string.IsNullOrEmpty(id)) continue;
			if (_solved.Contains(id)) continue;

			_solved.Add(id);

			// update inspector definitions if present
			foreach (var p in puzzles)
			{
				if (p != null && p.puzzleId == id)
				{
					p.isSolved = true;
				}
			}

			// invoke any runtime handlers
			OnPuzzleSolved?.Invoke(id);
		}
	}

	// Public accessor for save system / other systems
	public List<string> GetSolvedPuzzleIds()
	{
		return new List<string>(_solved);
	}

	// Editor / debug helper
	[ContextMenu("Dump inventory")]
	void DebugDumpInventory()
	{
		Debug.Log($"Inventory: {string.Join(", ", _inventory)}");
	}

	[ContextMenu("Dump puzzles")]
	void DebugDumpPuzzles()
	{
		foreach (var p in puzzles)
			Debug.Log($"Puzzle {p.puzzleId} solved={p.isSolved} trigger={p.trigger}");
	}
}