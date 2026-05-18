using UnityEngine;

public class PairingWorldItem : MonoBehaviour, IInteractable
{
	[Header("This Item")]
	[SerializeField] private ItemData thisItem;
	[SerializeField] private string promptMessage = "Pick up";

	[Header("Pairing")]
	[Tooltip("The partner item already in inventory. Both will be removed on pickup.")]
	[SerializeField] private ItemData partnerItem;
	[Tooltip("The combined item added when both are found. Can be a different ItemType (e.g. Clue).")]
	[SerializeField] private ItemData combinedItem;

	[Header("Fallback")]
	[Tooltip("If partner is not yet in inventory, just pick this up normally and wait.")]
	[SerializeField] private bool allowPickupWithoutPartner = false;

	[Header("Flags")]
	[SerializeField] private string[] flagsToSet;
	[SerializeField] private string[] flagsToClear;

	private bool _pickedUp = false;

	// ── Lifecycle ─────────────────────────────────────────────────

	private void Start()
	{
		if (GameFlags.Instance != null
			&& GameFlags.Instance.GetFlag("picked_up_" + thisItem.itemID))
			Destroy(gameObject);
	}

	// ── IInteractable ─────────────────────────────────────────────

	public string GetPrompt()
	{
		bool hasPartner = HasPartner();

		if (hasPartner)
			return $"[E] {promptMessage} {thisItem.itemName}";

		if (allowPickupWithoutPartner)
			return $"[E] {promptMessage} {thisItem.itemName}";

		// Partner not found yet — silently non-interactable, or show hint
		return "";
	}

	public void Interact(Player player)
	{
		if (_pickedUp) return;

		if (HasPartner())
		{
			Combine();
		}
		else if (allowPickupWithoutPartner)
		{
			PickupAlone();
		}
	}

	// ── Logic ─────────────────────────────────────────────────────

	private void Combine()
	{
		_pickedUp = true;

		// Remove the partner from inventory
		InventoryManager.Instance.RemoveItem(partnerItem);

		// Add the combined result
		if (combinedItem != null)
			InventoryManager.Instance.AddItem(combinedItem);

		// Flag both originals as consumed
		GameFlags.Instance?.SetFlag("picked_up_" + thisItem.itemID);
		GameFlags.Instance?.SetFlag("picked_up_" + partnerItem.itemID);

		// Extra flags
		if (GameFlags.Instance != null)
		{
			foreach (var flag in flagsToSet)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.SetFlag(flag);
			foreach (var flag in flagsToClear)
				if (!string.IsNullOrWhiteSpace(flag))
					GameFlags.Instance.ClearFlag(flag);
		}

		Destroy(gameObject);
	}

	private void PickupAlone()
	{
		_pickedUp = true;
		GameFlags.Instance?.SetFlag("picked_up_" + thisItem.itemID);
		InventoryManager.Instance.AddItem(thisItem);
		Destroy(gameObject);
	}

	private bool HasPartner()
	{
		if (partnerItem == null) return false;
		return InventoryManager.Instance.items.Exists(i => i.itemID == partnerItem.itemID);
	}
}