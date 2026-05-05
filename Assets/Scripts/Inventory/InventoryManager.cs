using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance { get; private set; }

	public List<ItemData> items = new List<ItemData>(); // usable items
	public List<ItemData> files = new List<ItemData>(); // readable notes
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
			case ItemType.Item: items.Add(data); break;
			case ItemType.File: files.Add(data); break;
			case ItemType.Clue: clues.Add(data); break;
		}
		OnInventoryChanged?.Invoke();
		Debug.Log($"Picked up: {data.itemName}");
	}

	public void RemoveItem(ItemData data)
	{
		items.Remove(data);
		files.Remove(data);
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