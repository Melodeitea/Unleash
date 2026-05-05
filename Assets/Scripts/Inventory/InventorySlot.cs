using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
	[SerializeField] private Image icon;
	[SerializeField] private TextMeshProUGUI label;
	private ItemData _data;
	private System.Action<ItemData> _onClick;

	public void Setup(ItemData data, System.Action<ItemData> onClick)
	{
		_data = data;
		_onClick = onClick;
		icon.sprite = data.icon;
		label.text = data.itemName;
	}

	public void OnClick() => _onClick?.Invoke(_data);
}