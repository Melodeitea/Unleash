using UnityEngine;

public class UsageTarget : MonoBehaviour, IInteractable
{
	[SerializeField] private string targetID;
	[SerializeField] private string lockedPrompt = "It won't budge...";
	[SerializeField] private string unlockedPrompt = "Use item here";
	[SerializeField] private Animator lockAnimator;   // lock mechanism anim
	[SerializeField] private Animator objectAnimator; // door/drawer anim
	[SerializeField] private UnityEngine.Events.UnityEvent onUsed;

	private bool _used = false;

	private void Start()
	{
		if (GameFlags.Instance != null && GameFlags.Instance.GetFlag("used_" + targetID))
		{
			_used = true;
			SnapToEnd(lockAnimator);
			SnapToEnd(objectAnimator);
		}
		else
		{
			if (lockAnimator != null) lockAnimator.enabled = false;
			if (objectAnimator != null) objectAnimator.enabled = false;
		}
	}

	public string GetPrompt()
	{
		if (_used) return "";
		bool hasItem = InventoryManager.Instance.items.Exists(i => i.usageTargetID == targetID);
		return hasItem ? $"[E] {unlockedPrompt}" : lockedPrompt;
	}

	public void Interact(Player player)
	{
		if (_used) return;
		if (InventoryManager.Instance.TryUseItemOnTarget(targetID, out _))
		{
			_used = true;
			GameFlags.Instance?.SetFlag("used_" + targetID);
			ActiveItemHolder.Clear();
			onUsed?.Invoke();
			StartCoroutine(PlaySequence());
		}
	}

	private System.Collections.IEnumerator PlaySequence()
	{
		// 1. Play lock anim and wait for it
		if (lockAnimator != null)
		{
			lockAnimator.enabled = true;
			yield return WaitForAnim(lockAnimator);
		}

		// 2. Then play door/drawer anim
		if (objectAnimator != null)
			objectAnimator.enabled = true;
		else
			Debug.LogWarning($"UsageTarget '{targetID}': no object Animator assigned.");
	}

	private System.Collections.IEnumerator WaitForAnim(Animator anim)
	{
		// Wait one frame for the animator to start
		yield return null;
		float length = anim.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(length);
	}

	private void SnapToEnd(Animator anim)
	{
		if (anim == null) return;
		anim.enabled = true;
		anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
	}
}