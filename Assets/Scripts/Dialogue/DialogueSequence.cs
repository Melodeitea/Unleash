using UnityEngine;

[CreateAssetMenu(menuName = "Unleash/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
	public DialogueLine[] lines;
	public bool triggerOnce = false;

	[HideInInspector] public bool hasPlayed = false;
}