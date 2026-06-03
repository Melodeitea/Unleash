using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Talk";
    [SerializeField] private DialogueSequence sequence;

    [Header("Flag Gate")]
    [SerializeField] private bool requireFlag = false;
    [SerializeField] private string requiredFlag = "flashlight_on";
    [SerializeField] private string lockedPrompt = "";

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceComplete;

    private bool _isPlaying = false;

    // ── IInteractable ─────────────────────────────────────────────

    public string GetPrompt()
    {
        if (_isPlaying) return string.Empty;
        if (!IsFlagSatisfied()) return lockedPrompt;
        return prompt;
    }

    public void Interact(Player player)
    {
        if (_isPlaying) return;
        if (!IsFlagSatisfied()) return;

        _isPlaying = true;
        DialogueManager.Instance.PlaySequence(sequence, OnComplete);
    }

    // ── Internals ─────────────────────────────────────────────────

    private void OnComplete()
{
    StartCoroutine(ResetNextFrame());
    onSequenceComplete?.Invoke();
}

	private System.Collections.IEnumerator ResetNextFrame()
	{
		yield return null;
		_isPlaying = false;
	}

    private bool IsFlagSatisfied()
    {
        if (!requireFlag) return true;
        if (GameFlags.Instance == null) return false;
        return GameFlags.Instance.GetFlag(requiredFlag);
    }
}