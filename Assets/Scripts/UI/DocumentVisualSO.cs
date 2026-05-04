using UnityEngine;

[CreateAssetMenu(menuName = "Documents/Visual")]
public class DocumentVisualSO : ScriptableObject
{
	[Tooltip("Prefab of a 3D object or plane shown in UI")]
	public GameObject visualPrefab;

	//[Header("Mesh Data")]
	//public Mesh mesh;
	//public Material material;

	public Vector3 localPositionOffset;
	public Vector3 localRotationOffset;
	public Vector3 localScale = Vector3.one;
}