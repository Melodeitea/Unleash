using UnityEngine;

[CreateAssetMenu(menuName = "Documents/Content")]
public class DocumentContentSO : ScriptableObject
{
	[TextArea(5, 20)]
	public string transcriptionText;
}