using UnityEngine;
public class WorldItem : MonoBehaviour, IInteractable
{
	[SerializeField] private ItemData itemData;
	[SerializeField] private string promptMessage = "Pick up";

	private void Start()
	{
		// If already picked up in a previous session, remove from world
		if (GameFlags.Instance != null && GameFlags.Instance.GetFlag("picked_up_" + itemData.itemID))
			Destroy(gameObject);
	}

	public string GetPrompt() => $"[E] {promptMessage} {itemData.itemName}";

	public void Interact(Player player)
	{
		GameFlags.Instance?.SetFlag("picked_up_" + itemData.itemID);
		InventoryManager.Instance.AddItem(itemData);
		Destroy(gameObject);
	}
}