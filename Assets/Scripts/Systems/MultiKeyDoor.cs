using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MultiKeyDoor : MonoBehaviour, IInteractable
{
	[System.Serializable]
	public class KeySlot
	{
		public string targetID;
		public string label = "Key";
		public Animator lockAnimator;
		[HideInInspector] public bool inserted;
	}

	[Header("Key Slots")]
	[SerializeField] private KeySlot[] slots;

	[Header("Prompts")]
	[SerializeField] private string lockedPrompt = "Requires all three keys";
	[SerializeField] private string openedPrompt = "";

	[Header("Door")]
	[SerializeField] private Animator doorAnimator;

	[Header("Flags")]
	[SerializeField] private string doorID;
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;

	[Header("Events")]
	[SerializeField] private UnityEvent onAllKeysInserted;
	[SerializeField] private UnityEvent onDoorOpened;

	private bool _opened = false;

	// ── Lifecycle ─────────────────────────────────────────────────

	private void Start()
	{
		if (GameFlags.Instance == null) return;

		foreach (var slot in slots)
		{
			if (GameFlags.Instance.GetFlag(SlotFlag(slot)))
			{
				slot.inserted = true;
				SnapToEnd(slot.lockAnimator);
			}
			else
			{
				if (slot.lockAnimator != null) slot.lockAnimator.enabled = false;
			}
		}

		if (GameFlags.Instance.GetFlag(DoorFlag()))
		{
			_opened = true;
			SnapToEnd(doorAnimator);
		}
		else
		{
			if (doorAnimator != null) doorAnimator.enabled = false;
		}
	}

	// ── IInteractable ─────────────────────────────────────────────

	public string GetPrompt()
	{
		if (_opened) return openedPrompt;

		KeySlot target = FindMatchingSlot();
		if (target != null)
			return $"[E] Insert {target.label}  ({InsertedCount()}/{slots.Length})";

		if (AllInserted()) return "[E] Open";

		return InsertedCount() > 0
			? $"{lockedPrompt}  ({InsertedCount()}/{slots.Length})"
			: lockedPrompt;
	}

	public void Interact(Player player)
	{
		if (_opened) return;

		if (AllInserted())
		{
			OpenDoor();
			return;
		}

		KeySlot slot = FindMatchingSlot();
		if (slot == null) return;

		if (InventoryManager.Instance.TryUseItemOnTarget(slot.targetID, out _))
		{
			slot.inserted = true;
			GameFlags.Instance?.SetFlag(SlotFlag(slot));
			ActiveItemHolder.Clear();

			StartCoroutine(PlaySlotAnim(slot));

			if (AllInserted())
			{
				onAllKeysInserted?.Invoke();
				StartCoroutine(OpenAfterDelay());
			}
		}
	}

	// ── Internal logic ────────────────────────────────────────────

	private void OpenDoor()
	{
		_opened = true;

		if (GameFlags.Instance != null)
		{
			GameFlags.Instance.SetFlag(DoorFlag());

			foreach (var flag in flagsToSet)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.SetFlag(flag);

			foreach (var flag in flagsToClear)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.ClearFlag(flag);
		}

		onDoorOpened?.Invoke();

		if (doorAnimator != null)
			doorAnimator.enabled = true;
		else
			Debug.LogWarning($"MultiKeyDoor '{doorID}': no door Animator assigned.");
	}

	private IEnumerator OpenAfterDelay()
	{
		float longestSlotAnim = 0f;
		foreach (var slot in slots)
		{
			if (slot.lockAnimator != null && slot.lockAnimator.enabled)
			{
				yield return null;
				float len = slot.lockAnimator.GetCurrentAnimatorStateInfo(0).length;
				if (len > longestSlotAnim) longestSlotAnim = len;
			}
		}
		yield return new WaitForSeconds(longestSlotAnim);
		OpenDoor();
	}

	private IEnumerator PlaySlotAnim(KeySlot slot)
	{
		if (slot.lockAnimator == null) yield break;
		slot.lockAnimator.enabled = true;
		yield return null;
		float length = slot.lockAnimator.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(length);
	}

	// ── Helpers ───────────────────────────────────────────────────

	private KeySlot FindMatchingSlot()
	{
		foreach (var slot in slots)
		{
			if (slot.inserted) continue;
			if (InventoryManager.Instance.items.Exists(i => i.usageTargetID == slot.targetID))
				return slot;
		}
		return null;
	}

	private bool AllInserted()
	{
		foreach (var slot in slots) if (!slot.inserted) return false;
		return true;
	}

	private int InsertedCount()
	{
		int count = 0;
		foreach (var slot in slots) if (slot.inserted) count++;
		return count;
	}

	private string SlotFlag(KeySlot slot) => $"slot_inserted_{doorID}_{slot.targetID}";
	private string DoorFlag() => $"door_opened_{doorID}";

	private void SnapToEnd(Animator anim)
	{
		if (anim == null) return;
		anim.enabled = true;
		anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
	}
}