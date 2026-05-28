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

	// 🔥 cache so we don't spam checks every frame in UI systems
	private bool isConsumed => sequence != null && sequence.triggerOnce && sequence.hasPlayed;

	public void Interact(Player player)
	{
		// ❌ Block forever if already used (triggerOnce)
		if (isConsumed) return;

		if (!IsFlagSatisfied()) return;

		if (sequence != null)
			DialogueManager.Instance.PlaySequence(sequence, OnComplete);
	}

	public string GetPrompt()
	{
		// ❌ DO NOT show prompt if already consumed
		if (isConsumed)
			return string.Empty;

		if (!IsFlagSatisfied())
			return lockedPrompt;

		return prompt;
	}

	private bool IsFlagSatisfied()
	{
		if (!requireFlag) return true;
		if (GameFlags.Instance == null) return false;

		return GameFlags.Instance.GetFlag(requiredFlag);
	}

	private void OnComplete()
	{
		onSequenceComplete?.Invoke();

		// Optional: hard disable object after completion
		if (isConsumed)
		{
			gameObject.SetActive(false);
		}
	}
}