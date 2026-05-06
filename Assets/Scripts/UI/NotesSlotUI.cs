using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NoteSlotUI : MonoBehaviour
{
	[SerializeField] private Button button;
	[SerializeField] private TextMeshProUGUI titleText;

	private ItemData item;

	public void Setup(ItemData data, Action<ItemData> onClick)
	{
		item = data;
		titleText.text = data.itemName; // or whatever your field is

		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => onClick(item));
	}
}