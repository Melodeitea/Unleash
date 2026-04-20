using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
	[Header("Identity")]
	[Tooltip("Stable item identifier (auto-generated). Use this id in PuzzleSystem definitions.")]
	[SerializeField]
	string itemId;
	public string ItemId => itemId;

	[Tooltip("Human readable name")]
	public string displayName = "New Item";

	[Tooltip("Tag used for quick matching in editors or level tools (optional)")]
	public string itemTag;

	[Header("Presentation")]
	[Tooltip("3D prefab used in the world (pickup model, placed object)")]
	public GameObject prefab3D;
	[Tooltip("2D icon used in the inventory UI")]
	public Sprite icon2D;
	[TextArea(3, 6)]
	public string description;

	[Header("Gameplay")]
	[Tooltip("Can the item be stacked in inventory?")]
	public bool stackable = false;
	[Tooltip("Maximum stack size (1 if not stackable)")]
	[Min(1)]
	public int maxStack = 1;
	[Tooltip("If true, item is consumed on use")]
	public bool consumable = false;
	[Tooltip("Optional pickup sound")]
	public AudioClip pickupSound;
	[Tooltip("Optional weight value for inventory limits")]
	public float weight = 0f;

	// Editor-only: ensure each asset has a stable id
	void OnValidate()
	{
#if UNITY_EDITOR
		if (string.IsNullOrEmpty(itemId))
			itemId = Guid.NewGuid().ToString();
#endif
		if (!stackable) maxStack = 1;
	}

	// Convenience: match by tag or id
	public bool MatchesTag(string tag)
	{
		if (string.IsNullOrEmpty(tag)) return false;
		if (!string.IsNullOrEmpty(itemTag) && itemTag == tag) return true;
		return itemId == tag;
	}
}