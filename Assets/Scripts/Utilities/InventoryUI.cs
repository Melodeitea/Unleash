using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Bridge between InventorySystem and UI. Attach to an Inventory UI root.
// Configure exactly 8 slots in the inspector (Slot UI prefabs with Image + Button + TMP message).
public class InventoryUI : MonoBehaviour
{
    [Serializable]
    public class SlotUI
    {
        public Image icon;
        public Button useButton;
        public TextMeshProUGUI messageText;
        public GameObject emptyOverlay;
    }

    [Header("References")]
    public InventorySystem inventory;
    public SlotUI[] slots = new SlotUI[8];

    void Start()
    {
        if (inventory == null)
            inventory = FindObjectOfType<InventorySystem>();

        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: No InventorySystem found in scene.");
            return;
        }

        // wire up events
        inventory.OnItemAdded += OnItemAdded;
        inventory.OnItemRemoved += OnItemRemoved;
        inventory.OnItemUsed += OnItemUsed;

        // wire button callbacks and refresh UI
        for (int i = 0; i < slots.Length; i++)
        {
            int idx = i;
            var s = slots[i];
            if (s != null && s.useButton != null)
            {
                s.useButton.onClick.RemoveAllListeners();
                s.useButton.onClick.AddListener(() => OnUseButtonClicked(idx));
            }
        }

        RefreshAll();
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnItemAdded -= OnItemAdded;
            inventory.OnItemRemoved -= OnItemRemoved;
            inventory.OnItemUsed -= OnItemUsed;
        }
    }

    void OnUseButtonClicked(int slotIndex)
    {
        // Default behavior: use item and show returned message in the slot's messageText.
        var msg = inventory.UseItemAt(slotIndex);
        ShowSlotMessage(slotIndex, msg);
        RefreshSlot(slotIndex);
    }

    // Public API to use item on a target (call from your interaction code)
    public void UseItemOnTarget(int slotIndex, string targetPuzzleId)
    {
        var msg = inventory.UseItemOnTarget(slotIndex, targetPuzzleId);
        ShowSlotMessage(slotIndex, msg);
        RefreshSlot(slotIndex);
    }

    void OnItemAdded(int slotIndex, string itemId) => RefreshSlot(slotIndex);
    void OnItemRemoved(int slotIndex, string itemId) => RefreshSlot(slotIndex);
    void OnItemUsed(int slotIndex, string message) => ShowSlotMessage(slotIndex, message);

    void RefreshAll()
    {
        for (int i = 0; i < slots.Length; i++) RefreshSlot(i);
    }

    void RefreshSlot(int i)
    {
        if (i < 0 || i >= slots.Length) return;
        var s = slots[i];
        if (s == null) return;

        var item = inventory.GetItemAt(i);
        if (item != null)
        {
            s.icon.sprite = item.icon2D;
            s.icon.enabled = item.icon2D != null;
            s.emptyOverlay?.SetActive(false);
            s.useButton.interactable = true;
            s.messageText.text = ""; // clear message until used (you can persist messages if desired)
        }
        else
        {
            s.icon.sprite = null;
            s.icon.enabled = false;
            s.emptyOverlay?.SetActive(true);
            s.useButton.interactable = false;
            // keep last message or clear
            // s.messageText.text = "";
        }
    }

    void ShowSlotMessage(int slotIndex, string message)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;
        var s = slots[slotIndex];
        if (s == null || s.messageText == null) return;

        s.messageText.text = message;
        // Optional: start a coroutine to clear message after delay (not implemented)
    }
}
