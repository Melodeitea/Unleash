using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance { get; private set; }

	public List<ItemData> items = new List<ItemData>(); // usable items
	public List<ItemData> notes = new List<ItemData>(); // readable notes
	public List<ItemData> clues = new List<ItemData>(); // audio clues

	public System.Action OnInventoryChanged;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
	}

	public void AddItem(ItemData data)
	{
		switch (data.itemType)
		{
			case ItemType.Items: items.Add(data); break;
			case ItemType.Notes: notes.Add(data); break;
			case ItemType.Clues: clues.Add(data); break;
		}
		OnInventoryChanged?.Invoke();
		Debug.Log($"Picked up: {data.itemName}");
	}

	public void LoadInventory(List<string> ids, ItemDatabase db)
	{
		items.Clear();
		notes.Clear();
		clues.Clear();

		foreach (string id in ids)
		{
			ItemData data = db.Get(id);
			if (data != null)
				AddItem(data); // reuses your existing switch + event
			else
				Debug.LogWarning($"[InventoryManager] Could not find item with ID '{id}' in database.");
		}
	}

	public void RemoveItem(ItemData data)
	{
		items.Remove(data);
		notes.Remove(data);
		clues.Remove(data);
		OnInventoryChanged?.Invoke();
	}

	// Called by UsageTarget — checks if player holds the right item
	public bool TryUseItemOnTarget(string targetID, out ItemData usedItem)
	{
		usedItem = items.Find(i => i.usageTargetID == targetID);
		if (usedItem != null)
		{
			if (usedItem.consumeOnUse) RemoveItem(usedItem);
			return true;
		}
		return false;
	}
}