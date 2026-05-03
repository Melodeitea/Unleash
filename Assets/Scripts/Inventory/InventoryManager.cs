using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance { get; private set; }

	[SerializeField] private List<InventoryItem> startingItems = new();

	private List<InventoryItem> items = new();

	public UnityEvent OnInventoryChanged;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		items = new List<InventoryItem>(startingItems);
	}

	public void AddItem(InventoryItem item)
	{
		if (item == null) return;

		items.Add(item);
		OnInventoryChanged?.Invoke();
	}

	public bool HasItem(string id)
	{
		return items.Exists(i => i.id == id);
	}

	public void RemoveItem(string id)
	{
		InventoryItem item = items.Find(i => i.id == id);
		if (item != null)
		{
			items.Remove(item);
			OnInventoryChanged?.Invoke();
		}
	}

	public List<InventoryItem> GetAll()
	{
		return new List<InventoryItem>(items);
	}

	public void CombineItems(string id1, string id2, InventoryItem result)
	{
		if (HasItem(id1) && HasItem(id2))
		{
			RemoveItem(id1);
			RemoveItem(id2);
			AddItem(result);
		}
	}

	// --- SAVE SYSTEM ---
	public void SaveToPlayerPrefs()
	{
		List<string> ids = new();
		foreach (var item in items)
			ids.Add(item.id);

		string data = string.Join(",", ids);
		PlayerPrefs.SetString("inventory", data);
	}

	public void LoadFromPlayerPrefs()
	{
		items.Clear();

		string data = PlayerPrefs.GetString("inventory", "");
		if (string.IsNullOrEmpty(data)) return;

		string[] ids = data.Split(',');

		foreach (string id in ids)
		{
			InventoryItem item = FindItemById(id);
			if (item != null)
				items.Add(item);
		}

		OnInventoryChanged?.Invoke();
	}

	private InventoryItem FindItemById(string id)
	{
		// ⚠ You will replace this later with a proper database
		InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("");

		foreach (var item in allItems)
		{
			if (item.id == id)
				return item;
		}

		return null;
	}
}