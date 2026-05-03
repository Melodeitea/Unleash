using UnityEngine;

[CreateAssetMenu(menuName = "Unleash/Inventory Item")]
public class InventoryItem : ScriptableObject
{
	public string id;
	public string displayName;
	[TextArea] public string description;
	public Sprite icon;
}