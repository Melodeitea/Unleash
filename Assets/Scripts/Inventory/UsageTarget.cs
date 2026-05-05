using UnityEngine;

public class UsageTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string targetID;       // must match ItemData.usageTargetID
	[SerializeField] private string lockedPrompt = "It won't budge...";
	[SerializeField] private string unlockedPrompt = "Use item here";
	[SerializeField] private UnityEngine.Events.UnityEvent onUsed;

	private bool _used = false;

	public string GetPrompt()
	{
		if (_used) return "";
		bool hasItem = ActiveItemHolder.Current != null &&
					   ActiveItemHolder.Current.usageTargetID == targetID;
		return hasItem ? $"[E] {unlockedPrompt}" : lockedPrompt;
	}

	public void Interact(Player player)
	{
		if (_used) return;
		if (InventoryManager.Instance.TryUseItemOnTarget(targetID, out _))
		{
			_used = true;
			ActiveItemHolder.Clear();
			onUsed?.Invoke();           // trigger door open, animation, etc.
			Debug.Log($"{targetID} used successfully.");
		}
		else
		{
			Debug.Log("Wrong item or no item selected.");
		}
	}
}