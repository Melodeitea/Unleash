using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NotesManager : MonoBehaviour
{
	public static NotesManager Instance { get; private set; }

	private List<InventoryItem> notes = new();
	public UnityEvent OnNotesChanged;

	void Awake()
	{
		if (Instance != null && Instance != this) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void AddNote(InventoryItem item)
	{
		if (item == null || notes.Contains(item)) return;
		notes.Add(item);
		OnNotesChanged?.Invoke();
	}

	public List<InventoryItem> GetAll() => new List<InventoryItem>(notes);
	public bool HasNote(string id) => notes.Exists(n => n.id == id);
}