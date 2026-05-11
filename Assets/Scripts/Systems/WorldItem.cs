using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;

    [SerializeField] private string promptMessage = "Pick up";

    [Header("Auto Pickup")]
    [SerializeField] private bool autoPickup;

    [Tooltip("Only required if Auto Pickup is enabled.")]
    [SerializeField] private bool destroyAfterPickup = true;

    private bool _pickedUp;

    private void Start()
    {
        // Already collected previously
        if (GameFlags.Instance != null &&
            GameFlags.Instance.GetFlag("picked_up_" + itemData.itemID))
        {
            Destroy(gameObject);
        }
    }

    public string GetPrompt()
    {
        if (autoPickup)
            return "";

        return $"[E] {promptMessage} {itemData.itemName}";
    }

    public void Interact(Player player)
    {
        if (autoPickup)
            return;

        Pickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoPickup || _pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        Pickup();
    }

    private void Pickup()
    {
        if (_pickedUp)
            return;

        _pickedUp = true;

        GameFlags.Instance?.SetFlag("picked_up_" + itemData.itemID);

        InventoryManager.Instance.AddItem(itemData);

        if (destroyAfterPickup)
            Destroy(gameObject);
    }
}