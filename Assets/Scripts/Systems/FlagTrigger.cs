using UnityEngine;
using UnityEngine.Events;

public class FlagTrigger : MonoBehaviour, IInteractable
{
	[Header("Prompt")]
	[SerializeField] private string prompt = "Examine";
	[SerializeField] private string usedPrompt = ""; // empty = hide after use

	[Header("Behaviour")]
	[SerializeField] private bool triggerOnce = true;

	[Header("Flags")]
	[SerializeField] private string selfFlag = "";
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;
	[SerializeField] private string requiredFlag = "";
	[SerializeField] private string lockedPrompt = "";

	[Header("Events")]
	[SerializeField] private UnityEvent onTriggered;

	private bool _used;

	private void Start()
	{
		if (!string.IsNullOrWhiteSpace(selfFlag)
			&& GameFlags.Instance != null
			&& GameFlags.Instance.GetFlag(selfFlag))
		{
			_used = true;
		}
	}

	// 🔥 RULE: null/empty = DO NOT SHOW PROMPT UI
	public string GetPrompt()
	{
		if (_used)
		{
			if (string.IsNullOrWhiteSpace(usedPrompt))
				return null;

			return usedPrompt;
		}

		if (!IsGateSatisfied())
		{
			if (string.IsNullOrWhiteSpace(lockedPrompt))
				return null;

			return lockedPrompt;
		}

		if (string.IsNullOrWhiteSpace(prompt))
			return null;

		return prompt;
	}

	public void Interact(Player player)
	{
		if (triggerOnce && _used) return;
		if (!IsGateSatisfied()) return;

		if (triggerOnce)
			_used = true;

		if (GameFlags.Instance != null)
		{
			if (!string.IsNullOrWhiteSpace(selfFlag))
				GameFlags.Instance.SetFlag(selfFlag);

			if (flagsToSet != null)
			{
				foreach (var flag in flagsToSet)
					if (!string.IsNullOrWhiteSpace(flag))
						GameFlags.Instance.SetFlag(flag);
			}

			if (flagsToClear != null)
			{
				foreach (var flag in flagsToClear)
					if (!string.IsNullOrWhiteSpace(flag))
						GameFlags.Instance.ClearFlag(flag);
			}
		}

		onTriggered?.Invoke();
	}

	private bool IsGateSatisfied()
	{
		if (string.IsNullOrWhiteSpace(requiredFlag))
			return true;

		if (GameFlags.Instance == null)
			return false;

		return GameFlags.Instance.GetFlag(requiredFlag);
	}
}