using UnityEngine;

public class ChapelSymbol : MonoBehaviour, IInteractable
{
	[SerializeField] private string symbolID;         // "cross" / "eye" / "serpent"
	[SerializeField] private string prompt = "Examine";
	[SerializeField] private string outOfSequencePrompt = "";  // empty = silent if wrong order

	[TextArea]
	[SerializeField] private string roxanneMonologue; // what she says when she names it

	private ChapelDoorPuzzle _puzzle;
	private bool _identified = false;

	private void Start()
	{
		_puzzle = FindObjectOfType<ChapelDoorPuzzle>();

		// Restore state if already identified
		if (GameFlags.Instance != null && GameFlags.Instance.GetFlag($"chapel_symbol_{symbolID}"))
			_identified = true;
	}

	public string GetPrompt()
	{
		if (_identified) return string.Empty;
		if (_puzzle == null) return string.Empty;

		// Only show interact prompt if this is the next expected symbol
		if (_puzzle.IsNextSymbol(symbolID))
			return $"[E] {prompt}";

		return outOfSequencePrompt;
	}

	public void Interact(Player player)
	{
		if (_identified) return;
		if (_puzzle == null) return;
		if (!_puzzle.IsNextSymbol(symbolID)) return;

		// Roxanne names it
		if (!string.IsNullOrEmpty(roxanneMonologue))
			MonologueManager.Instance.Play(roxanneMonologue);

		_identified = true;
		GameFlags.Instance?.SetFlag($"chapel_symbol_{symbolID}");

		_puzzle.OnSymbolIdentified(symbolID);
	}
}