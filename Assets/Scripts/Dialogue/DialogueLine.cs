using UnityEngine;

[CreateAssetMenu(menuName = "Unleash/Dialogue Line")]
public class DialogueLine : ScriptableObject
{
	public string speakerName;
	[TextArea] public string line;
}