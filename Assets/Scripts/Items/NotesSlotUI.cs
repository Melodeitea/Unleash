using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NoteSlotUI : MonoBehaviour
{
	private Button button;

	[SerializeField] private Image icon;
	[SerializeField] private TextMeshProUGUI titleText;

	private ItemData item;

	private void Awake()
	{
		button = GetComponent<Button>();
	}

	public void Setup(ItemData data, Action<ItemData> onClick)
	{
		item = data;

		titleText.text = data.itemName;
		icon.sprite = data.icon;

		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => onClick(item));
	}
}