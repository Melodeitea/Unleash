using UnityEngine;

public class UsageTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string targetID;
	[SerializeField] private string lockedPrompt = "It won't budge...";
	[SerializeField] private string unlockedPrompt = "Use item here";
	[SerializeField] private Animator objectAnimator; // drag the target object here, Animator disabled by default
	[SerializeField] private UnityEngine.Events.UnityEvent onUsed;

	private bool _used = false;

	public string GetPrompt()
	{
		if (_used) return "";
		bool hasItem = InventoryManager.Instance.items
						   .Exists(i => i.usageTargetID == targetID);
		return hasItem ? $"[E] {unlockedPrompt}" : lockedPrompt;
	}

	public void Interact(Player player)
	{
		if (_used) return;
		if (InventoryManager.Instance.TryUseItemOnTarget(targetID, out _))
		{
			_used = true;
			ActiveItemHolder.Clear();

			if (objectAnimator != null)
				objectAnimator.enabled = true;
			else
				Debug.LogWarning($"UsageTarget '{targetID}': no Animator assigned.");

			onUsed?.Invoke();
			Debug.Log($"{targetID} used successfully.");
		}
		else
		{
			Debug.Log("Wrong item or no item selected.");
		}
	}
}