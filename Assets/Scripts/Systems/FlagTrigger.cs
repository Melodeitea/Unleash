using UnityEngine;
using UnityEngine.Events;

public class FlagTrigger : MonoBehaviour, IInteractable
{
	[Header("Prompt")]
	[SerializeField] private string prompt = "Examine";
	[SerializeField] private string usedPrompt = "";          // empty = hide after use

	[Header("Behaviour")]
	[SerializeField] private bool triggerOnce = true;

	[Header("Flags")]
	[SerializeField] private string selfFlag = "";            // auto-set on use (like "used_" + id)
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;
	[SerializeField] private string requiredFlag = "";        // optional gate
	[SerializeField] private string lockedPrompt = "";        // shown when gate is unmet

	[Header("Events")]
	[SerializeField] private UnityEvent onTriggered;

	private bool _used = false;

	private void Start()
	{
		// Restore state from flags on load
		if (!string.IsNullOrWhiteSpace(selfFlag)
			&& GameFlags.Instance != null
			&& GameFlags.Instance.GetFlag(selfFlag))
		{
			_used = true;
		}
	}

	public string GetPrompt()
	{
		if (_used) return usedPrompt;
		if (!IsGateSatisfied()) return lockedPrompt;
		return prompt;
	}

	public void Interact(Player player)
	{
		if (triggerOnce && _used) return;
		if (!IsGateSatisfied()) return;

		if (triggerOnce) _used = true;

		if (GameFlags.Instance != null)
		{
			if (!string.IsNullOrWhiteSpace(selfFlag))
				GameFlags.Instance.SetFlag(selfFlag);

			foreach (var flag in flagsToSet)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.SetFlag(flag);

			foreach (var flag in flagsToClear)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.ClearFlag(flag);
		}

		onTriggered?.Invoke();
	}

	private bool IsGateSatisfied()
	{
		if (string.IsNullOrWhiteSpace(requiredFlag)) return true;
		if (GameFlags.Instance == null) return false;
		return GameFlags.Instance.GetFlag(requiredFlag);
	}
}