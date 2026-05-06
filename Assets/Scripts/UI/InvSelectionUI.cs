using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InvSelectionUI : MonoBehaviour
{
	[System.Serializable]
	public class ItemSlot
	{
		public Button button;
		public Image icon;
		public GameObject emptyIndicator; // optional grey X or blank image
	}

	[Header("The 8 pre-placed buttons")]
	[SerializeField] private ItemSlot[] slots;   // drag all 8 in here

	[Header("References")]
	[SerializeField] private InventoryUI inventoryUI; // to call ShowDetail

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
		var items = InventoryManager.Instance.items;

		for (int i = 0; i < slots.Length; i++)
		{
			bool hasItem = i < items.Count;
			ItemData data = hasItem ? items[i] : null;

			// Show or hide icon
			slots[i].icon.gameObject.SetActive(hasItem);
			if (hasItem) slots[i].icon.sprite = data.icon;

			// Show empty state
			if (slots[i].emptyIndicator != null)
				slots[i].emptyIndicator.SetActive(!hasItem);

			// Wire click — capture i in local var to avoid closure bug
			int index = i;
			slots[i].button.onClick.RemoveAllListeners();
			// InvSelectionUI.cs — in Refresh(), update the listener
			slots[i].button.onClick.AddListener(() =>
			{
				if (index < InventoryManager.Instance.items.Count)
					inventoryUI.ShowDetail(InventoryManager.Instance.items[index]);
				else
					inventoryUI.ClearDetail(); // ← add this
			});
		}
	}
}