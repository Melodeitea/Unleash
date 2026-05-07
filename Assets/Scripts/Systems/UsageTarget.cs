using UnityEngine;

public class UsageTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string targetID;
	[SerializeField] private string lockedPrompt = "It won't budge...";
	[SerializeField] private string unlockedPrompt = "Use item here";
	[SerializeField] private Animator objectAnimator; // drag the target object here, Animator disabled by default
	[SerializeField] private UnityEngine.Events.UnityEvent onUsed;

	private bool _used = false;

	private void Start()
	{
		if (GameFlags.Instance != null && GameFlags.Instance.GetFlag("used_" + targetID))
		{
			_used = true;
			if (objectAnimator != null)
			{
				objectAnimator.enabled = true;
				objectAnimator.Play(objectAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f); // snap to end
			}
		}
	}
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
			GameFlags.Instance?.SetFlag("used_" + targetID); // ADD THIS LINE
			ActiveItemHolder.Clear();
			if (objectAnimator != null)
				objectAnimator.enabled = true;
			else
				Debug.LogWarning($"UsageTarget '{targetID}': no Animator assigned.");
			onUsed?.Invoke();
		}
	}
}