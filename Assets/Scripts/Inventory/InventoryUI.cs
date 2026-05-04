using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
	public static InventoryUI Instance { get; private set; }

	[Header("Panel")]
	public GameObject panelRoot;

	[Header("Grid")]
	public Transform itemGrid;
	public GameObject itemSlotPrefab; // see slot prefab section below

	[Header("Controls")]
	public KeyCode toggleKey = KeyCode.I;

	void Awake()
	{
		if (Instance != null && Instance != this) { Destroy(gameObject); return; }
		Instance = this;
	}

	void Start()
	{
		panelRoot.SetActive(false);
		InventoryManager.Instance.OnInventoryChanged.AddListener(Refresh);
	}

	void Update()
	{
		if (Input.GetKeyDown(toggleKey))
			Toggle();
	}

	public void Toggle()
	{
		bool next = !panelRoot.activeSelf;
		panelRoot.SetActive(next);
		if (next) Refresh();
	}

	public void Open() { panelRoot.SetActive(true); Refresh(); }
	public void Close() { panelRoot.SetActive(false); }

	void Refresh()
	{
		foreach (Transform child in itemGrid)
			Destroy(child.gameObject);

		foreach (InventoryItem item in InventoryManager.Instance.GetAll())
		{
			GameObject slot = Instantiate(itemSlotPrefab, itemGrid);
			slot.GetComponentInChildren<Image>().sprite = item.icon;
			slot.GetComponentInChildren<TextMeshProUGUI>().text = item.displayName;
		}
	}
}