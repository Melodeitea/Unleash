// ItemDatabase.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
	[SerializeField] private List<ItemData> allItems = new();

	private Dictionary<string, ItemData> _lookup;

	// Wherever you call LoadPlayer() and apply the data:
	[SerializeField] private ItemDatabase itemDatabase; // drag your DB asset here

	void ApplySave(PlayerData data, Player player)
	{
		// ... your existing position/flashlight restore ...

		// Restore inventory
		if (data.inventoryItemIds != null && data.inventoryItemIds.Count > 0)
		{
			itemDatabase.Init();
			InventoryManager.Instance.LoadInventory(data.inventoryItemIds, itemDatabase);
		}

		// Restore flags
		if (data.gameFlags != null)
		{
			foreach (string flag in data.gameFlags)
				GameFlags.Instance.SetFlag(flag);
		}
	}
	public void Init()
	{
		_lookup = new Dictionary<string, ItemData>();
		foreach (var item in allItems)
		{
			if (!string.IsNullOrEmpty(item.itemID))
				_lookup[item.itemID] = item;
			else
				Debug.LogWarning($"[ItemDatabase] Item '{item.itemName}' has no itemID — it won't be restorable.");
		}
	}

	public ItemData Get(string id)
	{
		if (_lookup == null) Init();
		_lookup.TryGetValue(id, out var result);
		return result;
	}
}