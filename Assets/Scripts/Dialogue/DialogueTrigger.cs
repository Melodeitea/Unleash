using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
	[SerializeField] private string prompt = "Talk";
	[SerializeField] private DialogueSequence sequence;

	[Header("Optional Reward")]
	//[SerializeField] private InventoryItem itemToGiveOnComplete;

	[Header("Events")]
	[SerializeField] private UnityEvent onSequenceComplete;

	public void Interact(Player player)
	{
		Debug.Log("[DialogueTrigger] Interact called.");
		Debug.Log($"[DialogueTrigger] sequence assigned: {sequence != null}");
		if (sequence != null)
		{
			Debug.Log($"[DialogueTrigger] lines count: {sequence.lines?.Length ?? 0}");
			Debug.Log($"[DialogueTrigger] triggerOnce={sequence.triggerOnce}, hasPlayed={sequence.hasPlayed}");
		}
		Debug.Log($"[DialogueTrigger] DialogueManager.Instance: {DialogueManager.Instance != null}");

		DialogueManager.Instance.PlaySequence(sequence, OnComplete);
	}

	private void OnComplete()
	{
		//if (itemToGiveOnComplete != null)
		//{
		//	InventoryManager.Instance.AddItem(itemToGiveOnComplete);
		//}

		//onSequenceComplete?.Invoke();
	}

	public string GetPrompt()
	{
		return prompt;
	}
}