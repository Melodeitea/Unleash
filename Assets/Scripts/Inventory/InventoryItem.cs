using UnityEngine;

[CreateAssetMenu(menuName = "Unleash/Inventory Item")]
public class InventoryItem : ScriptableObject
{
	[Header("Identity")]
	public string id;
	public string displayName;
	[TextArea] public string description;
	public Sprite icon;

	[Header("Note / Document")]
	[Tooltip("If true, this item goes to the Notes panel instead of Inventory")]
	public bool isNote;
	[TextArea(5, 20)]
	public string transcriptionText;
	[Tooltip("Optional prefab (2D plane or 3D object) shown on the left of the reader")]
	public GameObject visualPrefab;
	public Vector3 visualPositionOffset;
	public Vector3 visualRotationOffset;
	public Vector3 visualScale = Vector3.one;
}