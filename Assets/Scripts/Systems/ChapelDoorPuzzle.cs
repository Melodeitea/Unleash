using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChapelDoorPuzzle : MonoBehaviour, IInteractable
{
	[Header("Sequence")]
	[Tooltip("Symbol IDs in the required identification order.")]
	[SerializeField] private string[] correctSequence = { "cross", "eye", "serpent" };

	[Header("Prompts")]
	[SerializeField] private string lockedPrompt = "Locked from inside.";
	[SerializeField] private string openedPrompt = "";

	[Header("Door")]
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private Animator workerAnimator;   // worker opening anim, optional

	[Header("Flags")]
	[SerializeField] private string puzzleID = "chapel_door_4_1";
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;

	[Header("Events")]
	[SerializeField] private UnityEvent onAllSymbolsIdentified;  // worker reacts
	[SerializeField] private UnityEvent onDoorOpened;

	private int _nextIndex = 0;
	private bool _opened = false;
	private readonly HashSet<string> _identified = new();

	// ── Lifecycle ─────────────────────────────────────────────────

	private void Start()
	{
		if (doorAnimator != null) doorAnimator.enabled = false;
		if (workerAnimator != null) workerAnimator.enabled = false;

		if (GameFlags.Instance == null) return;

		// Restore solved state
		if (GameFlags.Instance.GetFlag($"{puzzleID}_opened"))
		{
			_opened = true;
			_nextIndex = correctSequence.Length;
			SnapToEnd(doorAnimator);
			return;
		}

		// Restore partial progress
		foreach (var id in correctSequence)
		{
			if (GameFlags.Instance.GetFlag($"chapel_symbol_{id}"))
			{
				_identified.Add(id);
				_nextIndex++;
			}
			else break; // sequence must be contiguous
		}
	}

	// ── Called by ChapelSymbol ────────────────────────────────────

	public bool IsNextSymbol(string symbolID)
	{
		if (_opened || _nextIndex >= correctSequence.Length) return false;
		return correctSequence[_nextIndex] == symbolID;
	}

	public void OnSymbolIdentified(string symbolID)
	{
		if (!IsNextSymbol(symbolID)) return;

		_identified.Add(symbolID);
		_nextIndex++;

		Debug.Log($"[ChapelDoor] Identified: {symbolID}  ({_nextIndex}/{correctSequence.Length})");

		if (_nextIndex >= correctSequence.Length)
		{
			onAllSymbolsIdentified?.Invoke();
			StartCoroutine(OpenSequence());
		}
	}

	// ── IInteractable — door itself ───────────────────────────────

	public string GetPrompt()
	{
		if (_opened) return openedPrompt;
		return _nextIndex > 0 ? "" : lockedPrompt;  // silent once puzzle has started
	}

	public void Interact(Player player)
	{
		// Door isn't directly interactable during puzzle — symbols are the interaction
	}

	// ── Open sequence ─────────────────────────────────────────────

	private IEnumerator OpenSequence()
	{
		// Brief pause — worker heard her, now moves to open
		yield return new WaitForSeconds(1.2f);

		if (workerAnimator != null)
		{
			workerAnimator.enabled = true;
			yield return null;
			float workerAnimLength = workerAnimator.GetCurrentAnimatorStateInfo(0).length;
			yield return new WaitForSeconds(workerAnimLength);
		}

		_opened = true;

		if (GameFlags.Instance != null)
		{
			GameFlags.Instance.SetFlag($"{puzzleID}_opened");
			foreach (var flag in flagsToSet)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.SetFlag(flag);
			foreach (var flag in flagsToClear)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.ClearFlag(flag);
		}

		if (doorAnimator != null)
			doorAnimator.enabled = true;
		else
			Debug.LogWarning($"[ChapelDoorPuzzle] No door Animator assigned.");

		onDoorOpened?.Invoke();
	}

	private void SnapToEnd(Animator anim)
	{
		if (anim == null) return;
		anim.enabled = true;
		anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
	}
}