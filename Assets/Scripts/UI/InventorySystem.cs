using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Fixed number of slots (Resident Evil style = 8)")]
    public int slotCount = 8;

    [Tooltip("List of known ItemData assets to resolve item ids at runtime.")]
    public List<ItemData> itemDatabase = new List<ItemData>();

    // internal: itemId per slot (null or empty = empty slot)
    [SerializeField]
    string[] slots;

    // Events
    public event Action<int, string> OnItemAdded;      // slotIndex, itemId
    public event Action<int, string> OnItemRemoved;    // slotIndex, itemId
    public event Action<int, string> OnItemUsed;       // slotIndex, message

    void Awake()
    {
        if (slotCount < 1) slotCount = 8;
        if (slots == null || slots.Length != slotCount)
            slots = new string[slotCount];
    }

    // Public API

    // Try add an item by ItemData (non-stackable, place first empty slot). Returns slot index or -1.
    public int AddItem(ItemData item)
    {
        if (item == null) return -1;
        return AddItemById(item.ItemId);
    }

    // Try add an item by id. Returns slot index or -1 if full.
    public int AddItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (string.IsNullOrEmpty(slots[i]))
            {
                slots[i] = itemId;
                OnItemAdded?.Invoke(i, itemId);
                return i;
            }
        }
        return -1; // inventory full
    }

    // Remove item at slot (no use). Returns removed id or null.
    public string RemoveItemAt(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return null;
        var id = slots[slotIndex];
        if (string.IsNullOrEmpty(id)) return null;
        slots[slotIndex] = null;
        OnItemRemoved?.Invoke(slotIndex, id);
        return id;
    }

    // Use item at slot (consumes it). Returns message to display.
    public string UseItemAt(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return "Invalid slot.";
        var id = slots[slotIndex];
        if (string.IsNullOrEmpty(id)) return "Slot empty.";

        var item = GetItemData(id);
        string msg;
        if (item != null)
            msg = $"You used the {item.displayName}.";
        else
            msg = "You used an item.";

        // Consume / remove item
        slots[slotIndex] = null;
        OnItemUsed?.Invoke(slotIndex, msg);
        OnItemRemoved?.Invoke(slotIndex, id);
        return msg;
    }

    // Try to use an item at slot on a target puzzle id.
    // If PuzzleSystem exists and the targetId is non-empty, we mark that puzzle solved and consume the item.
    // Returns message to display (success/fail).
    public string UseItemOnTarget(int slotIndex, string targetPuzzleId)
    {
        if (!IsValidSlot(slotIndex)) return "Invalid slot.";
        var id = slots[slotIndex];
        if (string.IsNullOrEmpty(id)) return "Slot empty.";

        var item = GetItemData(id);
        // Basic game logic: consume and inform PuzzleSystem that puzzle was solved by this item.
        if (!string.IsNullOrEmpty(targetPuzzleId))
        {
            var ps = FindObjectOfType<PuzzleSystem>();
            if (ps != null)
            {
                // inform puzzle system the target is solved (designer can keep logic in PuzzleSystem)
                ps.ApplySolvedIds(new[] { targetPuzzleId });

                // consume item
                slots[slotIndex] = null;
                OnItemUsed?.Invoke(slotIndex, $"You used the {item?.displayName ?? id} on {targetPuzzleId}.");
                OnItemRemoved?.Invoke(slotIndex, id);
                return $"You used the {item?.displayName ?? id} on {targetPuzzleId}.";
            }
            // no puzzle system found — fallback message
            slots[slotIndex] = null;
            OnItemUsed?.Invoke(slotIndex, $"You used the {item?.displayName ?? id} on {targetPuzzleId}.");
            OnItemRemoved?.Invoke(slotIndex, id);
            return $"You used the {item?.displayName ?? id}.";
        }

        // default fallback to simple UseItemAt
        return UseItemAt(slotIndex);
    }

    // Query
    public string GetItemIdAt(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return null;
        return slots[slotIndex];
    }

    public ItemData GetItemAt(int slotIndex)
    {
        var id = GetItemIdAt(slotIndex);
        return GetItemData(id);
    }

    // Helper to resolve ItemData via local database
    public ItemData GetItemData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            if (itemDatabase[i] != null && itemDatabase[i].ItemId == itemId)
                return itemDatabase[i];
        }
        return null;
    }

    bool IsValidSlot(int i) => i >= 0 && i < slots.Length;

    // For UI: expose snapshot
    public IReadOnlyList<string> GetAllItemIds() => Array.AsReadOnly(slots);
}
