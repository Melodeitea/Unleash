using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
	[SerializeField] private ItemData itemData;
	[SerializeField] private string promptMessage = "Pick up";

	public string GetPrompt() => $"[E] {promptMessage} {itemData.itemName}";

	public void Interact(Player player)
	{
		InventoryManager.Instance.AddItem(itemData);
		Destroy(gameObject);
	}
}