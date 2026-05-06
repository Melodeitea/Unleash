using System.Collections.Generic;
using UnityEngine;

public class ScrollListUI : MonoBehaviour
{
	[SerializeField] private ItemType listType;       // set to File or Clue in Inspector
	[SerializeField] private Transform content;       // the Content inside ScrollView
	[SerializeField] private GameObject slotPrefab;
	[SerializeField] private InventoryUI inventoryUI;

	private void OnEnable()
	{
		InventoryManager.Instance.OnInventoryChanged += Refresh;
		Refresh();
	}

	private void OnDisable()
	{
		if (InventoryManager.Instance != null)
			InventoryManager.Instance.OnInventoryChanged -= Refresh;
	}

	private void Refresh()
	{
		foreach (Transform child in content)
			Destroy(child.gameObject);

		List<ItemData> list = listType == ItemType.File
			? InventoryManager.Instance.files
			: InventoryManager.Instance.clues;

		foreach (var item in list)
		{
			var go = Instantiate(slotPrefab, content);
			var slot = go.GetComponent<NoteSlotUI>();
			slot.Setup(item, inventoryUI.ShowDetail);
		}
	}
}