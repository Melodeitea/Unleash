using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CardSequencePanel : MonoBehaviour, IInteractable
{
	[System.Serializable]
	public class CardSlot
	{
		public string cardID;
		public string label = "Card";
		public Animator slotAnimator;
		[HideInInspector] public bool filled;
	}

	[Header("Sequence")]
	[Tooltip("Define the slots IN CORRECT ORDER. The player must insert cards in this exact order.")]
	[SerializeField] private CardSlot[] correctSequence;

	[Header("Prompts")]
	[SerializeField] private string idlePrompt = "Examine the panel";
	[SerializeField] private string waitingPrompt = "Insert a card";
	[SerializeField] private string wrongPrompt = "...nothing happened.";
	[SerializeField] private string openedPrompt = "";

	[Header("Panel")]
	[SerializeField] private Animator panelAnimator;
	[SerializeField] private string panelID;

	[Header("Feedback")]
	[SerializeField] private float wrongResetDelay = 1.2f;
	[SerializeField] private UnityEvent onWrongSequence;
	[SerializeField] private UnityEvent onCorrectSequence;
	[SerializeField] private UnityEvent onPanelOpened;

	[Header("Flags")]
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;

	// ── State ─────────────────────────────────────────────────────

	private int _nextSlot = 0;
	private bool _opened = false;
	private bool _resetting = false;
	private string _feedbackPrompt = "";

	// ── Lifecycle ─────────────────────────────────────────────────

	private void Start()
	{
		if (panelAnimator != null) panelAnimator.enabled = false;
		foreach (var slot in correctSequence)
			if (slot.slotAnimator != null) slot.slotAnimator.enabled = false;

		if (GameFlags.Instance == null) return;

		if (GameFlags.Instance.GetFlag($"panel_opened_{panelID}"))
		{
			_opened = true;
			_nextSlot = correctSequence.Length;
			SnapToEnd(panelAnimator);
			foreach (var slot in correctSequence)
			{
				slot.filled = true;
				SnapToEnd(slot.slotAnimator);
			}
		}
	}

	// ── IInteractable ─────────────────────────────────────────────

	public string GetPrompt()
	{
		if (_opened) return openedPrompt;
		if (_resetting) return _feedbackPrompt;

		CardSlot next = NextEmptySlot();
		if (next == null) return idlePrompt;

		bool holdingCorrectCard = InventoryManager.Instance.items
			.Exists(i => i.usageTargetID == next.cardID);

		if (holdingCorrectCard)
			return $"[E] Insert {next.label}  ({_nextSlot}/{correctSequence.Length})";

		bool holdingAnyCard = InventoryManager.Instance.items
			.Exists(i => IsAnyCardID(i.usageTargetID));

		return holdingAnyCard
			? $"[E] Insert card  ({_nextSlot}/{correctSequence.Length})"
			: waitingPrompt;
	}

	public void Interact(Player player)
	{
		if (_opened || _resetting) return;

		CardSlot expected = NextEmptySlot();
		if (expected == null) return;

		bool holdingAnyCard = InventoryManager.Instance.items
			.Exists(i => IsAnyCardID(i.usageTargetID));

		if (!holdingAnyCard) return;

		bool correctCard = InventoryManager.Instance.items
			.Exists(i => i.usageTargetID == expected.cardID);

		if (correctCard)
		{
			// Don't consume yet — track placement only
			expected.filled = true;
			_nextSlot++;

			StartCoroutine(PlaySlotAnim(expected));

			if (_nextSlot >= correctSequence.Length)
			{
				// Sequence complete — consume all cards now
				foreach (var slot in correctSequence)
					InventoryManager.Instance.TryUseItemOnTarget(slot.cardID, out _);

				ActiveItemHolder.Clear();
				StartCoroutine(OpenAfterLastAnim());
			}
		}
		else
		{
			onWrongSequence?.Invoke();
			StartCoroutine(ResetSequence());
		}
	}

	// ── Sequence reset ────────────────────────────────────────────

	private IEnumerator ResetSequence()
	{
		_resetting = true;
		_feedbackPrompt = wrongPrompt;

		yield return new WaitForSeconds(wrongResetDelay);

		// Nothing was consumed — just reset visual state
		for (int i = 0; i < _nextSlot; i++)
		{
			var slot = correctSequence[i];
			slot.filled = false;
			if (slot.slotAnimator != null)
			{
				slot.slotAnimator.Play(slot.slotAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
				slot.slotAnimator.enabled = false;
			}
		}

		_nextSlot = 0;
		_feedbackPrompt = "";
		_resetting = false;
	}

	// ── Open panel ────────────────────────────────────────────────

	private IEnumerator OpenAfterLastAnim()
	{
		var lastSlot = correctSequence[correctSequence.Length - 1];
		if (lastSlot.slotAnimator != null && lastSlot.slotAnimator.enabled)
		{
			yield return null;
			float len = lastSlot.slotAnimator.GetCurrentAnimatorStateInfo(0).length;
			yield return new WaitForSeconds(len);
		}

		_opened = true;

		if (GameFlags.Instance != null)
		{
			GameFlags.Instance.SetFlag($"panel_opened_{panelID}");
			foreach (var flag in flagsToSet)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.SetFlag(flag);
			foreach (var flag in flagsToClear)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.ClearFlag(flag);
		}

		onCorrectSequence?.Invoke();

		if (panelAnimator != null)
			panelAnimator.enabled = true;
		else
			Debug.LogWarning($"CardSequencePanel '{panelID}': no panel Animator assigned.");

		onPanelOpened?.Invoke();
	}

	// ── Helpers ───────────────────────────────────────────────────

	private CardSlot NextEmptySlot()
	{
		if (_nextSlot >= correctSequence.Length) return null;
		return correctSequence[_nextSlot];
	}

	private bool IsAnyCardID(string id)
	{
		foreach (var slot in correctSequence)
			if (slot.cardID == id) return true;
		return false;
	}

	private IEnumerator PlaySlotAnim(CardSlot slot)
	{
		if (slot.slotAnimator == null) yield break;
		slot.slotAnimator.enabled = true;
		yield return null;
		float length = slot.slotAnimator.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(length);
	}

	private void SnapToEnd(Animator anim)
	{
		if (anim == null) return;
		anim.enabled = true;
		anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
	}
}