using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TeleportDoor : MonoBehaviour, IInteractable
{
	[Header("Key (optional)")]
	[Tooltip("Leave empty if no key is required — e.g. the return door.")]
	[SerializeField] private string targetID;
	[SerializeField] private string lockedPrompt = "It won't budge...";
	[SerializeField] private string unlockedPrompt = "Use key";
	[SerializeField] private string openPrompt = "Go through";

	[Header("Teleport")]
	[SerializeField] private Transform destination;
	[SerializeField] private float teleportDelay = 0f;   // seconds after door anim before warp

	[Header("Animators")]
	[SerializeField] private Animator lockAnimator;
	[SerializeField] private Animator doorAnimator;

	[Header("Flags")]
	[Tooltip("Shared between both doors — same ID on both so unlocking one unlocks the other.")]
	[SerializeField] private string unlockFlagID;
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;

	[Header("Events")]
	[SerializeField] private UnityEvent onUnlocked;
	[SerializeField] private UnityEvent onTeleport;

	private bool _unlocked = false;

	// ── Lifecycle ─────────────────────────────────────────────────

	private void Start()
	{
		bool noKeyRequired = string.IsNullOrWhiteSpace(targetID);

		if (noKeyRequired)
		{
			// Return door — always passable
			_unlocked = true;
			SnapToEnd(lockAnimator);
		}
		else if (GameFlags.Instance != null
				 && GameFlags.Instance.GetFlag(UnlockFlag()))
		{
			// Already unlocked in a previous session
			_unlocked = true;
			SnapToEnd(lockAnimator);
			SnapToEnd(doorAnimator);
		}
		else
		{
			if (lockAnimator != null) lockAnimator.enabled = false;
			if (doorAnimator != null) doorAnimator.enabled = false;
		}
	}

	// ── IInteractable ─────────────────────────────────────────────

	public string GetPrompt()
	{
		if (_unlocked) return $"[E] {openPrompt}";

		bool hasKey = !string.IsNullOrWhiteSpace(targetID)
					  && InventoryManager.Instance.items.Exists(i => i.usageTargetID == targetID);

		return hasKey ? $"[E] {unlockedPrompt}" : lockedPrompt;
	}

	public void Interact(Player player)
	{
		if (_unlocked)
		{
			StartCoroutine(TeleportSequence(player, playDoorAnim: true));
			return;
		}

		// Requires key
		if (string.IsNullOrWhiteSpace(targetID)) return;

		if (InventoryManager.Instance.TryUseItemOnTarget(targetID, out _))
		{
			_unlocked = true;
			ActiveItemHolder.Clear();

			if (GameFlags.Instance != null)
			{
				GameFlags.Instance.SetFlag(UnlockFlag());
				foreach (var flag in flagsToSet)
					if (!string.IsNullOrWhiteSpace(flag))
						GameFlags.Instance.SetFlag(flag);
				foreach (var flag in flagsToClear)
					if (!string.IsNullOrWhiteSpace(flag))
						GameFlags.Instance.ClearFlag(flag);
			}

			onUnlocked?.Invoke();
			StartCoroutine(TeleportSequence(player, playDoorAnim: true));
		}
	}

	// ── Teleport sequence ─────────────────────────────────────────

	private IEnumerator TeleportSequence(Player player, bool playDoorAnim)
	{
		// 1. Lock anim (only plays on first unlock — already snapped on re-entry)
		if (!_unlocked && lockAnimator != null)
		{
			lockAnimator.enabled = true;
			yield return WaitForAnim(lockAnimator);
		}

		// 2. Door open anim
		if (playDoorAnim && doorAnimator != null)
		{
			doorAnimator.enabled = true;
			yield return WaitForAnim(doorAnimator);
		}

		// 3. Optional extra delay before warp
		if (teleportDelay > 0f)
			yield return new WaitForSeconds(teleportDelay);

		// 4. Teleport
		if (destination == null)
		{
			Debug.LogWarning($"[TeleportDoor] No destination assigned on {gameObject.name}.");
			yield break;
		}

		CharacterController cc = player.GetComponent<CharacterController>();
		if (cc != null) cc.enabled = false;

		player.transform.SetPositionAndRotation(destination.position, destination.rotation);

		if (cc != null) cc.enabled = true;

		onTeleport?.Invoke();
	}

	// ── Helpers ───────────────────────────────────────────────────

	private string UnlockFlag() => $"door_unlocked_{unlockFlagID}";

	private IEnumerator WaitForAnim(Animator anim)
	{
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