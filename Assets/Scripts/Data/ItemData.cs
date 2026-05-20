using UnityEngine;

public enum ItemType { Items, Notes, Clues }

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
	[Header("Identity")]
	public string itemID;           // unique ID e.g. "key_library"
	public string itemName;
	public ItemType itemType;       // Item = usable, File = readable, Clue = audio

	[Header("Display")]
	public Sprite icon;
	[TextArea] public string description;   // shown in right panel

	[Header("File Content")]
	[TextArea(5, 20)] public string fileText;   // for Files tab

	[Header("Clue Audio")]
	public AudioClip audioClip;     // for Clues tab

	[Header("Usage")]
	public string usageTargetID;    // ID of world object this item unlocks
	public bool consumeOnUse = true;
}