using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
	[SerializeField] private string prompt = "Talk";
	[SerializeField] private DialogueSequence sequence;

	[Header("Optional Reward")]
	[SerializeField] private InventoryItem itemToGiveOnComplete;

	[Header("Events")]
	[SerializeField] private UnityEvent onSequenceComplete;

	public void Interact(Player player)
	{
		DialogueManager.Instance.PlaySequence(sequence, OnComplete);
	}

	private void OnComplete()
	{
		if (itemToGiveOnComplete != null)
		{
			InventoryManager.Instance.AddItem(itemToGiveOnComplete);
		}

		onSequenceComplete?.Invoke();
	}

	public string GetPrompt()
	{
		return prompt;
	}
}